using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace FastVideoDownloader.Config
{
    public class LogConfig
    {
        public ILogger Logger { get; private set; }

        public bool InitLogConfig()
        {
            if (Logger == null)
            {
                Logger = CreateLogger();
                return Logger != null;
            }
            
            return Logger != null;
        }

        private static ILogger CreateLogger()
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console(LogEventLevel.Information, theme: SystemConsoleTheme.Colored, standardErrorFromLevel: LogEventLevel.Error)
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                .CreateLogger();

            return logger;
        }
    }
}