using System;
using System.Diagnostics;
using System.Timers;

namespace MSAgentAI.AppHook.Hooks
{
    /// <summary>
    /// Example hook that monitors when a specific application starts or stops
    /// Can be used to greet the user when they start a game or application
    /// </summary>
    public class ProcessMonitorHook : AppHookBase
    {
        private Timer _pollTimer;
        private bool _wasRunning;
        private readonly int _pollIntervalMs;
        private readonly string _processName;
        private readonly string _startPrompt;
        private readonly string _stopPrompt;
        
        /// <summary>
        /// Creates a process monitor hook
        /// </summary>
        /// <param name="processName">Name of process to monitor (without .exe)</param>
        /// <param name="displayName">Display name for this hook</param>
        /// <param name="startPrompt">Prompt to send when app starts</param>
        /// <param name="stopPrompt">Prompt to send when app stops</param>
        /// <param name="pollIntervalMs">Check interval in milliseconds</param>
        public ProcessMonitorHook(
            string processName, 
            string displayName = null,
            string startPrompt = null,
            string stopPrompt = null,
            int pollIntervalMs = 2000)
            : base($"process_monitor_{processName}", 
                   displayName ?? $"{processName} Monitor", 
                   $"Monitors when {processName} starts or stops", 
                   processName)
        {
            _processName = processName;
            _pollIntervalMs = pollIntervalMs;
            _startPrompt = startPrompt ?? $"The user just started {processName}. React to this.";
            _stopPrompt = stopPrompt ?? $"The user just closed {processName}. React to this.";
            _wasRunning = false;
        }
        
        protected override void OnStart()
        {
            // Initialize current state
            _wasRunning = IsProcessRunning(_processName);
            
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
        
        private void PollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                bool isRunning = IsProcessRunning(_processName);
                
                // Detect state changes
                if (isRunning && !_wasRunning)
                {
                    // Process started
                    TriggerEvent(new AppHookEventArgs
                    {
                        EventType = AppHookEventType.ApplicationStarted,
                        Prompt = _startPrompt,
                        Context = _processName,
                        Priority = 2,
                        Interrupt = false
                    });
                }
                else if (!isRunning && _wasRunning)
                {
                    // Process stopped
                    TriggerEvent(new AppHookEventArgs
                    {
                        EventType = AppHookEventType.ApplicationStopped,
                        Prompt = _stopPrompt,
                        Context = _processName,
                        Priority = 1,
                        Interrupt = false
                    });
                }
                
                _wasRunning = isRunning;
            }
            catch (Exception ex)
            {
                Logging.Logger.LogError($"ProcessMonitorHook: Error monitoring {_processName}", ex);
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _pollTimer?.Dispose();
        }
    }
}
