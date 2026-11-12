using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NSubstitute;
using Octokit;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.UX.Windows.Updater;
using Xunit;

namespace OpenTabletDriver.Tests.Updater
{
    public class GitHubUpdaterTests
    {
        [Theory]
        [InlineData("0.6.8.5", true)]
        [InlineData("v0.6.8.5", true)]
        [InlineData("0.6.8.4", false)]
        [InlineData("v0.6.8.3", false)]
        public async Task CheckForUpdates_UsesForkAndComparesReleaseVersion(string tag, bool expected)
        {
            using var env = new UpdaterEnvironment(new Version("0.6.8.4"));
            var client = Substitute.For<IGitHubClient>();
            client.Repository.Release.GetLatest("XY-cpp", "OpenTabletDriver")
                .Returns(Task.FromResult(CreateRelease(tag)));
            var updater = new TestGitHubUpdater(env, client);

            var update = await updater.CheckForUpdates();

            Assert.Equal(expected, update is not null);
            await client.Repository.Release.Received(1).GetLatest("XY-cpp", "OpenTabletDriver");
            await client.Repository.Release.DidNotReceive().GetLatest("OpenTabletDriver", "OpenTabletDriver");
            Assert.Equal("https://github.com/XY-cpp/OpenTabletDriver/releases/latest", UpdaterWindow.LATEST_RELEASE_URL);
        }

        [Theory]
        [InlineData(Architecture.Arm64, "osx-arm64")]
        [InlineData(Architecture.X64, "osx-x64")]
        public void MacOS_SelectsArchiveForProcessArchitecture(Architecture architecture, string runtime)
        {
            var release = CreateRelease("0.6.8.5", new[]
            {
                "OpenTabletDriver-0.6.8.5_osx-x64.tar.gz",
                "OpenTabletDriver-0.6.8.5_osx-arm64.zip",
                "OpenTabletDriver-0.6.8.5_osx-arm64.tar.gz"
            }.Select(CreateAsset).ToArray());

            var asset = TestMacOSUpdater.SelectAsset(release, architecture);

            Assert.Equal($"OpenTabletDriver-0.6.8.5_{runtime}.tar.gz", asset.Name);
        }

        [Fact]
        public void MacOS_DoesNotFallBackToAnotherArchitecture()
        {
            var release = CreateRelease("0.6.8.5", [CreateAsset("OpenTabletDriver-0.6.8.5_osx-x64.tar.gz")]);

            Assert.Throws<InvalidOperationException>(() => TestMacOSUpdater.SelectAsset(release, Architecture.Arm64));
        }

        private static Release CreateRelease(string tag, IReadOnlyList<ReleaseAsset>? assets = null)
            => new Release("", "", "", "", 1, "", tag, "0.6.x", tag, "", false, false,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null!, "", "", assets ?? []);

        private static ReleaseAsset CreateAsset(string name)
            => new ReleaseAsset("", 1, "", name, "", "uploaded", "application/gzip", 0, 0,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, "", null!);

        private sealed class TestGitHubUpdater : GitHubUpdater
        {
            public TestGitHubUpdater(UpdaterEnvironment env, IGitHubClient client)
                : base(env.Version, new AppInfo { AppDataDirectory = env.AppDataDir }, client)
            {
            }

            protected override Task<Update> Download(Release release, Version version)
                => throw new NotSupportedException();
        }

        private sealed class TestMacOSUpdater : MacOSUpdater
        {
            public TestMacOSUpdater(AppInfo info, IGitHubClient client) : base(info, client)
            {
            }

            public static ReleaseAsset SelectAsset(Release release, Architecture architecture)
                => GetAsset(release, architecture);
        }
    }
}
