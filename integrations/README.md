# MSAgent-AI Game & Application Integrations

This directory contains integrations that allow MSAgent-AI to interact with external applications, games, and mods through the Named Pipe API.

## Available Integrations

### GTA V ScriptHookDotNet Integration
**Location:** `GTAV-ScriptHookDotNet/`

A C# ScriptHookDotNet script that provides live AI commentary for Grand Theft Auto V.

**Key Features:**
- Real-time reactions to in-game events
- Vehicle detection and commentary (cars, bikes, boats, planes, helicopters)
- Environment monitoring (weather, time, location)
- Character health and status monitoring
- Wanted level reactions
- In-game menu for configuration ([ key)
- Toggleable reaction categories
- Live commentary mode

**Requirements:**
- GTA V (PC version)
- ScriptHookDotNet v3
- MSAgent-AI running
- Visual Studio 2019+ (to build from source - optional)

**Getting Started:**
1. Download ScriptHookDotNet v3
2. Copy DLL to GTA V/scripts folder (pre-built available)
3. Launch and press [ in-game
4. See [Quick Start Guide](GTAV-ScriptHookDotNet/QUICKSTART.md) for details

**Advantages over ScriptHook V:**
- ✅ No SDK required
- ✅ C# instead of C++
- ✅ Easier to build and modify
- ✅ Pre-built DLL can be distributed

**Documentation:**
- [Full README](GTAV-ScriptHookDotNet/README.md) - Complete documentation
- [Quick Start Guide](GTAV-ScriptHookDotNet/QUICKSTART.md) - Installation steps

## How Integrations Work

All integrations communicate with MSAgent-AI through the **Named Pipe API**:

1. Integration connects to `\\.\pipe\MSAgentAI`
2. Sends text commands (SPEAK, CHAT, ANIMATION, etc.)
3. MSAgent-AI processes and responds

See [PIPELINE.md](../PIPELINE.md) for the complete API specification.

## Creating Your Own Integration

### Basic Template (C++)

```cpp
#include <windows.h>
#include <string>

void SendToMSAgent(const std::string& command) {
    HANDLE hPipe = CreateFileA(
        "\\\\.\\pipe\\MSAgentAI",
        GENERIC_READ | GENERIC_WRITE,
        0, NULL, OPEN_EXISTING, 0, NULL
    );
    
    if (hPipe == INVALID_HANDLE_VALUE) return;
    
    DWORD mode = PIPE_READMODE_MESSAGE;
    SetNamedPipeHandleState(hPipe, &mode, NULL, NULL);
    
    std::string message = command + "\n";
    DWORD bytesWritten;
    WriteFile(hPipe, message.c_str(), message.length(), &bytesWritten, NULL);
    
    char buffer[1024];
    DWORD bytesRead;
    ReadFile(hPipe, buffer, sizeof(buffer) - 1, &bytesRead, NULL);
    
    CloseHandle(hPipe);
}

// Usage
SendToMSAgent("SPEAK:Hello from my game!");
SendToMSAgent("CHAT:The player just scored a goal!");
```

### Basic Template (Python)

```python
import win32pipe
import win32file

def send_to_msagent(command):
    pipe = win32file.CreateFile(
        r'\\.\pipe\MSAgentAI',
        win32file.GENERIC_READ | win32file.GENERIC_WRITE,
        0, None, win32file.OPEN_EXISTING, 0, None
    )
    
    win32file.WriteFile(pipe, (command + '\n').encode())
    result, data = win32file.ReadFile(pipe, 1024)
    
    win32file.CloseHandle(pipe)
    return data.decode().strip()

# Usage
send_to_msagent("SPEAK:Hello from Python!")
send_to_msagent("CHAT:Make a comment about this game event")
```

### Integration Guidelines

When creating an integration:

1. **Check Connection**: Always verify MSAgent-AI is running before sending commands
2. **Use CHAT for AI**: For contextual responses, use `CHAT:prompt` instead of `SPEAK:text`
3. **Throttle Events**: Don't spam commands - space them out or batch similar events
4. **Provide Context**: When using CHAT, give the AI enough context to generate relevant responses
5. **Handle Errors**: Gracefully handle connection failures
6. **Test Thoroughly**: Ensure your integration doesn't impact game/app performance

## Integration Ideas

### Potential Integrations

**Games:**
- Minecraft (via Forge/Fabric mod)
- Counter-Strike (via SourceMod)
- World of Warcraft (via addon)
- Flight Simulator (via SimConnect)
- Any game with Lua scripting support

**Applications:**
- OBS Studio (streaming integration)
- Discord bot (chat reactions)
- Visual Studio (build notifications)
- Browser extension (webpage reactions)
- System monitor (performance alerts)

**Automation:**
- PowerShell scripts
- Task Scheduler events
- File system watchers
- Network monitors
- Smart home integrations

## Directory Structure

```
integrations/
├── README.md                    # This file
├── GTAV-ScriptHookV/           # GTA V integration
│   ├── script.cpp              # Main script
│   ├── keyboard.h              # Input handling
│   ├── MSAgentGTA.vcxproj      # VS project
│   ├── MSAgentGTA.sln          # VS solution
│   ├── README.md               # Full documentation
│   ├── QUICKSTART.md           # Quick start guide
│   ├── inc/                    # SDK headers (placeholders)
│   └── lib/                    # SDK library
└── [future integrations...]
```

## Contributing

Have an integration to share? We'd love to see it!

**Steps:**
1. Create a new folder in `integrations/`
2. Include comprehensive documentation
3. Add example code and build instructions
4. Update this README with your integration
5. Submit a pull request

**Requirements:**
- Must use the Named Pipe API
- Include clear installation steps
- Provide usage examples
- Document any dependencies
- Follow the template structure

## Support

For integration development help:
1. Review [PIPELINE.md](../PIPELINE.md) for API details
2. Check existing integrations for examples
3. Test with simple PING/SPEAK commands first
4. Use logging to debug connection issues
5. Open an issue if you need assistance

## License

All integrations follow the main project's MIT License unless otherwise specified.

---

**Happy integrating!** 🎮🤖
