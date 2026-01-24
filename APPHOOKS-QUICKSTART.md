# Application Hooks - Quick Start Guide

This guide will help you quickly get started with Application Hooks in MSAgent-AI.

## What are Application Hooks?

Application Hooks allow MSAgent-AI to automatically react to events from games and applications with dynamic AI responses. Your agent can:

- 🎮 React when you start or stop games
- 📝 Respond to window title changes
- 🏆 Celebrate achievements
- 💬 Provide context-aware commentary

## Quick Setup (5 Minutes)

### Step 1: Enable Application Hooks

1. **Open your settings file**: `%AppData%\MSAgentAI\settings.json`
2. **Add the hook configuration**:

```json
{
  "EnableAppHooks": true,
  "EnableOllamaChat": true,
  "AppHooks": [
    {
      "HookType": "ProcessMonitor",
      "DisplayName": "Notepad Monitor",
      "TargetApp": "notepad",
      "Enabled": true,
      "Parameters": {
        "StartPrompt": "The user just opened Notepad. Encourage them with their writing!",
        "StopPrompt": "The user closed Notepad. Ask if they saved their work.",
        "PollInterval": "2000"
      }
    }
  ]
}
```

3. **Save the file** and restart MSAgent-AI

### Step 2: Test It

1. Open Notepad
2. Your agent should react with an AI-generated response!
3. Close Notepad
4. Your agent should ask if you saved your work

## Common Use Cases

### Monitor a Game

```json
{
  "HookType": "ProcessMonitor",
  "DisplayName": "Minecraft Monitor",
  "TargetApp": "javaw",
  "Enabled": true,
  "Parameters": {
    "StartPrompt": "The user started playing Minecraft! Get excited about their adventure!",
    "StopPrompt": "Minecraft session ended. Ask about their builds and adventures.",
    "PollInterval": "2000"
  }
}
```

### Monitor Multiple Applications

```json
{
  "EnableAppHooks": true,
  "AppHooks": [
    {
      "HookType": "ProcessMonitor",
      "DisplayName": "VS Code Monitor",
      "TargetApp": "Code",
      "Enabled": true,
      "Parameters": {
        "StartPrompt": "The user opened VS Code. Wish them productive coding!",
        "StopPrompt": "Coding session ended. Ask how it went!",
        "PollInterval": "2000"
      }
    },
    {
      "HookType": "ProcessMonitor",
      "DisplayName": "Chrome Monitor",
      "TargetApp": "chrome",
      "Enabled": true,
      "Parameters": {
        "StartPrompt": "Browser opened. Ask what they're looking up!",
        "StopPrompt": "Browser closed.",
        "PollInterval": "2000"
      }
    }
  ]
}
```

### Window Title Monitoring

Monitor what the user is doing in an application by watching the window title:

```json
{
  "HookType": "WindowMonitor",
  "DisplayName": "Browser Tab Monitor",
  "TargetApp": "chrome",
  "Enabled": true,
  "Parameters": {
    "PollInterval": "1000"
  }
}
```

This will trigger an AI prompt whenever the window title changes (e.g., switching browser tabs).

## Hook Types

### ProcessMonitor

Detects when an application starts or stops.

**Parameters:**
- `StartPrompt`: What to send to the AI when app starts
- `StopPrompt`: What to send to the AI when app stops  
- `PollInterval`: How often to check (milliseconds, default: 2000)

**Example:**
```json
{
  "HookType": "ProcessMonitor",
  "TargetApp": "notepad",
  "Parameters": {
    "StartPrompt": "User opened Notepad!",
    "StopPrompt": "User closed Notepad!",
    "PollInterval": "2000"
  }
}
```

### WindowMonitor

Monitors window title changes for an application.

**Parameters:**
- `PollInterval`: How often to check window title (milliseconds, default: 1000)

**Example:**
```json
{
  "HookType": "WindowMonitor",
  "TargetApp": "notepad",
  "Parameters": {
    "PollInterval": "1000"
  }
}
```

## Finding Process Names

To monitor an application, you need its process name:

### Windows (PowerShell)
```powershell
Get-Process | Select-Object Name | Sort-Object Name
```

### Windows (Task Manager)
1. Open Task Manager (Ctrl+Shift+Esc)
2. Go to "Details" tab
3. Look at the "Name" column (without .exe)

### Common Process Names
- **Notepad**: `notepad`
- **Chrome**: `chrome`
- **Firefox**: `firefox`
- **VS Code**: `Code`
- **Steam**: `steam`
- **Discord**: `Discord`
- **Spotify**: `Spotify`
- **Minecraft Java**: `javaw`
- **Minecraft Bedrock**: `Minecraft.Windows`

## Tips for Better Prompts

### Be Specific
❌ Bad: "User started app"
✅ Good: "The user just launched Photoshop. Get excited about their creative work!"

### Add Context
❌ Bad: "App closed"
✅ Good: "The user closed their coding session in VS Code. Ask how productive they were and if they need a break!"

### Match Personality
Align prompts with your agent's personality:
- **Enthusiastic**: "OMG! They're starting the game! GET HYPED!"
- **Sarcastic**: "Oh great, another coding session. Try not to break anything."
- **Professional**: "Development environment initialized. Wishing you an efficient session."

## Troubleshooting

### Hook Not Triggering

**Problem**: Agent doesn't react when you open/close an app

