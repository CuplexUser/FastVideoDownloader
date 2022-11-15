namespace FastVideoDownloader.Models
{
    public class SettingsFileModel
    {
        public string FullPath { get; set; }

        public string Directory { get; set; }

        public string Filename { get; set; }

        public long FileSize { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastWriteTime { get; set; }

        public SettingsFileModel()
        {
            CreationTime=DateTime.Now;
        }

        public static SettingsFileModel CreateModel(string fullPath)
        {
            var model = new SettingsFileModel
            {
                FullPath = fullPath,
                Directory = Path.GetDirectoryName(fullPath),
                Filename = Path.GetFileName(fullPath)
            };

            FileInfo fi= new FileInfo(fullPath);

            model.FileSize = fi.Length;
            model.CreationTime = fi.CreationTime;
            model.LastWriteTime = fi.LastWriteTime;

            return model;
        }
    }
}