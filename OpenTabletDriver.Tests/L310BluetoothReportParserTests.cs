using System.Numerics;
using OpenTabletDriver.Configurations.Parsers.Huion;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class L310BluetoothReportParserTests
    {
        private readonly L310BluetoothReportParser parser = new();

        [Theory]
        [InlineData(0, 32000, 0, 0)]
        [InlineData(0, 0, 32000, 0)]
        [InlineData(20000, 32000, 0, 20000)]
        [InlineData(20000, 0, 32000, 20000)]
        [InlineData(10000, 16000, 16000, 10000)]
        public void MapsPhysicalAxesAndKeepsHoverInRange(int rawX, int rawY, int x, int y)
        {
            byte[] raw = [2, 0xc0, (byte)rawX, (byte)(rawX >> 8), (byte)rawY, (byte)(rawY >> 8), 0, 0, 0, 0];
            var report = Assert.IsType<L310BluetoothTabletReport>(parser.Parse(raw));

            Assert.Equal(new Vector2(x, y), report.Position);
            Assert.Equal(0u, report.Pressure);
            Assert.Same(raw, report.Raw);
        }

        [Theory]
        [InlineData(0xc1, false, false)]
        [InlineData(0xc3, true, false)]
        [InlineData(0xc5, false, true)]
        [InlineData(0xc7, true, true)]
        public void DecodesPressureAndIndependentSideButtons(byte flags, bool first, bool second)
        {
            byte[] raw = [2, flags, 0, 0, 0, 0, 0xff, 0x3f, 0, 0];
            var report = Assert.IsType<L310BluetoothTabletReport>(parser.Parse(raw));

            Assert.Equal(16383u, report.Pressure);
            Assert.Equal(new[] { first, second }, report.PenButtons);
        }

        [Fact]
        public void MissingInRangeBitProducesOutOfRange()
        {
            Assert.IsType<OutOfRangeReport>(parser.Parse([2, 0x80, 0, 0, 0, 0, 0, 0, 0, 0]));
        }

        [Fact]
        public void UnrecognizedReportsRemainGeneric()
        {
            Assert.IsType<DeviceReport>(parser.Parse([]));
            Assert.IsType<DeviceReport>(parser.Parse([2, 0xc0]));
            Assert.IsType<DeviceReport>(parser.Parse([3, 0xc0, 0, 0, 0, 0, 0, 0, 0, 0]));
        }
    }
}
