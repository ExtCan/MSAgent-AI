using System;
using System.Diagnostics;
using MSAgentAI.Logging;

namespace MSAgentAI.AppHook
{
    /// <summary>
    /// Base class for application hooks providing common functionality
    /// </summary>
    public abstract class AppHookBase : IAppHook
    {
        public string HookId { get; protected set; }
        public string DisplayName { get; protected set; }
        public string Description { get; protected set; }
        public bool IsActive { get; protected set; }
        public string TargetApplication { get; protected set; }
        
        public event EventHandler<AppHookEventArgs> OnTrigger;
        
        protected AppHookBase(string hookId, string displayName, string description, string targetApp)
        {
            HookId = hookId ?? throw new ArgumentNullException(nameof(hookId));
            DisplayName = displayName ?? hookId;
            Description = description ?? "";
            TargetApplication = targetApp ?? "*";
            IsActive = false;
        }
        
        public virtual void Start()
        {
            if (IsActive)
                return;
                
            Logger.Log($"AppHook: Starting hook '{DisplayName}' (ID: {HookId})");
            
            try
            {
                OnStart();
                IsActive = true;
                Logger.Log($"AppHook: Hook '{DisplayName}' started successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError($"AppHook: Failed to start hook '{DisplayName}'", ex);
                throw;
            }
        }
        
        public virtual void Stop()
        {
            if (!IsActive)
                return;
                
            Logger.Log($"AppHook: Stopping hook '{DisplayName}' (ID: {HookId})");
            
            try
            {
                OnStop();
                IsActive = false;
                Logger.Log($"AppHook: Hook '{DisplayName}' stopped successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError($"AppHook: Error stopping hook '{DisplayName}'", ex);
            }
        }
        
        public virtual bool IsCompatible()
        {
            // Default implementation - can be overridden
            return true;
        }
        
        /// <summary>
        /// Called when the hook should start monitoring
        /// </summary>
        protected abstract void OnStart();
        
        /// <summary>
        /// Called when the hook should stop monitoring
        /// </summary>
        protected abstract void OnStop();
        
        /// <summary>
        /// Triggers an event to send to the AI
        /// </summary>
        protected void TriggerEvent(AppHookEventArgs args)
        {
            if (!IsActive)
                return;
                
            Logger.Log($"AppHook: '{DisplayName}' triggered event type '{args.EventType}'");
            OnTrigger?.Invoke(this, args);
        }
        
        /// <summary>
        /// Helper to check if a process is running
        /// </summary>
        protected bool IsProcessRunning(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName.Replace(".exe", ""));
                return processes.Length > 0;
            }
            catch (Exception ex)
            {
                Logger.LogError($"AppHook: Error checking process '{processName}'", ex);
                return false;
            }
        }
        
        public virtual void Dispose()
        {
            Stop();
        }
    }
}
