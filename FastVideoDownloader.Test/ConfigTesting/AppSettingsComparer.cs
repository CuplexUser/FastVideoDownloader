using FastVideoDownloader.Models;
using System.Diagnostics.CodeAnalysis;

namespace FastVideoDownloader.Test.ConfigTesting
{
    public class AppSettingsComparer : IEqualityComparer<AppSettings>
    {
        public bool Equals(AppSettings x, AppSettings y)
        {
            return x.MonitorClipboardForVideoUrl == y.MonitorClipboardForVideoUrl &&
                x.ClipboardPatternRegExp == y.ClipboardPatternRegExp &&
                x.DownloadParams == y.DownloadParams &&
                x.ClipboardPatternRegExp == y.ClipboardPatternRegExp &&
                x.DownloadCommand == y.DownloadCommand &&
                x.DownloadParams == y.DownloadParams;
        }

        public int GetHashCode([DisallowNull] AppSettings obj)
        {
            return obj.GetHashCode();
        }
    }
}