**Check:**
1. Is `EnableAppHooks` set to `true`?
2. Is `EnableOllamaChat` set to `true`? (Required for AI responses)
3. Is the hook `Enabled` set to `true`?
4. Is the process name correct? (Check Task Manager)
5. Check the log file: `MSAgentAI.log` for errors

### Wrong Process Name

If your hook isn't working, the process name might be wrong:

1. **Open Task Manager** (Ctrl+Shift+Esc)
2. **Find your application** in the Details tab
3. **Copy the exact name** (without .exe)
4. **Update your config**

Example: For "Visual Studio Code", the process is `Code`, not `vscode`.

### AI Not Responding

**Problem**: Hook triggers but agent doesn't speak

**Check:**
1. Is Ollama running? (Required for AI chat)
2. Is `EnableOllamaChat` enabled in settings?
3. Is a model loaded in Ollama?
4. Check logs for Ollama connection errors

### Performance Issues

**Problem**: Application is slow or laggy

**Solutions:**
1. Increase `PollInterval` (less frequent checks)
   - Recommended: 2000-5000ms for ProcessMonitor
   - Recommended: 1000-2000ms for WindowMonitor
2. Disable unused hooks
3. Reduce the number of active hooks (max 5-10 recommended)

## Advanced Configuration

### Multiple Hooks for Same App

You can have multiple hooks for different behaviors:

```json
{
  "AppHooks": [
    {
      "HookType": "ProcessMonitor",
      "DisplayName": "Game Start/Stop",
      "TargetApp": "MyGame",
      "Enabled": true,
      "Parameters": {
        "StartPrompt": "Game started!",
        "StopPrompt": "Game ended!",
        "PollInterval": "2000"
      }
    },
    {
      "HookType": "WindowMonitor",
      "DisplayName": "Game Window Changes",
      "TargetApp": "MyGame",
      "Enabled": true,
      "Parameters": {
        "PollInterval": "1000"
      }
    }
  ]
}
```

### Conditional Enabling

Enable/disable hooks without removing them:

```json
{
  "HookType": "ProcessMonitor",
  "DisplayName": "Work App Monitor",
  "TargetApp": "Slack",
  "Enabled": false,  // <-- Temporarily disabled
  "Parameters": { ... }
}
```

## Example Configurations

### For Gamers

```json
{
  "EnableAppHooks": true,
  "AppHooks": [
    {
      "HookType": "ProcessMonitor",
      "DisplayName": "Steam Monitor",
      "TargetApp": "steam",
      "Enabled": true,
      "Parameters": {
        "StartPrompt": "Steam launched! Ready to game?",
        "StopPrompt": "Steam closed. Done gaming for today?",
        "PollInterval": "3000"
      }
    },
    {
      "HookType": "ProcessMonitor",
      "DisplayName": "Minecraft Monitor",
      "TargetApp": "javaw",
      "Enabled": true,
      "Parameters": {
        "StartPrompt": "Minecraft started! What are you building today?",
        "StopPrompt": "Minecraft closed. Show me what you built!",
        "PollInterval": "2000"
      }
    },
    {
      "HookType": "ProcessMonitor",
      "DisplayName": "Discord Monitor",
      "TargetApp": "Discord",
      "Enabled": true,
      "Parameters": {
        "StartPrompt": "Discord opened. Chatting with friends?",
        "StopPrompt": "Discord closed.",
        "PollInterval": "2000"
      }
    }
  ]
}
```

### For Developers

```json
{
  "EnableAppHooks": true,
  "AppHooks": [
    {
      "HookType": "ProcessMonitor",
      "DisplayName": "VS Code Monitor",
      "TargetApp": "Code",
      "Enabled": true,
      "Parameters": {
        "StartPrompt": "VS Code opened. Happy coding! Need any tips?",
        "StopPrompt": "Coding session complete. How did it go?",
        "PollInterval": "2000"
      }
    },
    {
      "HookType": "ProcessMonitor",
      "DisplayName": "Git Bash Monitor",
      "TargetApp": "bash",
      "Enabled": true,
      "Parameters": {
        "StartPrompt": "Git Bash opened. Working on version control?",
        "StopPrompt": "Git Bash closed.",
        "PollInterval": "2000"
      }
    }
  ]
}
```

### For Content Creators

```json
{
  "EnableAppHooks": true,
  "AppHooks": [
    {
      "HookType": "ProcessMonitor",
      "DisplayName": "OBS Monitor",
      "TargetApp": "obs64",
      "Enabled": true,
      "Parameters": {
        "StartPrompt": "OBS started! Ready to create content?",
        "StopPrompt": "OBS closed. Stream/recording done?",
        "PollInterval": "2000"
      }
    },
    {
      "HookType": "ProcessMonitor",
      "DisplayName": "Photoshop Monitor",
      "TargetApp": "Photoshop",
      "Enabled": true,
      "Parameters": {
        "StartPrompt": "Photoshop launched! Time to create art!",
        "StopPrompt": "Photoshop closed. Finished your masterpiece?",
        "PollInterval": "2000"
      }
    }
  ]
}
```

## Next Steps

- 📚 Read [APPHOOKS.md](APPHOOKS.md) for full developer documentation
- 🔧 Learn to create custom hooks in C#
- 💡 Check out example hooks in the `examples/` directory
- 🎯 Explore advanced features like priority and interrupts

## Support

- **Issues**: Report bugs on GitHub
- **Discussions**: Ask questions in GitHub Discussions
- **Examples**: Share your hook configs with the community!

---

**Happy Hooking!** 🎣

If you create cool hook configurations, please share them with the community!
