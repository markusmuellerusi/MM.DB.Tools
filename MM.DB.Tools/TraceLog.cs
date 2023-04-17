using System;

namespace MM.DB.Tools
{
    /// <summary>
    /// Stellt eine, für diese Assembly, exclusive Instanz des Loggers zur Verfügung.
    /// </summary>
    public static class TraceLog
    {
        private static ILogger _logger;
        private static readonly object LockObject = new();

        /// <summary>
        /// Liefert die Instanz des Loggers.
        /// </summary>
        public static ILogger Logger
        {
            get
            {
                lock (LockObject)
                {
                    return _logger ??= new Logger();
                }
            }
        }
    }

    public interface ILogger
    {
        void Verbose(string msg);
        void Information(string msg);
        void Warning(string msg);
        void Error(string msg);
        void LogError(Exception exception);
    }

    public class Logger : ILogger
    {
        public void Verbose(string msg)
        {
            var fgc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Out.WriteLine(msg);
            Console.ForegroundColor = fgc;
        }

        public void Information(string msg)
        {
            var fgc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Out.WriteLine(msg);
            Console.ForegroundColor = fgc;
        }

        public void Warning(string msg)
        {
            var fgc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Out.WriteLine(msg);
            Console.ForegroundColor = fgc;
        }

        public void Error(string msg)
        {
            var fgc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Out.WriteLine(msg);
            Console.ForegroundColor = fgc;
        }

        public void LogError(Exception exception)
        {
            if (exception == null) return;
            var fgc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Out.WriteLine(exception.ToString());
            Console.ForegroundColor = fgc;
        }
    }
}