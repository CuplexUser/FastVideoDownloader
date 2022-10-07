using FastVideoDownloader.Config;
using Newtonsoft.Json;

namespace FastVideoDownloader.Models
{
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

        public AppSettings()
        {

        }

        public static AppSettings ReadAppSettings()
        {
            AppSettings instance = null;
            string fileName = ConfigReader.GetConfigFilePath();
            if (!File.Exists(fileName))
                throw new ApplicationException($"Can not open config file at {fileName}");

            var fs = File.OpenRead(fileName);
            try
            {

                StreamReader sr = new StreamReader(fs);
                string data = sr.ReadToEnd();
                instance = JsonConvert.DeserializeObject<AppSettings>(data);
                sr.Close();

                return instance;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                fs.Close();
            }

            return instance;
        }
    }
}