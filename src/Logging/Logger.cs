using System;
using System.IO;
using System.Text;

namespace MSAgentAI.Logging
{
    /// <summary>
    /// Simple file logger for diagnostics and error tracking
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logFilePath;
        private static bool _initialized;

        /// <summary>
        /// Gets the path to the log file
        /// </summary>
        public static string LogFilePath => _logFilePath;

        /// <summary>
        /// Initializes the logger with the default log file location
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                // Log file in the same directory as the executable
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                _logFilePath = Path.Combine(appDir, "MSAgentAI.log");
                _initialized = true;

                // Write header
                Log("=== MSAgent AI Log Started ===");
                Log($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Log($"OS: {Environment.OSVersion}");
                Log($".NET Runtime: {Environment.Version}");
                Log($"64-bit Process: {Environment.Is64BitProcess}");
                Log("================================");
            }
            catch
            {
                // If we can't write to the app directory, try temp
                try
                {
                    _logFilePath = Path.Combine(Path.GetTempPath(), "MSAgentAI.log");
                    _initialized = true;
                }
                catch
                {
                    _initialized = false;
                }
            }
        }

        /// <summary>
        /// Logs a message to the log file
        /// </summary>
        public static void Log(string message)
        {
            if (!_initialized) Initialize();
            if (!_initialized) return;

            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logFilePath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
                }
            }
            catch
            {
                // Silently fail if we can't write
            }
        }

        /// <summary>
        /// Logs an error with exception details
        /// </summary>
        public static void LogError(string message, Exception ex = null)
        {
            var sb = new StringBuilder();
            sb.Append("[ERROR] ");
            sb.Append(message);

            if (ex != null)
            {
                sb.AppendLine();
                sb.Append("  Exception: ");
                sb.Append(ex.GetType().Name);
                sb.Append(" - ");
                sb.Append(ex.Message);

                if (ex.InnerException != null)
                {
                    sb.AppendLine();
                    sb.Append("  Inner: ");
                    sb.Append(ex.InnerException.Message);
                }
            }

            Log(sb.ToString());
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        public static void LogWarning(string message)
        {
            Log($"[WARN] {message}");
        }

        /// <summary>
        /// Logs diagnostic information
        /// </summary>
        public static void LogDiagnostic(string category, string message)
        {
            Log($"[DIAG:{category}] {message}");
        }

        /// <summary>
        /// Opens the log file in the default text editor
        /// </summary>
        public static void OpenLogFile()
        {
            if (!_initialized || string.IsNullOrEmpty(_logFilePath) || !File.Exists(_logFilePath))
                return;

            try
            {
                System.Diagnostics.Process.Start(_logFilePath);
            }
            catch
            {
                // Failed to open
            }
        }

        /// <summary>
        /// Clears the log file
        /// </summary>
        public static void ClearLog()
        {
            if (!_initialized || string.IsNullOrEmpty(_logFilePath))
                return;

            try
            {
                lock (_lock)
                {
                    File.WriteAllText(_logFilePath, string.Empty);
                }
                Log("=== Log Cleared ===");
            }
            catch
            {
                // Silently fail
            }
        }
    }
}
