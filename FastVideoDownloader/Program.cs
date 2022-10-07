// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using FastVideoDownloader.Config;
using FastVideoDownloader.Models;
using FastVideoDownloader.Service;
using Serilog;


LogConfig logConfig = new();
logConfig.InitLogConfig();
ILogger logger = logConfig.Logger;
Log.Logger = logger;

Console.WriteLine("Starting AppRunnerService");

var taskRunnerService = new AppRunnerService();
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

var settings = AppSettings.ReadAppSettings();
logger.Information("Paste Url... or enter quit to exit");

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
    else
    {
        if (input is "quit")
            break;
        if (input is "clear")
            Console.Clear();
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No recognized command or url");
            Console.ResetColor();
        }
    }

} while (true);

await taskRunnerService.Stop();
Console.WriteLine("App closing, press enter to close");