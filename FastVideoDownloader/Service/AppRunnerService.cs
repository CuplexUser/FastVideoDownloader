using System.Diagnostics;
using FastVideoDownloader.Models;
using Serilog;

namespace FastVideoDownloader.Service
{
    public class AppRunnerService
    {
        private readonly AutoResetEvent _autoReset;
        private readonly Queue<string> _jobQueue;
        private readonly Task taskRunner;
        private readonly CancellationTokenSource tokenSource;
        private TextWriter _downloadTextWriter;
        private bool runMainTask = true;
        private readonly AppSettingsService _asyncFileAccessService;

        public AppRunnerService(AppSettingsService asyncFileAccessService)
        {
            _asyncFileAccessService = asyncFileAccessService;
            _autoReset = new AutoResetEvent(true);
            _jobQueue = new Queue<string>();

            tokenSource = new CancellationTokenSource();
            taskRunner = Task.Factory.StartNew(MainLoop, tokenSource.Token);
            _asyncFileAccessService.AppSettingsChanged += OnAppsettingsChanged;
        }

        private void OnAppsettingsChanged(object sender, EventArgs e)
        {
          
        }

        public async Task Stop()
        {
            runMainTask = false;
            _autoReset.Set();
            tokenSource.CancelAfter(250);
            await taskRunner;
        }

        public async Task<bool> Start()
        {
            var settings = await _asyncFileAccessService.LoadAppSettingsAsync();

            return await Task<bool>.Factory.StartNew(() =>
            {
                _autoReset.Set();
                return taskRunner.Status == TaskStatus.Running;
            });
        }

        private async Task<bool> MainLoop()
        {
            var settings = await _asyncFileAccessService.LoadAppSettingsAsync();

            while (runMainTask)
            {
                while (_jobQueue.Count > 0)
                {
                    string url = _jobQueue.Dequeue();
                    string parameters = settings.DownloadParams;
                    parameters += $" --paths {settings.DownloadFolder}";

                    // Full command string
                    string command = settings.DownloadCommand.Replace("$params", parameters).Replace("$url", url);

                    await _downloadTextWriter.WriteLineAsync($"Downloading {url}");
                    var psi = new ProcessStartInfo
                    {
                        FileName = settings.DownloadCommand,
                        Arguments = command,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        WorkingDirectory = settings.DownloadFolder,
                    };

                    try
                    {
                        using Process process = Process.Start(psi);

                        if (process != null)
                        {
                            var buffer = new char[2048];
                            var memoryBuffer = new Memory<char>(buffer);

                            using StreamReader reader = process.StandardOutput;
                            Log.Debug("Starting app with arguments: {args}", psi.Arguments);

                            //Console.SetIn(reader);
                            //var stream = Console.OpenStandardInput(256);

                            while (!reader.EndOfStream)
                            {
                                int charsRead = await reader.ReadBlockAsync(memoryBuffer, tokenSource.Token);
                                if (charsRead > 0)
                                {
                                    await _downloadTextWriter.WriteAsync(memoryBuffer, tokenSource.Token);

                                    Console.SetCursorPosition(0, Console.CursorTop);
                                }
                            }

                            await _downloadTextWriter.WriteLineAsync("");
                            Console.ResetColor();
                            await process.WaitForExitAsync();

                            if (process.ExitCode == 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                await _downloadTextWriter.WriteLineAsync("Download Complete");
                                Console.ResetColor();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Faild to process request, exception in process: {proc}", psi);
                    }
                }

                if (!runMainTask)
                    break;

                _autoReset.Reset();
                _autoReset.WaitOne();
            }

            return true;
        }



        public void QueueJob(string input, TextWriter textWriter)
        {
            _downloadTextWriter = textWriter;
            _jobQueue.Enqueue(input);
            _autoReset.Set();
        }
    }
}