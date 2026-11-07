# Vendored HidSharpCore

Source: https://github.com/OpenTabletDriver/HidSharpCore

Base commit: `bca4aee1c349985b0119eed24aba83e2dd63492d` (HidSharpCore package version 1.3.0).
The upstream `HidSharp` sources, `LICENSE`, and `NOTICE.txt` are included here so a
normal clone can build the macOS L310 backend without a separate repository or
locally generated DLL. Assembly identity remains `HidSharpCore, Version=2.1.0.0`.

Local modifications:

- `MacHidManager.cs`: append the IORegistry entry ID to HID device paths, because
  BLE IOHIDUserDevice instances can share an IOService path.
- `NativeMethods.cs`: reopen those paths by registry entry ID; retain the legacy
  path lookup for paths without the suffix; declare `SeizeDevice`.
- `MacHidStream.cs`: open and close only Huion VID `0x256c` / PID `0x8251` with
  `SeizeDevice`, preventing the system's native pointer from competing with OTD.
  All other devices retain shared-open behavior.
- `HidSharp.csproj`: target the enclosing OTD repository's `FrameworkBase`.

The modified macOS source files carry local-change notices. Other upstream source files are unchanged apart from trailing-whitespace cleanup in
Device.cs, DeviceStream.cs, OpenOption.cs, HidDeviceInputReceiver.cs, and LibusbHidManager.cs. HidSharpCore is licensed under Apache-2.0; see the
included license and notice. OTD's own license is unchanged.
