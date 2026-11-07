using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion;

// Observed BLE HID reports: ID 2, 10 bytes. C0 means hover, not out of range.
public sealed class L310BluetoothReportParser : IReportParser<IDeviceReport>
{
    public IDeviceReport Parse(byte[] data)
    {
        if (data.Length != 10 || data[0] != 0x02)
            return new DeviceReport(data);

        // Bit 7 is set in actual C0/C1 reports despite descriptor constant bits.
        // Use only the observed InRange bit; do not compare the entire flags byte.
        if ((data[1] & 0x40) == 0)
            return new OutOfRangeReport(data);

        return new L310BluetoothTabletReport(data);
    }
}

public struct L310BluetoothTabletReport : ITabletReport
{
    public L310BluetoothTabletReport(byte[] data)
    {
        Raw = data;
        // Rechecked after the hardware area-mode switch with separate labeled strokes:
        // moving right decreases raw Y; moving down increases raw X.
        // This axis conversion is still required in the center-aligned mode.
        // Area-mode scaling is controlled by the tablet, not a screen-center offset here.
        // Keep Raw unchanged so the original coordinate fields remain traceable.
        Position = new Vector2(32000 - (data[4] | data[5] << 8), data[2] | data[3] << 8);
        Pressure = (uint)(data[6] | data[7] << 8);
        PenButtons = new[] { (data[1] & 0x02) != 0, (data[1] & 0x04) != 0 };
    }

    public byte[] Raw { get; set; }
    public Vector2 Position { get; set; }
    public uint Pressure { get; set; }
    public bool[] PenButtons { get; set; }
}
