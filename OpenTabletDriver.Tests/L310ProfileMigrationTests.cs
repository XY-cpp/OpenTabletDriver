using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Output;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class L310ProfileMigrationTests
    {
        private const string OldName = "Huion L310 (Bluetooth experimental)";
        private const string NewName = "Huion L310 (Bluetooth)";

        [Fact]
        public void DeviceLookupReusesTheEntireLegacyProfile()
        {
            var saved = CreateProfile(OldName);
            var area = saved.AbsoluteModeSettings;
            var bindings = saved.BindingSettings;
            var profiles = new ProfileCollection(new[] { saved });
            var tablet = new TabletReference
            {
                Properties = new TabletConfiguration
                {
                    Name = NewName,
                    Specifications = new TabletSpecifications
                    {
                        Digitizer = new DigitizerSpecifications(),
                        Pen = new PenSpecifications()
                    },
                    DigitizerIdentifiers = []
                }
            };

            Assert.Same(saved, profiles.GetProfile(tablet));
            Assert.Equal(NewName, saved.Tablet);
            Assert.Single(profiles);
            Assert.Same(area, saved.AbsoluteModeSettings);
            Assert.Same(bindings, saved.BindingSettings);
            Assert.Equal(90, saved.AbsoluteModeSettings!.Tablet.Rotation);
            Assert.Equal(typeof(EraserToggleBinding).FullName, saved.BindingSettings.PenButtons[0]!.Path);
        }

        [Fact]
        public void ExistingNewProfileTakesPrecedence()
        {
            var legacy = CreateProfile(OldName);
            var current = CreateProfile(NewName);
            var profiles = new ProfileCollection(new[] { legacy, current });

            Assert.Same(current, profiles.GetProfile(NewName));
            Assert.Equal(OldName, legacy.Tablet);
            Assert.Equal(2, profiles.Count);
        }

        [Fact]
        public void UnrelatedDeviceLookupDoesNotRenameTheLegacyProfile()
        {
            var saved = CreateProfile(OldName);
            var profiles = new ProfileCollection(new[] { saved });

            Assert.Null(profiles.GetProfile("Another tablet"));
            Assert.Equal(OldName, saved.Tablet);
        }

        private static Profile CreateProfile(string name) => new()
        {
            Tablet = name,
            OutputMode = new PluginSettingStore(typeof(AbsoluteMode)),
            AbsoluteModeSettings = new AbsoluteModeSettings
            {
                Display = new AreaSettings { Width = 1920, Height = 1080, X = 960, Y = 540 },
                Tablet = new AreaSettings { Width = 100, Height = 160, X = 80, Y = 50, Rotation = 90 }
            },
            BindingSettings = new BindingSettings
            {
                PenButtons = new PluginSettingStoreCollection { new(typeof(EraserToggleBinding)) }
            }
        };
    }
}
