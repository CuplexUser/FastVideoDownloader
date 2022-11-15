using FastVideoDownloader.Models;
using Serilog;
using FastVideoDownloader.Service;

namespace FastVideoDownloader.Automation
{
    public class ConfigFileChangeListener : IDisposable
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private AppSettings _settings;
        private SettingsFileModel _configFileModel;
        private bool monitoringActive = false;
        private readonly CancellationTokenSource cancellationTokenSource;
        private Task backgroundWorker;
        private readonly AsyncFileAccessService _fileAccessService;

        public event EventHandler<FileSystemEventArgs> Changed;
        public event EventHandler<FileSystemEventArgs> Deleted;

        public SettingsFileModel SettingsMetadata
        {
            get
            {
                return _configFileModel;
            }
        }

        public ConfigFileChangeListener(AppSettings settings, SettingsFileModel configFileModel)
        {
            _settings = settings;
            _fileSystemWatcher = new FileSystemWatcher();
            _fileSystemWatcher.Changed += OnConfigFileChanged;
            _fileSystemWatcher.Deleted += OnConfigFileDeleted;
            _configFileModel = configFileModel;
            cancellationTokenSource = new CancellationTokenSource();
            _fileAccessService = new AsyncFileAccessService(true);
        }

        protected virtual void OnConfigFileDeleted(object sender, FileSystemEventArgs e)
        {

        }

        protected virtual void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Settings file was updated, Reloading!");
        }

        public bool StartMonitoringConfigFile()
        {
            if (monitoringActive)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_configFileModel.Directory) || !Directory.Exists(_configFileModel.Directory))
            {
                Log.Warning("Given config file path does not exist");
                return false;
            }
            _fileSystemWatcher.BeginInit();

            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.IncludeSubdirectories = false;
            _fileSystemWatcher.Path = _configFileModel.Directory;
            _fileSystemWatcher.Filter = _configFileModel.Filename;

            _fileSystemWatcher.EndInit();
            monitoringActive = true;
            backgroundWorker = Task.Factory.StartNew(TaskRunner, cancellationTokenSource.Token);

            return true;
        }

        private void TaskRunner()
        {
            while (monitoringActive)
            {
                var handle = _fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Changed | WatcherChangeTypes.Deleted);

                if (handle.ChangeType == WatcherChangeTypes.Changed)
                {
                    Log.Debug("Configuration file changed. The previous lastWriteTime was {LastWrite}", _configFileModel.LastWriteTime);
                    _configFileModel.LastWriteTime = DateTime.Now;

                    Changed?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, _configFileModel.Directory, _configFileModel.Filename));
                }
                else if (handle.ChangeType == WatcherChangeTypes.Deleted)
                {
                    Log.Debug("Configuration file deleted at: {deletionDate}. The previous CreateDate was {CreateDate}", DateTime.Now, _configFileModel.CreationTime);
                    _configFileModel.LastWriteTime = DateTime.Now;

                    Deleted?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, _configFileModel.Directory, _configFileModel.Filename));
                }
            }
        }

        public void SetMonitoringStatus(bool monitor)
        {
            if (!monitoringActive)
                return;

            _fileSystemWatcher.EnableRaisingEvents = monitor;
        }

        public void Dispose()
        {
            monitoringActive = false;
            cancellationTokenSource.CancelAfter(150);
            cancellationTokenSource.Token.WaitHandle.WaitOne(100);
            _fileSystemWatcher?.Dispose();

            if (backgroundWorker.Status == TaskStatus.RanToCompletion)
                backgroundWorker.Dispose();
        }

        public async Task<SettingsFileModel> UpdateSettingsMetaModel()
        {
            SettingsFileModel model= null;
            DateTime lastWriteTime = _configFileModel.LastWriteTime;

            _settings = await _fileAccessService.LoadAppSettingsAsync();
            model = _fileAccessService.SettingsMetadata;

            if (SettingsMetadata.LastWriteTime != lastWriteTime)
            {
                //Reload settings
                Log.Debug("Reloading App Settings from file since the file has changed. Triggered from UpdateSettingsMetaModel()");
                
            }

            return model;
        }
    }
}