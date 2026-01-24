using System;

namespace MSAgentAI.AppHook
{
    /// <summary>
    /// Interface for application hooks that can monitor and react to application events
    /// </summary>
    public interface IAppHook : IDisposable
    {
        /// <summary>
        /// Unique identifier for this hook
        /// </summary>
        string HookId { get; }
        
        /// <summary>
        /// Display name for this hook
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// Description of what this hook does
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Whether this hook is currently active
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Target application or process name (can be wildcard)
        /// </summary>
        string TargetApplication { get; }
        
        /// <summary>
        /// Event raised when the hook wants to send a prompt to the AI
        /// </summary>
        event EventHandler<AppHookEventArgs> OnTrigger;
        
        /// <summary>
        /// Starts monitoring for events
        /// </summary>
        void Start();
        
        /// <summary>
        /// Stops monitoring
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Checks if this hook is compatible with the current system/application
        /// </summary>
        bool IsCompatible();
    }
    
    /// <summary>
    /// Event arguments for application hook triggers
    /// </summary>
    public class AppHookEventArgs : EventArgs
    {
        /// <summary>
        /// Type of event that occurred
        /// </summary>
        public AppHookEventType EventType { get; set; }
        
        /// <summary>
        /// Prompt to send to the AI
        /// </summary>
        public string Prompt { get; set; }
        
        /// <summary>
        /// Optional text to speak directly (bypasses AI)
        /// </summary>
        public string DirectSpeech { get; set; }
        
        /// <summary>
        /// Optional animation to play
        /// </summary>
        public string Animation { get; set; }
        
        /// <summary>
        /// Additional context data
        /// </summary>
        public string Context { get; set; }
        
        /// <summary>
        /// Priority level (0 = normal, higher = more important)
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// Whether this event should interrupt current speech/activity
        /// </summary>
        public bool Interrupt { get; set; }
    }
    
    /// <summary>
    /// Types of events that can trigger hooks
    /// </summary>
    public enum AppHookEventType
    {
        /// <summary>
        /// Application started
        /// </summary>
        ApplicationStarted,
        
        /// <summary>
        /// Application stopped/closed
        /// </summary>
        ApplicationStopped,
        
        /// <summary>
        /// Window title changed
        /// </summary>
        WindowTitleChanged,
        
        /// <summary>
        /// Window became focused
        /// </summary>
        WindowFocused,
        
        /// <summary>
        /// Window lost focus
        /// </summary>
        WindowUnfocused,
        
        /// <summary>
        /// Custom event from script
        /// </summary>
        Custom,
        
        /// <summary>
        /// Achievement or milestone reached
        /// </summary>
        Achievement,
        
        /// <summary>
        /// Error or failure occurred
        /// </summary>
        Error,
        
        /// <summary>
        /// Status update
        /// </summary>
        StatusUpdate,
        
        /// <summary>
        /// Periodic check/poll
        /// </summary>
        Periodic
    }
}
