using System;
using System.IO;

namespace Aljaras.Core
{
    /// <summary>
    /// Minimal thread-safe file logger. Writes one daily log file under
    /// <c>{AppLocation}\Logs\Aljaras-yyyy-MM-dd.log</c>. Logging never throws:
    /// any failure (e.g. disk/permission) is swallowed so it can't crash the app.
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new();
        private static readonly string LogDirectory = Path.Combine(GlobalVariables.AppLocation, "Logs");

        private static string LogFilePath => Path.Combine(LogDirectory, $"Aljaras-{DateTime.Now:yyyy-MM-dd}.log");

        public static void Info(string message) => Write("INFO", message);

        public static void Error(string message) => Write("ERROR", message);

        public static void Error(string message, Exception ex) => Write("ERROR", $"{message}{Environment.NewLine}{ex}");

        private static void Write(string level, string message)
        {
            try
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(LogDirectory);
                    File.AppendAllText(LogFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}");
                }
            }
            catch
            {
                // Logging must never crash the app.
            }
        }
    }
}
