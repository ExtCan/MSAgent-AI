using System;
using System.Collections.Generic;
using System.Linq;
using MSAgentAI.Logging;

namespace MSAgentAI.AppHook
{
    /// <summary>
    /// Manages all application hooks and their lifecycle
    /// </summary>
    public class AppHookManager : IDisposable
    {
        private readonly Dictionary<string, IAppHook> _hooks;
        private bool _isRunning;
        
        /// <summary>
        /// Event raised when any hook triggers
        /// </summary>
        public event EventHandler<AppHookEventArgs> OnHookTriggered;
        
        public AppHookManager()
        {
            _hooks = new Dictionary<string, IAppHook>();
            _isRunning = false;
        }
        
        /// <summary>
        /// Registers a new hook
        /// </summary>
        public void RegisterHook(IAppHook hook)
        {
            if (hook == null)
                throw new ArgumentNullException(nameof(hook));
                
            if (_hooks.ContainsKey(hook.HookId))
            {
                Logger.Log($"AppHookManager: Hook '{hook.HookId}' already registered, replacing...");
                UnregisterHook(hook.HookId);
            }
            
            _hooks[hook.HookId] = hook;
            hook.OnTrigger += Hook_OnTrigger;
            
            Logger.Log($"AppHookManager: Registered hook '{hook.DisplayName}' (ID: {hook.HookId})");
            
            // Auto-start if manager is already running
            if (_isRunning && hook.IsCompatible())
            {
                try
                {
                    hook.Start();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"AppHookManager: Failed to auto-start hook '{hook.HookId}'", ex);
                }
            }
        }
        
        /// <summary>
        /// Unregisters a hook by ID
        /// </summary>
        public void UnregisterHook(string hookId)
        {
            if (_hooks.TryGetValue(hookId, out var hook))
            {
                hook.OnTrigger -= Hook_OnTrigger;
                hook.Dispose();
                _hooks.Remove(hookId);
                Logger.Log($"AppHookManager: Unregistered hook '{hookId}'");
            }
        }
        
        /// <summary>
        /// Gets a hook by ID
        /// </summary>
        public IAppHook GetHook(string hookId)
        {
            return _hooks.TryGetValue(hookId, out var hook) ? hook : null;
        }
        
        /// <summary>
        /// Gets all registered hooks
        /// </summary>
        public IEnumerable<IAppHook> GetAllHooks()
        {
            return _hooks.Values.ToList();
        }
        
        /// <summary>
        /// Starts all compatible hooks
        /// </summary>
        public void StartAll()
        {
            if (_isRunning)
                return;
                
            Logger.Log("AppHookManager: Starting all compatible hooks...");
            _isRunning = true;
            
            foreach (var hook in _hooks.Values)
            {
                if (!hook.IsCompatible())
                {
                    Logger.Log($"AppHookManager: Hook '{hook.HookId}' is not compatible, skipping");
                    continue;
                }
                
                try
                {
                    if (!hook.IsActive)
                        hook.Start();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"AppHookManager: Failed to start hook '{hook.HookId}'", ex);
                }
            }
        }
        
        /// <summary>
        /// Stops all active hooks
        /// </summary>
        public void StopAll()
        {
            if (!_isRunning)
                return;
                
            Logger.Log("AppHookManager: Stopping all hooks...");
            _isRunning = false;
            
            foreach (var hook in _hooks.Values)
            {
                try
                {
                    if (hook.IsActive)
                        hook.Stop();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"AppHookManager: Error stopping hook '{hook.HookId}'", ex);
                }
            }
        }
        
        /// <summary>
        /// Starts a specific hook by ID
        /// </summary>
        public void StartHook(string hookId)
        {
            if (_hooks.TryGetValue(hookId, out var hook))
            {
                if (!hook.IsCompatible())
                {
                    Logger.Log($"AppHookManager: Hook '{hookId}' is not compatible");
                    return;
                }
                
                try
                {
                    hook.Start();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"AppHookManager: Failed to start hook '{hookId}'", ex);
                    throw;
                }
            }
        }
        
        /// <summary>
        /// Stops a specific hook by ID
        /// </summary>
        public void StopHook(string hookId)
        {
            if (_hooks.TryGetValue(hookId, out var hook))
            {
                try
                {
                    hook.Stop();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"AppHookManager: Error stopping hook '{hookId}'", ex);
                }
            }
        }
        
        private void Hook_OnTrigger(object sender, AppHookEventArgs e)
        {
            // Forward the event to listeners
            OnHookTriggered?.Invoke(sender, e);
        }
        
        public void Dispose()
        {
            StopAll();
            
            foreach (var hook in _hooks.Values.ToList())
            {
                hook.OnTrigger -= Hook_OnTrigger;
                hook.Dispose();
            }
            
            _hooks.Clear();
        }
    }
}
