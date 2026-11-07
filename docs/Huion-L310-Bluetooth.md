# Huion L310 Bluetooth support in this fork

This fork adds macOS Bluetooth support for the **Huion L310 / Inspiroy Frego S**
and a native **Pen / Eraser Toggle** binding. This is a fork build,
not an official OpenTabletDriver release. The existing profile name
`Huion L310 (Bluetooth experimental)` is retained for settings compatibility.
The L610/Frego M and USB transport have not been validated by this work.

## Using the tablet

1. Pair the L310 with macOS over Bluetooth and open OpenTabletDriver.
2. Grant OpenTabletDriver **Input Monitoring** and **Accessibility** access when
   requested. A newly signed build may require refreshing those existing grants
   and restarting the app.
3. Configure the display and tablet area for your setup in **Output**. Do not
   import another user's screen dimensions or rotation settings.
4. To use a pen side button as a tool toggle, select **Pen / Eraser Toggle** in
   **Pen Settings**, then Apply and Save. Press once to select the native eraser;
   press again to return to the pen. Releasing the button or lifting the pen
   keeps the selected state. Both buttons can share the same toggle state.

The new binding sends macOS tablet proximity events. It does not send keyboard
shortcuts or generate a tip click when a side button is pressed. The drawing
application must support native tablet erasers. **Adaptive Binding: Eraser**
retains its upstream contact-binding behavior and is not the toggle binding.

If the tablet center maps noticeably off-center, check the tablet's hardware
working-area mode. Quickly pressing the Bluetooth pairing button twice switches
the supported working-area modes. This fork does not add an arbitrary center
offset. The tablet's coordinate orientation still requires the parser's axis
conversion in the tested center-aligned mode.

Do not run the Huion driver and OTD against the tablet at the same time. OTD
seizes only this Bluetooth VID/PID on macOS so the native pointer cannot produce
a second competing cursor. The original vendor driver is not needed at runtime.

## Building from a fresh clone

Requirements: macOS, .NET 10 SDK, fish, and Xcode Command Line Tools. The patched
HidSharpCore source and its license are included in `ThirdParty/HidSharpCore`;
no external workspace paths, binary plugins, or Git submodules are required.

```fish
git clone git@github.com:XY-cpp/OpenTabletDriver.git
cd OpenTabletDriver
dotnet test OpenTabletDriver.Tests/OpenTabletDriver.Tests.csproj --configuration Release
fish eng/fish/build-l310-macos.fish
```

The self-contained Apple Silicon app is written to
`dist/l310-osx-arm64/OpenTabletDriver.app` and uses an ad hoc local signature.
It is not Developer ID signed or notarized. Copy it to Applications to install;
the script only builds and does not overwrite an installed app or its settings.
An `osx-x64` script argument is available, but Intel hardware has not been tested.

Do not replace this fork with the official in-app updater if you need these
changes: the upstream release does not include this fork's L310 support.

## Implementation

- BLE HID VID `0x256c`, PID `0x8251`, report ID `0x02`, length 10.
- Coordinates: `X = 32000 - rawY`, `Y = rawX`; active area 160 × 100 mm.
- Pressure: 0–16383; flags bit 6 is in-range, bits 1 and 2 are the side buttons.
  `0xc0` is a valid hover report. Captured bit 7 contradicts the descriptor's
  constant-bit declaration and is not used to reject input.
- The parser does not advertise tilt or a physical eraser end that was not
  observed in the reports.
- The macOS backend reopens the exact IORegistry HID entry instead of an
  ambiguous IOService path. Only the L310 BLE VID/PID uses exclusive access.
- The toggle belongs to the output pointer, so two bindings share one state.
  Hardware pen reports cannot reset the software toggle on every frame. Tool
  changes emit an old-tool exit followed by a new-tool entry.

## Validation and limits

The earlier integrated build was tested on an Apple Silicon Mac with a physical
L310: stable cursor, direction, center mapping, tip clicks, Photoshop pressure
drawing, and native eraser/pen switching in Obsidian Excalidraw were confirmed by
the user. A replay checked 7,190 captured reports with 50,341 assertions. A
separate native-event observer verified Pen exit → Eraser enter → Eraser exit →
Pen enter using CoreGraphics and AppKit.

This repository includes parser regression tests for hover, axis endpoints,
pressure, both side buttons, out-of-range and unrelated reports, plus binding
tests for press edges and shared state. Historical raw captures and local user
settings are not published. Sleep/reconnect, hardware area-mode persistence,
Intel macOS, and this Bluetooth device on Windows/Linux remain unverified.

For the repository integration on 2026-11-07, the self-contained macOS arm64
bundle built successfully and passed `codesign --verify --deep --strict`. The
full test suite on macOS reported **2,164 passed, 6 skipped, 12 failed**. All 12
failures are Linux USB-path tests that also fail on the unmodified fork base
`7acf6adc38d1d7ed8517f2ffc36ed7c368baae43` on the same Mac (that baseline reports
2,144 passed, 6 skipped, 12 failed). There were no new failing tests; all 13 new
parser/binding cases and the additional configuration checks passed. The first
run under concurrent build load had two extra timer-accuracy failures; all four
timer cases passed in the final isolated run. These results are local macOS
validation, not a claim of Windows/Linux hardware testing or hosted CI success.

## AI disclosure

This fork's L310 integration, native eraser toggle, tests, and build documentation
include AI-assisted work. Device behavior was checked against captured input and
user testing; remaining limits are listed above.
