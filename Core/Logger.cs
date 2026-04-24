using System;
using System.IO;
using System.Text;

namespace SmartDesktopAssistant.Core
{
    /// <summary>
    /// Simple logger for error tracking and diagnostics
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new();
        private static string? _logPath;
        private static StringBuilder? _buffer;

        public static void Initialize()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folder = Path.Combine(appDataPath, "SmartDesktopAssistant", "Logs");
            Directory.CreateDirectory(folder);
            
            _logPath = Path.Combine(folder, $"log_{DateTime.Now:yyyyMMdd}.txt");
            _buffer = new StringBuilder();
            
            Log("INFO", "Logger initialized");
        }

        public static void Info(string message)
        {
            Log("INFO", message);
        }

        public static void Warning(string message)
        {
            Log("WARN", message);
        }

        public static void Error(string message, Exception? ex = null)
        {
            var fullMessage = ex != null 
                ? $"{message}\nException: {ex.GetType().Name}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}"
                : message;
            Log("ERROR", fullMessage);
        }

        private static void Log(string level, string message)
        {
            lock (_lock)
            {
                var entry = $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {message}";
                
                _buffer?.AppendLine(entry);
                
                // Flush to disk every 10 entries or on error
                if (_buffer?.Length > 5000 || level == "ERROR")
                {
                    Flush();
                }
                
#if DEBUG
                System.Diagnostics.Debug.WriteLine(entry);
#endif
            }
        }

        public static void Flush()
        {
            if (_logPath != null && _buffer != null && _buffer.Length > 0)
            {
                try
                {
                    File.AppendAllText(_logPath, _buffer.ToString());
                    _buffer.Clear();
                }
                catch { }
            }
        }

        public static string GetLogPath() => _logPath ?? "";

        public static string[] ReadRecentLogs(int lines = 100)
        {
            if (_logPath == null || !File.Exists(_logPath))
                return Array.Empty<string>();

            try
            {
                var allLines = File.ReadAllLines(_logPath);
                var start = Math.Max(0, allLines.Length - lines);
                var result = new string[allLines.Length - start];
                Array.Copy(allLines, start, result, 0, result.Length);
                return result;
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Analyze logs for errors and warnings
        /// </summary>
        public static LogAnalysisResult AnalyzeLogs()
        {
            var result = new LogAnalysisResult();
            var logs = ReadRecentLogs(1000);

            foreach (var line in logs)
            {
                if (line.Contains("[ERROR]"))
                    result.ErrorCount++;
                else if (line.Contains("[WARN]"))
                    result.WarningCount++;
                else if (line.Contains("[INFO]"))
                    result.InfoCount++;
            }

            // Find recent errors
            for (int i = logs.Length - 1; i >= 0 && result.RecentErrors.Count < 5; i--)
            {
                if (logs[i].Contains("[ERROR]"))
                {
                    result.RecentErrors.Add(logs[i]);
                }
            }

            return result;
        }
    }

    public class LogAnalysisResult
    {
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }
        public List<string> RecentErrors { get; set; } = new();

        public bool HasErrors => ErrorCount > 0;
        public bool HasWarnings => WarningCount > 0;
    }
}
