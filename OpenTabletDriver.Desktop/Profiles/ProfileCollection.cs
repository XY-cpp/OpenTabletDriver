using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class ProfileCollection : ObservableCollection<Profile>
    {
        public ProfileCollection()
        {
        }

        public ProfileCollection(IEnumerable<Profile> profiles)
            : base(profiles)
        {
        }

        public ProfileCollection(IEnumerable<TabletReference> tablets)
            : this(tablets.Select(Profile.GetDefaults))
        {
        }

        public Profile this[TabletReference tablet]
        {
            set => SetProfile(tablet, value);
            get => GetProfile(tablet);
        }

        public void SetProfile(TabletReference tablet, Profile profile)
        {
            if (this.FirstOrDefault(t => t.Tablet == tablet.Properties.Name) is Profile oldProfile)
            {
                this.Remove(oldProfile);
            }
            this.Add(profile);
        }

        public Profile GetProfile(TabletReference tablet)
        {
            return GetProfile(tablet.Properties.Name) ?? Generate(tablet);
        }

        public Profile? GetProfile(string tablet)
        {
            var profile = this.FirstOrDefault(t => t.Tablet == tablet);
            if (profile == null && tablet == "Huion L310 (Bluetooth)")
            {
                // Reuse the complete saved profile from the earlier fork builds.
                // An existing profile under the new name always takes precedence.
                profile = this.FirstOrDefault(t => t.Tablet == "Huion L310 (Bluetooth experimental)");
                if (profile != null)
                    profile.Tablet = tablet;
            }
            return profile;
        }

        public Profile Generate(TabletReference tablet)
        {
            var profile = Profile.GetDefaults(tablet);
            SetProfile(tablet, profile);
            return profile;
        }
    }
}
