using Newtonsoft.Json;

namespace FastVideoDownloader.Models
{
    [JsonObject(Id = "AppSettings")]
    public class AppSettings
    {
        [JsonProperty("downloadCommand", Order = 1)]
        public string DownloadCommand { get; set; }

        [JsonProperty("downloadFolder", Order = 2)]
        public string DownloadFolder { get; set; }

        [JsonProperty("downloadParams", Order = 3)]
        public string DownloadParams { get; set; }

        [JsonProperty("autoConfirmDownload", Order = 4)]
        public bool AutoConfirmDownload { get; set; }

        [JsonProperty("monitorClipboardForVideoUrl", Order = 5)]
        public bool MonitorClipboardForVideoUrl { get; set; }

        [JsonProperty("clipboardPatternRegExp", Order = 6)]
        public string ClipboardPatternRegExp { get; set; }


        public AppSettings()
        {

        }
    }
}