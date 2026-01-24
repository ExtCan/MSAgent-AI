# Application Hooks Developer Guide

MSAgent-AI supports **Application Hooks** - a powerful extensibility system that allows the agent to monitor and react to applications, games, and system events with dynamic AI-powered responses.

## Table of Contents

- [Overview](#overview)
- [How It Works](#how-it-works)
- [Getting Started](#getting-started)
- [Creating Custom Hooks](#creating-custom-hooks)
- [Built-in Hooks](#built-in-hooks)
- [Hook API Reference](#hook-api-reference)
- [Example Scripts](#example-scripts)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

## Overview

Application Hooks allow MSAgent-AI to:
- 🎮 **React to game events** - Celebrate victories, commiserate defeats
- 📝 **Monitor applications** - Respond to window title changes, app launches
- 🏆 **Detect achievements** - Trigger custom reactions to milestones
- ⚡ **Dynamic AI responses** - Send contextual prompts to the AI based on what's happening
- 🔧 **Fully extensible** - Create custom hooks in C# for any scenario

### Use Cases

- **Gaming**: React when a player starts/stops a game, changes levels, or achieves something
- **Development**: Respond to build successes/failures, test results, Git commits
- **Productivity**: Notify about document saves, email arrivals, task completions
- **System Monitoring**: Alert on high CPU usage, low disk space, network issues
- **Streaming**: Integrate with OBS, chat events, follower alerts
- **Automation**: Trigger based on file changes, scheduled tasks, custom events

## How It Works

```
┌─────────────────┐
│  Application    │
│  or Game        │
└────────┬────────┘
         │
         │ (Events: window change, process start/stop, etc.)
         │
         ▼
┌─────────────────┐
│  Application    │
│  Hook           │
└────────┬────────┘
         │
         │ (Triggers with context and prompt)
         │
         ▼
┌─────────────────┐
│  Hook Manager   │
└────────┬────────┘
         │
         │ (Forwards to main app)
         │
         ▼
┌─────────────────┐      ┌──────────────┐
│  MSAgent-AI     │─────▶│  Ollama AI   │
│  Main App       │      │  (Optional)  │
└────────┬────────┘      └──────────────┘
         │
         │ (Speaks response or plays animation)
         │
         ▼
┌─────────────────┐
│  MS Agent       │
│  Character      │
└─────────────────┘
```

**Key Components:**

1. **IAppHook Interface**: Defines the contract all hooks must implement
2. **AppHookBase**: Base class with common functionality for custom hooks
3. **AppHookManager**: Manages lifecycle of all registered hooks
4. **AppHookEventArgs**: Contains event data (prompt, animation, context, priority)
5. **Built-in Hooks**: ProcessMonitorHook, WindowMonitorHook (examples)

## Getting Started

### Enabling Application Hooks

1. Open **MSAgent-AI Settings**
2. Go to the **Hooks** tab (if available) or **Advanced** settings
3. Check **"Enable Application Hooks"**
4. Add hooks you want to use
5. Click **Apply** or **OK**

### Configuration Format

Hooks are configured in `settings.json`:

```json
{
  "EnableAppHooks": true,
  "AppHooks": [
    {
      "HookType": "ProcessMonitor",
      "DisplayName": "Notepad Monitor",
      "TargetApp": "notepad",
      "Enabled": true,
      "Parameters": {
        "StartPrompt": "The user opened Notepad. Say something encouraging about writing!",
        "StopPrompt": "The user closed Notepad. Ask if they saved their work.",
        "PollInterval": "2000"
      }
    }
  ]
}
```

## Creating Custom Hooks

### Basic Hook Template

Create a new C# class implementing `IAppHook` or extending `AppHookBase`:

```csharp
using System;
using MSAgentAI.AppHook;
using MSAgentAI.Logging;

namespace MyGameMod.Hooks
{
    /// <summary>
    /// Custom hook for monitoring My Awesome Game
    /// </summary>
    public class MyGameHook : AppHookBase
    {
        private System.Timers.Timer _checkTimer;
        
        public MyGameHook() 
            : base("my_game_hook", "My Game Monitor", 
                   "Reacts to events in My Awesome Game", "MyGame")
        {
        }
        
        protected override void OnStart()
        {
            // Initialize monitoring (timers, event handlers, etc.)
            Logger.Log("MyGameHook: Starting monitoring...");
            
            _checkTimer = new System.Timers.Timer(5000); // Check every 5 seconds
            _checkTimer.Elapsed += CheckGameState;
            _checkTimer.Start();
        }
        
        protected override void OnStop()
        {
            // Clean up resources
            Logger.Log("MyGameHook: Stopping monitoring...");
            
            if (_checkTimer != null)
            {
                _checkTimer.Stop();
                _checkTimer.Dispose();
                _checkTimer = null;
            }
        }
        
        private void CheckGameState(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Your custom logic here
            // Example: Check a file, read game memory, monitor log files, etc.
            
            bool playerWon = CheckIfPlayerWon(); // Your implementation
            
            if (playerWon)
            {
                // Trigger an event
                TriggerEvent(new AppHookEventArgs
                {
                    EventType = AppHookEventType.Achievement,
                    Prompt = "The player just won the game! Congratulate them enthusiastically!",
                    Animation = "Congratulate", // Optional animation
                    Priority = 3, // Higher priority
                    Interrupt = true // Interrupt current activity
                });
            }
        }
        
        private bool CheckIfPlayerWon()
        {
            // Your game-specific logic
            // Could read: files, shared memory, registry, network, etc.
            return false; // Placeholder
        }
        
        public override bool IsCompatible()
        {
            // Check if this hook can run on the current system
            // For example: check if the game is installed
            return IsProcessRunning("MyGame");
        }
    }
}
```

### Registering Your Hook

In your main initialization code or plugin loader:

```csharp
// Create the hook manager (usually done in MainForm)
var hookManager = new AppHookManager();

// Register your custom hook
hookManager.RegisterHook(new MyGameHook());

// Subscribe to events
hookManager.OnHookTriggered += (sender, args) =>
{
    // Handle the event - send to AI, speak directly, play animation
    if (!string.IsNullOrEmpty(args.Prompt))
    {
        // Send to AI for dynamic response
        SendToAI(args.Prompt);
    }
    else if (!string.IsNullOrEmpty(args.DirectSpeech))
    {
        // Speak directly without AI
        Speak(args.DirectSpeech);
    }
    
    if (!string.IsNullOrEmpty(args.Animation))
    {
        PlayAnimation(args.Animation);
    }
};

// Start all hooks
hookManager.StartAll();
```

## Built-in Hooks

### ProcessMonitorHook

Monitors when a specific application starts or stops.

**Constructor:**
```csharp
new ProcessMonitorHook(
    processName: "notepad",
    displayName: "Notepad Monitor",
    startPrompt: "User opened Notepad. Be encouraging!",
    stopPrompt: "User closed Notepad. Ask if they saved.",
    pollIntervalMs: 2000
)
```

**Example Use Cases:**
- Greet user when they start a game
- React when they close an important app
- Monitor productivity apps

### WindowMonitorHook

Monitors window title changes for a specific application.

**Constructor:**
```csharp
new WindowMonitorHook(
    processName: "chrome",
    displayName: "Chrome Tab Monitor",
    pollIntervalMs: 1000
)
```

**Example Use Cases:**
- React to webpage titles
- Monitor document names in editors
- Track game level/area changes

## Hook API Reference

### IAppHook Interface

```csharp
public interface IAppHook : IDisposable
{
    string HookId { get; }           // Unique identifier
    string DisplayName { get; }      // Human-readable name
    string Description { get; }      // What this hook does
    bool IsActive { get; }           // Current status
    string TargetApplication { get; } // Target app/process
    
    event EventHandler<AppHookEventArgs> OnTrigger;
    
    void Start();                    // Begin monitoring
    void Stop();                     // Stop monitoring
    bool IsCompatible();             // Check if hook can run
}
```

### AppHookEventArgs Properties

```csharp
public class AppHookEventArgs : EventArgs
{
    public AppHookEventType EventType { get; set; }
    public string Prompt { get; set; }        // AI prompt
    public string DirectSpeech { get; set; }  // Direct speech (no AI)
    public string Animation { get; set; }     // Animation to play
    public string Context { get; set; }       // Additional context
    public int Priority { get; set; }         // 0 = normal, higher = more important
    public bool Interrupt { get; set; }       // Interrupt current activity
}
```

### AppHookEventType Enum

```csharp
public enum AppHookEventType
{
    ApplicationStarted,    // App launched
    ApplicationStopped,    // App closed
    WindowTitleChanged,    // Window title changed
    WindowFocused,         // Window gained focus
    WindowUnfocused,       // Window lost focus
    Custom,                // Custom event
    Achievement,           // Achievement/milestone
    Error,                 // Error occurred
    StatusUpdate,          // Status changed
    Periodic               // Periodic check
}
```

### AppHookBase Helper Methods

```csharp
protected abstract class AppHookBase : IAppHook
{
    // Trigger an event to be sent to the AI
    protected void TriggerEvent(AppHookEventArgs args);
    
    // Check if a process is currently running
    protected bool IsProcessRunning(string processName);
    
    // Override these in your hook
    protected abstract void OnStart();
    protected abstract void OnStop();
}
```

## Example Scripts

### Example 1: File Watcher Hook

Monitor a specific file or directory for changes:

```csharp
using System;
using System.IO;
using MSAgentAI.AppHook;

public class FileWatcherHook : AppHookBase
{
    private FileSystemWatcher _watcher;
    private readonly string _path;
    
    public FileWatcherHook(string path) 
        : base($"file_watcher_{Path.GetFileName(path)}", 
               $"File Watcher: {Path.GetFileName(path)}", 
               $"Monitors {path} for changes", "*")
    {
        _path = path;
    }
    
    protected override void OnStart()
    {
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(_path));
        _watcher.Filter = Path.GetFileName(_path);
        _watcher.Changed += OnFileChanged;
        _watcher.EnableRaisingEvents = true;
    }
    
    protected override void OnStop()
    {
        if (_watcher != null)
        {
            _watcher.Dispose();
            _watcher = null;
        }
    }
    
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        TriggerEvent(new AppHookEventArgs
        {
            EventType = AppHookEventType.StatusUpdate,
            Prompt = $"The file {e.Name} was just modified. React to this.",
            Context = e.FullPath
        });
    }
}
```

### Example 2: Game Score Monitor

Read a game's save file or score file:

```csharp
using System;
using System.IO;
using System.Timers;
using MSAgentAI.AppHook;

public class GameScoreHook : AppHookBase
{
    private Timer _pollTimer;
    private int _lastScore = 0;
    private readonly string _scoreFilePath;
    
    public GameScoreHook(string scoreFile) 
        : base("game_score_monitor", "Game Score Monitor", 
               "Monitors game score and reacts to milestones", "Game")
    {
        _scoreFilePath = scoreFile;
    }
    
    protected override void OnStart()
    {
        _pollTimer = new Timer(5000); // Check every 5 seconds
        _pollTimer.Elapsed += CheckScore;
        _pollTimer.Start();
    }
    
    protected override void OnStop()
    {
        _pollTimer?.Stop();
        _pollTimer?.Dispose();
    }
    
    private void CheckScore(object sender, ElapsedEventArgs e)
    {
        try
        {
            if (!File.Exists(_scoreFilePath))
                return;
                
            string scoreText = File.ReadAllText(_scoreFilePath).Trim();
            if (int.TryParse(scoreText, out int currentScore))
            {
                // Check for milestones
                if (currentScore >= 1000 && _lastScore < 1000)
                {
                    TriggerEvent(new AppHookEventArgs
                    {
                        EventType = AppHookEventType.Achievement,
                        Prompt = "The player just reached 1000 points! Congratulate them!",
                        Animation = "Celebrate",
                        Priority = 3
                    });
                }
                else if (currentScore > _lastScore + 100)
                {
                    TriggerEvent(new AppHookEventArgs
                    {
                        EventType = AppHookEventType.StatusUpdate,
                        Prompt = $"The player's score increased by {currentScore - _lastScore}. Encourage them!",
                        Priority = 1
                    });
                }
                
                _lastScore = currentScore;
            }
        }
        catch (Exception ex)
        {
            Logging.Logger.LogError("GameScoreHook: Error reading score", ex);
        }
    }
}
```

### Example 3: Named Pipe Integration Hook

For games that support custom named pipes:

```csharp
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MSAgentAI.AppHook;

public class NamedPipeGameHook : AppHookBase
{
    private CancellationTokenSource _cts;
    private readonly string _pipeName;
    
    public NamedPipeGameHook(string pipeName) 
        : base($"pipe_{pipeName}", $"Pipe: {pipeName}", 
               $"Listens to game events from pipe {pipeName}", "*")
    {
        _pipeName = pipeName;
    }
    
    protected override void OnStart()
    {
        _cts = new CancellationTokenSource();
        Task.Run(() => ListenToPipe(_cts.Token));
    }
    
    protected override void OnStop()
    {
        _cts?.Cancel();
    }
    
    private async Task ListenToPipe(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using (var server = new NamedPipeServerStream(_pipeName, PipeDirection.In))
                {
                    await server.WaitForConnectionAsync(ct);
                    
                    using (var reader = new StreamReader(server, Encoding.UTF8))
                    {
                        string message = await reader.ReadLineAsync();
                        
                        if (!string.IsNullOrEmpty(message))
                        {
                            TriggerEvent(new AppHookEventArgs
                            {
                                EventType = AppHookEventType.Custom,
                                Prompt = $"Game event: {message}",
                                Context = message
                            });
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logging.Logger.LogError("NamedPipeGameHook: Error", ex);
            }
        }
    }
}
```

### Example 4: Web API Hook

Monitor a web API or local server:

```csharp
using System;
using System.Net.Http;
using System.Timers;
using Newtonsoft.Json;
using MSAgentAI.AppHook;

public class WebApiHook : AppHookBase
{
    private Timer _pollTimer;
    private readonly string _apiUrl;
    private readonly HttpClient _httpClient;
    private string _lastState;
    
    public WebApiHook(string apiUrl) 
        : base("web_api_monitor", "Web API Monitor", 
               "Monitors a web API for changes", "*")
    {
        _apiUrl = apiUrl;
        _httpClient = new HttpClient();
        _lastState = "";
    }
    
    protected override void OnStart()
    {
        _pollTimer = new Timer(10000); // Poll every 10 seconds
        _pollTimer.Elapsed += PollApi;
        _pollTimer.Start();
    }
    
    protected override void OnStop()
    {
        _pollTimer?.Stop();
        _pollTimer?.Dispose();
    }
    
    private async void PollApi(object sender, ElapsedEventArgs e)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(_apiUrl);
            
            if (response != _lastState)
            {
                // Parse the response (example assumes JSON)
                dynamic data = JsonConvert.DeserializeObject(response);
                
                TriggerEvent(new AppHookEventArgs
                {
                    EventType = AppHookEventType.StatusUpdate,
                    Prompt = $"API status changed: {data.status}",
                    Context = response
                });
                
                _lastState = response;
            }
        }
        catch (Exception ex)
        {
            Logging.Logger.LogError("WebApiHook: Error polling API", ex);
        }
    }
    
    public override void Dispose()
    {
        base.Dispose();
        _httpClient?.Dispose();
    }
}
```

## Best Practices

### Performance

1. **Use appropriate poll intervals**: Don't poll too frequently (< 500ms) unless necessary
2. **Dispose resources**: Always dispose timers, file watchers, network connections
3. **Handle exceptions**: Wrap poll/event code in try-catch to prevent crashes
4. **Async operations**: Use async/await for I/O operations

### Event Design

1. **Clear prompts**: Make prompts descriptive for better AI responses
2. **Set priority**: Use priority levels to control which events are more important
3. **Use context**: Provide relevant context data for logging and debugging
4. **Interrupt wisely**: Only set `Interrupt = true` for critical events

### Compatibility

1. **Check compatibility**: Implement `IsCompatible()` to check if hook can run
2. **Graceful degradation**: Handle missing files, processes, or APIs gracefully
3. **Platform-specific code**: Use conditional compilation or runtime checks for Windows-only APIs

### Security

1. **Validate input**: Sanitize any data from external sources
2. **File paths**: Validate and sanitize file paths before accessing
3. **Network requests**: Use HTTPS and validate certificates when possible
4. **Don't expose credentials**: Never hardcode API keys or passwords

### Example Best Practice Hook

```csharp
public class BestPracticeHook : AppHookBase
{
    private Timer _timer;
    private readonly int _intervalMs;
    
    public BestPracticeHook(int intervalMs = 5000) 
        : base("best_practice", "Best Practice Example", 
               "Example of a well-designed hook", "*")
    {
        _intervalMs = Math.Max(1000, intervalMs); // Minimum 1 second
    }
    
    protected override void OnStart()
    {
        try
        {
            _timer = new Timer(_intervalMs);
            _timer.Elapsed += SafeTimerHandler;
            _timer.AutoReset = true;
            _timer.Start();
        }
        catch (Exception ex)
        {
            Logging.Logger.LogError("BestPracticeHook: Failed to start", ex);
            throw; // Re-throw so caller knows initialization failed
        }
    }
    
    protected override void OnStop()
    {
        try
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }
        catch (Exception ex)
        {
            Logging.Logger.LogError("BestPracticeHook: Error during stop", ex);
        }
    }
    
    private void SafeTimerHandler(object sender, ElapsedEventArgs e)
    {
        // Always wrap timer/event handlers in try-catch
        try
        {
            DoWork();
        }
        catch (Exception ex)
        {
            Logging.Logger.LogError("BestPracticeHook: Error in timer handler", ex);
            // Don't throw - would crash the timer
        }
    }
    
    private void DoWork()
    {
        // Your monitoring logic here
    }
    
    public override bool IsCompatible()
    {
        // Check prerequisites
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            return false;
            
        // Check if required resources exist
        // return File.Exists(requiredFile);
        
        return true;
    }
    
    public override void Dispose()
    {
        base.Dispose(); // Calls Stop()
        // Dispose any additional resources here
    }
}
```

## Troubleshooting

### Hook Not Starting

**Problem**: Hook is registered but `OnStart()` is never called

**Solutions**:
1. Check if `EnableAppHooks` is set to `true` in settings
2. Verify `IsCompatible()` returns `true`
3. Check logs for error messages
4. Ensure `hookManager.StartAll()` is called

### Events Not Triggering

**Problem**: Hook is active but events aren't reaching the AI

**Solutions**:
1. Verify `OnHookTriggered` event is subscribed in main app
2. Check if `TriggerEvent()` is being called (add logging)
3. Ensure hook is `IsActive = true`
4. Verify `AppHookEventArgs` contains valid data

### Performance Issues

**Problem**: Application becomes slow or unresponsive

**Solutions**:
1. Increase poll intervals (reduce frequency)
2. Profile your hook code for bottlenecks
3. Use async/await for I/O operations
4. Consider using file system watchers instead of polling
5. Limit the number of active hooks

### Memory Leaks

**Problem**: Memory usage grows over time

**Solutions**:
1. Ensure `Dispose()` is called when hook stops
2. Unsubscribe from events in `OnStop()`
3. Dispose timers, file watchers, HTTP clients
4. Use `using` statements for IDisposable resources

### Integration Issues

**Problem**: Can't get game/app data

**Solutions**:
1. **Files**: Check file permissions, verify path exists
2. **Processes**: Check process name (without .exe), verify running
3. **Network**: Check firewall, verify endpoint is accessible
4. **Memory**: Use appropriate memory reading libraries (careful with anti-cheat)

### Debugging Tips

1. **Enable verbose logging**: Add `Logger.Log()` calls liberally
2. **Test in isolation**: Test your hook separately before integrating
3. **Use breakpoints**: Attach debugger to MSAgent-AI process
4. **Check compatibility**: Verify `IsCompatible()` returns correct value
5. **Monitor resources**: Use Task Manager to check CPU/memory usage

## Advanced Topics

### Custom Hook Configuration UI

To add UI for configuring hooks, you would extend the SettingsForm:

```csharp
// In SettingsForm.cs
private void LoadHookSettings()
{
    listViewHooks.Items.Clear();
    
    foreach (var hookConfig in _settings.AppHooks)
    {
        var item = new ListViewItem(hookConfig.DisplayName);
        item.SubItems.Add(hookConfig.HookType);
        item.SubItems.Add(hookConfig.TargetApp);
        item.SubItems.Add(hookConfig.Enabled ? "Yes" : "No");
        item.Tag = hookConfig;
        listViewHooks.Items.Add(item);
    }
}
```

### Dynamic Hook Loading

For loading hooks from external assemblies:

```csharp
// Advanced: Load hooks from DLL files
public void LoadHooksFromDirectory(string directory)
{
    var dllFiles = Directory.GetFiles(directory, "*.dll");
    
    foreach (var dll in dllFiles)
    {
        try
        {
            var assembly = Assembly.LoadFrom(dll);
            var hookTypes = assembly.GetTypes()
                .Where(t => typeof(IAppHook).IsAssignableFrom(t) && !t.IsAbstract);
            
            foreach (var type in hookTypes)
            {
                var hook = (IAppHook)Activator.CreateInstance(type);
                RegisterHook(hook);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load hooks from {dll}", ex);
        }
    }
}
```

### Hook Scripting with Roslyn

For advanced scenarios, you could allow users to write hook scripts in C#:

```csharp
// Would require Microsoft.CodeAnalysis.CSharp package
// This is an advanced feature - not implemented in base system
```

## Contributing

Want to share your custom hooks with the community?

1. Create your hook following best practices
2. Add comprehensive comments and documentation
3. Include example usage
4. Test thoroughly on different systems
5. Submit a pull request or share in discussions

## Support

- **GitHub Issues**: Report bugs or request features
- **Discussions**: Ask questions and share hooks
- **Wiki**: Community-contributed hook examples
- **Discord**: Real-time help and community

## License

Application hooks follow the same MIT license as MSAgent-AI.

---

**Happy Hooking!** 🎣

For more information, see:
- [README.md](README.md) - Main documentation
- [PIPELINE.md](PIPELINE.md) - External communication
- API Reference - In-code documentation
