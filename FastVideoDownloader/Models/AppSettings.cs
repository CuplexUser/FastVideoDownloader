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

        [JsonProperty("downloaderApp", Order = 4)]
        public string DownloaderApp { get; set; }

        [JsonProperty("autoConfirmDownload", Order = 5)]
        public bool AutoConfirmDownload { get; set; }

        [JsonProperty("alternetiveConfigData", Order = 6, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AlternativeConfigData { get; set; }

        public AppSettings()
        {

        }
    }
}