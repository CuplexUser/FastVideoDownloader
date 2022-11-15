// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using FastVideoDownloader.Automation;
using FastVideoDownloader.Config;
using FastVideoDownloader.Models;
using FastVideoDownloader.Service;
using Newtonsoft.Json;
using Serilog;


LogConfig logConfig = new();
if (!logConfig.InitLogConfig())
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Failed to initialize log configuration!");
    Environment.Exit(1);
}

AsyncFileAccessService _asyncFileAccessService = new AsyncFileAccessService(true);
ILogger logger = logConfig.Logger;
Log.Logger = logger;

Console.WriteLine("Starting AppRunnerService");

var taskRunnerService = new AppRunnerService(_asyncFileAccessService);
bool result = await taskRunnerService.Start();

if (result)
{
    logger.Information("Config read and Application is Ready!");
}
else
{
    logger.Error("Failed to start taskRunnerService");
    Environment.Exit(1);
}



var settings = await _asyncFileAccessService.LoadAppSettingsAsync();
logger.Information("Paste Url... or enter quit to exit");

List<Tuple<string, ConsoleColor>> HelpTexts = new List<Tuple<string, ConsoleColor>>
{
    new("**Available commands are**", ConsoleColor.Blue),
    new("Enter download url [downloads video]\nexit or quit [quit application]\nhelp [show help text]\nclear [clear console]", ConsoleColor.White),
    new("config [prints configuration options]", ConsoleColor.White),
    new("--reload-conf updates app settings based on file changes", ConsoleColor.White),
    new("Configuration settings are defined in 'config.json'", ConsoleColor.Gray)
};

ConfigFileChangeListener changeListener = new ConfigFileChangeListener(settings, SettingsFileModel.CreateModel(ConfigReader.GetConfigFilePath()));
changeListener.Changed += ConfigChangeListener_Changed;
changeListener.Deleted += OnConfigFileDeleted;
bool status = changeListener.StartMonitoringConfigFile();



if (!status)
{
    Log.Error("Failed to Start Config File Monitoring");
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
        settings = await _asyncFileAccessService.LoadAppSettingsAsync();
    });
}

async Task ReloadLoadSettingsAsync()
{
    settings = await _asyncFileAccessService.LoadAppSettingsAsync();
    Log.Debug("settings object model was updated");
}

//Console.WriteLine("Config read and App is Ready");
//Console.WriteLine("Paste Url... or enter quit to exit");
Regex urlRegex = new Regex(@"(^http://[\w\./]+)|(^https://[\w\./]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);



do
{
    string input = Console.ReadLine();
    if (!string.IsNullOrEmpty(input) && urlRegex.IsMatch(input))
    {
        if (!settings.AutoConfirmDownload)
        {
            Console.WriteLine($"Download from url '{input}'?  (y/n)");
            string response = Console.ReadLine();
            if (response != null && response.Equals("y", StringComparison.CurrentCultureIgnoreCase))
            {
                taskRunnerService.QueueJob(input, Console.Out);
            }
        }
        else
        {
            taskRunnerService.QueueJob(input, Console.Out);
        }
    }
    else if (!string.IsNullOrEmpty(input))
    {
        input = input.TrimStart('-');

        if (input is "quit" or "exit")
            break;

        switch (input)
        {
            case "help":
                foreach (var helpText in HelpTexts)
                {
                    Console.ForegroundColor = helpText.Item2;
                    Console.WriteLine(helpText.Item1);
                }
                Console.ResetColor();
                break;
            case "clear":
                Console.Clear();
                break;
            case "config":
                Console.WriteLine(JsonConvert.SerializeObject(settings, Formatting.Indented));
                break;
            case "reload-conf":
                try
                {
                    await ReloadLoadSettingsAsync();
                    Console.WriteLine("reloaded app settings");
                    Console.WriteLine("Current config:");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(JsonConvert.SerializeObject(settings, Formatting.Indented));
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error, failed to parse the updated settings!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Log.Error(ex, "Error message: {message}", ex.Message);
                    Console.ResetColor();
                }
                break;
            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No recognized command or url, enter 'help' for options.");
                Console.ResetColor();
                break;
        }
    }

} while (true);

changeListener.Dispose();
changeListener = null;
await taskRunnerService.Stop();
Console.WriteLine("App closing, press enter to close");