﻿using System.Diagnostics;
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
        private AppSettings _settings;
        private bool runMainTask = true;

        public AppRunnerService()
        {
            _autoReset = new AutoResetEvent(true);
            _jobQueue = new Queue<string>();

            tokenSource = new CancellationTokenSource();
            taskRunner = Task.Factory.StartNew(MainLoop, tokenSource.Token);
        }

        public async Task Stop()
        {
            runMainTask = false;
            _autoReset.Set();
            tokenSource.CancelAfter(250);
            await taskRunner;
        }

        public Task<bool> Start()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                ValueTask<AppSettings> valueTask = new(LoadSettings());
                _settings = valueTask.Result;
                _autoReset.Set();
                return taskRunner.Status == TaskStatus.Running;
            });
        }

        private async Task<AppSettings> LoadSettings()
        {
            AppSettings settings = await Task<AppSettings>.Factory.StartNew(() => AppSettings.ReadAppSettings());

            return settings;
        }

        private async Task<bool> MainLoop()
        {
            while (runMainTask)
            {
                while (_jobQueue.Count > 0)
                {
                    string url = _jobQueue.Dequeue();
                    string parameters = _settings.DownloadParams;
                    parameters += $" --paths {_settings.DownloadFolder}";

                    // Full command string
                    string command = _settings.DownloadCommand.Replace("$params", parameters).Replace("$url", url);

                    await _downloadTextWriter.WriteLineAsync($"Downloading {url}");
                    var psi = new ProcessStartInfo
                    {
                        FileName = _settings.DownloaderApp,
                        Arguments = command,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        WorkingDirectory = _settings.DownloadFolder,
                    };

                    try
                    {
                        using Process process = Process.Start(psi);

                        if (process != null)
                        {
                            var buffer = new char[4096];
                            var memoryBuffer = new Memory<char>(buffer);

                            using StreamReader reader = process.StandardOutput;

                            //Console.SetIn(reader);
                            //var stream = Console.OpenStandardInput(256);

                            while (!reader.EndOfStream)
                            {
                                int charsRead = await reader.ReadBlockAsync(memoryBuffer,tokenSource.Token); 
                                if (charsRead>0)
                                {
                                    await _downloadTextWriter.WriteAsync(memoryBuffer,tokenSource.Token);
                                    
                                    Console.SetCursorPosition(0, Console.CursorTop);
                                }
                            }

                            await _downloadTextWriter.WriteLineAsync("");
                            Console.ForegroundColor= ConsoleColor.Green;
                            await _downloadTextWriter.WriteLineAsync("Download Complete");
                            Console.ResetColor();
                            await process.WaitForExitAsync();


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