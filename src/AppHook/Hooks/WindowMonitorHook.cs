using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace MSAgentAI.AppHook.Hooks
{
    /// <summary>
    /// Example hook that monitors a Windows application and reacts to window title changes
    /// This is a template for creating custom application-specific hooks
    /// </summary>
    public class WindowMonitorHook : AppHookBase
    {
        private Timer _pollTimer;
        private string _lastWindowTitle;
        private readonly int _pollIntervalMs;
        private readonly string _processName;
        
        // Win32 API imports for window monitoring
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        
        /// <summary>
        /// Creates a window monitor hook for a specific process
        /// </summary>
        /// <param name="processName">Name of the process to monitor (without .exe)</param>
        /// <param name="displayName">Display name for this hook</param>
        /// <param name="pollIntervalMs">How often to check window title (default: 1000ms)</param>
        public WindowMonitorHook(string processName, string displayName = null, int pollIntervalMs = 1000)
            : base($"window_monitor_{processName}", displayName ?? $"{processName} Monitor", 
                   $"Monitors window title changes for {processName}", processName)
        {
            _processName = processName;
            _pollIntervalMs = pollIntervalMs;
            _lastWindowTitle = "";
        }
        
        protected override void OnStart()
        {
            _pollTimer = new Timer(_pollIntervalMs);
            _pollTimer.Elapsed += PollTimer_Elapsed;
            _pollTimer.AutoReset = true;
            _pollTimer.Start();
        }
        
        protected override void OnStop()
        {
            if (_pollTimer != null)
            {
                _pollTimer.Stop();
                _pollTimer.Dispose();
                _pollTimer = null;
            }
        }
        
        public override bool IsCompatible()
        {
            // This hook works on Windows only
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }
        
        private void PollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Get the foreground window
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero)
                    return;
                
                // Get the process ID of the window
                GetWindowThreadProcessId(hwnd, out uint processId);
                
                // Get the process
                Process process;
                try
                {
                    process = Process.GetProcessById((int)processId);
                }
                catch
                {
                    return; // Process doesn't exist
                }
                
                // Check if this is the target process
                if (!process.ProcessName.Equals(_processName, StringComparison.OrdinalIgnoreCase))
                    return;
                
                // Get the window title
                const int maxChars = 256;
                var text = new StringBuilder(maxChars);
                if (GetWindowText(hwnd, text, maxChars) > 0)
                {
                    string currentTitle = text.ToString();
                    
                    // Check if title changed
                    if (currentTitle != _lastWindowTitle)
                    {
                        if (!string.IsNullOrEmpty(_lastWindowTitle)) // Skip first time
                        {
                            TriggerEvent(new AppHookEventArgs
                            {
                                EventType = AppHookEventType.WindowTitleChanged,
                                Prompt = $"The {DisplayName} window title changed to: {currentTitle}",
                                Context = currentTitle,
                                Priority = 1,
                                Interrupt = false
                            });
                        }
                        
                        _lastWindowTitle = currentTitle;
                    }
                }
            }
            catch (Exception ex)
            {
                // Don't crash the timer on errors
                Logging.Logger.LogError($"WindowMonitorHook: Error polling window for {_processName}", ex);
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _pollTimer?.Dispose();
        }
    }
}
