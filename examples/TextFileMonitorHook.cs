using System;
using System.IO;
using System.Timers;
using MSAgentAI.AppHook;
using MSAgentAI.Logging;

namespace Examples.CustomHooks
{
    /// <summary>
    /// Example custom hook that monitors a text file for changes
    /// and triggers events when the file content changes.
    /// 
    /// This demonstrates:
    /// - File monitoring
    /// - Periodic polling
    /// - State tracking
    /// - Error handling
    /// - Event triggering
    /// </summary>
    public class TextFileMonitorHook : AppHookBase
    {
        private Timer _pollTimer;
        private readonly string _filePath;
        private string _lastContent;
        private readonly int _pollIntervalMs;
        
        /// <summary>
        /// Creates a new text file monitor hook
        /// </summary>
        /// <param name="filePath">Full path to the file to monitor</param>
        /// <param name="pollIntervalMs">How often to check the file (default: 5000ms)</param>
        public TextFileMonitorHook(string filePath, int pollIntervalMs = 5000)
            : base(
                $"textfile_{Path.GetFileName(filePath)}", 
                $"Text File Monitor: {Path.GetFileName(filePath)}", 
                $"Monitors {filePath} for content changes", 
                "*")
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _pollIntervalMs = Math.Max(1000, pollIntervalMs); // Minimum 1 second
            _lastContent = "";
        }
        
        protected override void OnStart()
        {
            Logger.Log($"TextFileMonitorHook: Starting monitoring of {_filePath}");
            
            // Initialize with current content if file exists
            if (File.Exists(_filePath))
            {
                try
                {
                    _lastContent = File.ReadAllText(_filePath);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"TextFileMonitorHook: Failed to read initial content", ex);
                }
            }
            
            // Start polling timer
            _pollTimer = new Timer(_pollIntervalMs);
            _pollTimer.Elapsed += PollTimer_Elapsed;
            _pollTimer.AutoReset = true;
            _pollTimer.Start();
        }
        
        protected override void OnStop()
        {
            Logger.Log($"TextFileMonitorHook: Stopping monitoring of {_filePath}");
            
            if (_pollTimer != null)
            {
                _pollTimer.Stop();
                _pollTimer.Dispose();
                _pollTimer = null;
            }
        }
        
        public override bool IsCompatible()
        {
            // Check if file exists or if the directory exists (file might be created later)
            string directory = Path.GetDirectoryName(_filePath);
            return Directory.Exists(directory);
        }
        
        private void PollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Check if file exists
                if (!File.Exists(_filePath))
                {
                    // File was deleted
                    if (!string.IsNullOrEmpty(_lastContent))
                    {
                        TriggerEvent(new AppHookEventArgs
                        {
                            EventType = AppHookEventType.StatusUpdate,
                            Prompt = $"The monitored file {Path.GetFileName(_filePath)} was deleted.",
                            Context = "File deleted",
                            Priority = 1
                        });
                        _lastContent = "";
                    }
                    return;
                }
                
                // Read current content
                string currentContent = File.ReadAllText(_filePath);
                
                // Check if content changed
                if (currentContent != _lastContent)
                {
                    // Determine what kind of change occurred
                    bool wasEmpty = string.IsNullOrEmpty(_lastContent);
                    bool isEmpty = string.IsNullOrEmpty(currentContent);
                    
                    if (wasEmpty && !isEmpty)
                    {
                        // File was created or populated
                        TriggerEvent(new AppHookEventArgs
                        {
                            EventType = AppHookEventType.StatusUpdate,
                            Prompt = $"Content was added to {Path.GetFileName(_filePath)}. React to this new content!",
                            Context = currentContent.Length > 100 
                                ? currentContent.Substring(0, 100) + "..." 
                                : currentContent,
                            Priority = 2
                        });
                    }
                    else if (!wasEmpty && isEmpty)
                    {
                        // File was cleared
                        TriggerEvent(new AppHookEventArgs
                        {
                            EventType = AppHookEventType.StatusUpdate,
                            Prompt = $"The file {Path.GetFileName(_filePath)} was cleared of all content.",
                            Context = "File cleared",
                            Priority = 1
                        });
                    }
                    else if (currentContent.Length > _lastContent.Length)
                    {
                        // Content was added
                        string added = currentContent.Substring(_lastContent.Length);
                        TriggerEvent(new AppHookEventArgs
                        {
                            EventType = AppHookEventType.StatusUpdate,
                            Prompt = $"New content was added to {Path.GetFileName(_filePath)}: {(added.Length > 50 ? added.Substring(0, 50) + "..." : added)}",
                            Context = added,
                            Priority = 1
                        });
                    }
                    else
                    {
                        // Content was modified
                        TriggerEvent(new AppHookEventArgs
                        {
                            EventType = AppHookEventType.StatusUpdate,
                            Prompt = $"The file {Path.GetFileName(_filePath)} was modified.",
                            Context = currentContent.Length > 100 
                                ? currentContent.Substring(0, 100) + "..." 
                                : currentContent,
                            Priority = 1
                        });
                    }
                    
                    _lastContent = currentContent;
                }
            }
            catch (IOException ex)
            {
                // File might be locked - this is common, just log and continue
                Logger.Log($"TextFileMonitorHook: File access error (might be locked): {ex.Message}");
            }
            catch (Exception ex)
            {
                // Other errors should be logged but shouldn't crash the hook
                Logger.LogError($"TextFileMonitorHook: Error polling file", ex);
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _pollTimer?.Dispose();
        }
    }
    
    /// <summary>
    /// Example: How to use this hook
    /// </summary>
    public static class TextFileMonitorExample
    {
        public static void RegisterExample(AppHook.AppHookManager hookManager)
        {
            // Example 1: Monitor a game's save file
            var saveFileHook = new TextFileMonitorHook(
                @"C:\Users\YourName\Documents\MyGame\save.txt",
                pollIntervalMs: 5000
            );
            hookManager.RegisterHook(saveFileHook);
            
            // Example 2: Monitor a log file
            var logFileHook = new TextFileMonitorHook(
                @"C:\Logs\application.log",
                pollIntervalMs: 3000
            );
            hookManager.RegisterHook(logFileHook);
            
            // Start all hooks
            hookManager.StartAll();
        }
    }
}
