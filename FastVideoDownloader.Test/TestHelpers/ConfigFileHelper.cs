using FastVideoDownloader.Models;
using Newtonsoft.Json;
using NuGet.Frameworks;

namespace FastVideoDownloader.Test.TestHelpers
{
    internal class ConfigFileHelper
    {
        public ConfigFileHelper(string path)
        {

        }

        public static AppSettings GetDefaultSettings()
        {
            var settings = new AppSettings();
            settings.DownloadFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp");
            settings.AutoConfirmDownload = true;
            settings.DownloadCommand = "yt-dlp.exe $params $url";
            settings.DownloadParams = "-U --windows-filenames --trim-filenames 40";
            settings.MonitorClipboardForVideoUrl = false;
            settings.ClipboardPatternRegExp = "";

            return settings;
        }

        internal static bool SaveSettingsFile(AppSettings settings, string filePath)
        {
            FileStream fs = null; ;

            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);

                fs = File.OpenWrite(filePath);
                string jsonData = JsonConvert.SerializeObject(settings, Formatting.Indented);                

                StreamWriter writer = new StreamWriter(fs);
                writer.Write(jsonData);
                writer.Flush();
                fs.Flush();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            finally
            {
                fs?.Close();
            }

            return true;
        }
    }
}
