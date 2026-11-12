using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Octokit;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop.Updater
{
    public abstract partial class GitHubUpdater : Updater
    {
        static private readonly string _binaryDirectory = BinaryDirectoryRegex().Match(AppContext.BaseDirectory) switch
        {
            { Success: true } match => match.Groups[1].ToString(),
            _ => AppContext.BaseDirectory
        };
        private readonly IGitHubClient _github;

        protected GitHubUpdater(Version currentVersion, AppInfo appInfo, IGitHubClient client)
            : base(currentVersion, _binaryDirectory, appInfo.AppDataDirectory, appInfo.BackupDirectory)
        {
            _github = client;
        }

        protected abstract Task<Update> Download(Release release, Version version);

        protected override async Task<UpdateInfo?> CheckForUpdatesCore()
        {
            try
            {
                var release = await _github.Repository.Release.GetLatest(GitHubReleaseSource.Owner, GitHubReleaseSource.Repository);
                var tag = release.TagName;
                var version = new Version(tag.StartsWith('v') ? tag[1..] : tag);

                return new UpdateInfo(async () => await Download(release, version))
                {
                    Version = version
                };
            }
            catch (Exception ex)
            {
                Log.Write(nameof(GitHubUpdater), $"Failed to check for updates: {ex}", LogLevel.Error);
                return null;
            }
        }

        [GeneratedRegex("^(.*)/[^/]+\\.app/Contents/MacOS/?$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex BinaryDirectoryRegex();
    }
}
