using FastVideoDownloader.Config;
using FastVideoDownloader.Models;
using Newtonsoft.Json;
using System.Text;
using Serilog;
using FastVideoDownloader.Automation;

namespace FastVideoDownloader.Service;

/// <summary>
/// AsyncFileAccessService
/// </summary>
public class AsyncFileAccessService:IDisposable
{
    /// <summary>
    /// The application settings
    /// </summary>
    private AppSettings _appSettings;

    /// <summary>
    /// The file model
    /// </summary>
    private SettingsFileModel _fileModel;

    private readonly ConfigFileChangeListener _changeListener;


    /// <summary>
    /// The settings filepath
    /// </summary>
    private readonly string settingsFilepath;

    /// <summary>
    /// Gets a value indicating whether [only reload from disk after change].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [only reload from disk after change]; otherwise, <c>false</c>.
    /// </value>
    public bool OnlyReloadFromDiskAfterChange { get; }

    public SettingsFileModel SettingsMetadata { get => _fileModel; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncFileAccessService"/> class.
    /// </summary>
    /// <param name="onlyReloadFromDiskAfterChange">if set to <c>true</c> [only reload from disk after change].</param>
    public AsyncFileAccessService(bool onlyReloadFromDiskAfterChange)
    {
        
 
        _changeListener = new ConfigFileChangeListener(SettingsFileModel.CreateModel(ConfigReader.GetConfigFilePath()),this);

        settingsFilepath = ConfigReader.GetConfigFilePath();
        OnlyReloadFromDiskAfterChange = onlyReloadFromDiskAfterChange;

        
        

    
        _changeListener.Changed += ConfigChangeListener_Changed;
        _changeListener.Deleted += OnConfigFileDeleted;
        bool status = _changeListener.StartMonitoringConfigFile();

        if (!status)
        {
            Log.Error("Failed to Start Config File Monitoring");
        }

    }

    void OnConfigFileDeleted(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine("Configuration File was deleted!");
        Console.WriteLine("Still using previous values.");
    }

    void ConfigChangeListener_Changed(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine("Configuration File was changed!");
        Console.WriteLine("Updating internal settings from config file");

        Task.Factory.StartNew(async () =>
        {
            _appSettings = await LoadAppSettingsAsync();
        });
    }

    /// <summary>
    /// Reads the text asynchronous.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns></returns>
    private async Task<string> ReadTextAsync(string filePath)
    {
        await using var sourceStream =
            new FileStream(
                filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true);

        var sb = new StringBuilder();

        byte[] buffer = new byte[0x1000];
        int numRead;
        while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
        {
            string text = Encoding.UTF8.GetString(buffer, 0, numRead);
            sb.Append(text);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Determines whether [is last write time modified] [the specified model].
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>
    ///   <c>true</c> if [is last write time modified] [the specified model]; otherwise, <c>false</c>.
    /// </returns>
    private bool IsLastWriteTimeModified(SettingsFileModel model)
    {
        bool result = false;
        try
        {
            if (!File.Exists(model.FullPath))
            {
                return true;
            }

            var fi = new FileInfo(model.FullPath);
            result = fi.LastWriteTime != model.LastWriteTime;
        }
        catch (Exception exception)
        {
            Log.Error("Exception caought in IsLastWriteTimeModified(). Message: {message}", exception.Message);
        }

        return result;
    }

    /// <summary>
    /// Loads the application settings asynchronous.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.ApplicationException">Can not open config file at {settingsFilepath}</exception>
    public async Task<AppSettings> LoadAppSettingsAsync()
    {
        AppSettings instance = null;

        if (!File.Exists(settingsFilepath))
            throw new ApplicationException($"Can not open config file at {settingsFilepath}");

        if (OnlyReloadFromDiskAfterChange && _appSettings != null && _fileModel != null)
        {
            if (!IsLastWriteTimeModified(_fileModel))
            {
                return _appSettings;
            }
        }


        string jsonData = await ReadTextAsync(settingsFilepath);

        if (jsonData != null)
        {
            try
            {
                instance = JsonConvert.DeserializeObject<AppSettings>(jsonData);
                _appSettings = instance;
                _fileModel = SettingsFileModel.CreateModel(settingsFilepath);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to deserialize appsettings. Message: {message}", ex.Message);
            }
        }

        return instance;
    }

    public void Dispose()
    {
        _changeListener.Dispose();
    }
}