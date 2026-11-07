#!/usr/bin/env fish
# Run with a .NET 10 SDK, fish, and Xcode Command Line Tools on macOS.
set -l repository (path resolve (path dirname (status filename))/../..)
set -l runtime osx-arm64
if test (count $argv) -gt 0
    set runtime $argv[1]
end
if not contains -- "$runtime" osx-arm64 osx-x64
    echo 'Usage: fish eng/fish/build-l310-macos.fish [osx-arm64|osx-x64]' >&2
    exit 1
end

set -l app "$repository/dist/l310-$runtime/OpenTabletDriver.app"
set -l binaries "$app/Contents/MacOS"
mkdir -p "$binaries" "$app/Contents/Resources"; or exit $status
for project in OpenTabletDriver.Daemon OpenTabletDriver.Console OpenTabletDriver.UX.MacOS
    dotnet publish "$repository/$project/$project.csproj" \
        --configuration Release --framework net10.0 --runtime "$runtime" \
        --self-contained true --output "$binaries" \
        -p:PublishSingleFile=true -p:PublishTrimmed=false -p:DebugType=embedded \
        -p:MacBuildBundle=false -p:_EnableMacOSCodeSign=false \
        -p:VersionSuffix=huion-l310.3; or exit $status
end
cp "$repository/eng/bash/macos/Info.plist" "$app/Contents/Info.plist"; or exit $status
cp "$repository/eng/bash/macos/Icon.icns" "$app/Contents/Resources/"; or exit $status
/usr/libexec/PlistBuddy -c 'Set :CFBundleShortVersionString 0.6.8.1' "$app/Contents/Info.plist"; or exit $status
/usr/libexec/PlistBuddy -c 'Add :CFBundleVersion string 20261107.3' "$app/Contents/Info.plist"; or exit $status
codesign --deep --force --sign - "$app"; or exit $status
codesign --verify --deep --strict "$app"; or exit $status
echo "Built and verified: $app"
