namespace FastVideoDownloader.Config
{
    public class ConfigHelper
    {
        private const string FileName = "config.json";

        public static string GetConfigFilePath()
        {
            string dir = Environment.CurrentDirectory;

            return Path.Join(dir, FileName);
        }
    }
}