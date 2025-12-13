# GTA V MSAgent-AI Integration (ScriptHookDotNet)

This ScriptHookDotNet script integrates Grand Theft Auto V with MSAgent-AI, allowing your Microsoft Agent character to react to in-game events in real-time through AI-powered commentary.

## Features

### Real-Time Reactions
- **Vehicle Events**: Reacts when you enter/exit vehicles, with commentary based on vehicle type, class, and estimated value
- **Environment Changes**: Comments on weather changes, time of day transitions, and location changes
- **Character Events**: Reacts to health changes and low health warnings
- **General Events**: Responds to wanted level changes and provides periodic commentary
- **Live Commentary**: Optional 5-minute interval commentary about current gameplay

### In-Game Menu
Press **[** (left bracket) to open the MSAgent Reactions menu with the following options:
- Vehicle Reactions (ON/OFF)
- Mission Reactions (ON/OFF)
- Environment Reactions (ON/OFF)
- Character Reactions (ON/OFF)
- General Reactions (ON/OFF)
- Live Commentary (ON/OFF)

Navigate with **Arrow Keys**, toggle settings with **Enter**, and close with **[** key.

## Prerequisites

### Required Software
1. **Grand Theft Auto V** (obviously!)
2. **ScriptHookDotNet v3** - Download from: https://github.com/scripthookvdotnet/scripthookvdotnet/releases
3. **MSAgent-AI** - Must be running before launching GTA V
   - Download from the main repository
   - Ensure Ollama is set up for AI responses

### Development Requirements (for building)
1. **Visual Studio 2019 or later** with .NET Framework 4.8
2. **ScriptHookDotNet v3** - Download from: https://github.com/scripthookvdotnet/scripthookvdotnet/releases

## Installation

⚠️ **Much simpler than ScriptHook V!** No SDK required, just copy the DLL.

### Step 1: Install ScriptHookDotNet Runtime

1. Download ScriptHookDotNet v3 from https://github.com/scripthookvdotnet/scripthookvdotnet/releases
2. Extract and copy these files to your GTA V directory:
   - `ScriptHookVDotNet3.dll`
   - `ScriptHookV.dll` (included with SHVDN)

### Step 2: Get the Script

**Option A: Download Pre-built DLL from GitHub Actions (Easiest)**
1. Go to the [Actions tab](../../actions/workflows/build-gtav-dotnet.yml)
2. Find the latest successful workflow run
3. Download the `MSAgentGTA-ScriptHookDotNet-*` artifact
4. Extract `MSAgentGTA.dll` from the artifact
5. Copy to `GTA V/scripts/` folder (create the folder if it doesn't exist)

**Option B: Download from Releases**
1. Check the [Releases](../../releases) page for pre-built versions
2. Download `MSAgentGTA.dll`
3. Copy to `GTA V/scripts/` folder

**Option C: Build from Source**
1. Open `MSAgentGTA.csproj` in Visual Studio
2. Add reference to `ScriptHookVDotNet3.dll` from your GTA V installation
3. Build in Release mode (Ctrl+Shift+B)
4. Copy `bin/Release/MSAgentGTA.dll` to `GTA V/scripts/` folder

### Step 3: Launch

1. Start MSAgent-AI application
2. Launch GTA V
3. Press **[** (left bracket) in-game to open the menu

## Building the Script

### Quick Build (30 seconds)

1. **Download ScriptHookDotNet v3** from https://github.com/scripthookvdotnet/scripthookvdotnet/releases

2. **Set up the reference:**
   - Either set the `GTAV` environment variable to your GTA V path
   - Or manually add reference to `ScriptHookVDotNet3.dll` in the project

3. **Build in Visual Studio:**
   - Open `MSAgentGTA.csproj`
   - Configuration: **Release**, Platform: **Any CPU**
   - Press **Ctrl+Shift+B** to build
   - DLL created at: `bin/Release/MSAgentGTA.dll`

**Much easier than ScriptHook V** - No complex SDK setup, just a standard C# project!

### Setting Up the Build Environment

1. **Download ScriptHook V SDK**:
   - Visit http://www.dev-c.com/gtav/scripthookv/
   - Download the SDK package
   - Extract and locate the `SDK` folder

2. **Copy SDK Files**:
   ```
   Copy these files from SDK to integrations/GTAV-ScriptHookV/inc/:
   - main.h
   - natives.h
   - types.h
   - enums.h
   ```

3. **Copy ScriptHook V Library**:
   ```
   Copy ScriptHookV.lib to integrations/GTAV-ScriptHookV/lib/
   ```

4. **Open in Visual Studio**:
   - Open `MSAgentGTA.sln` in Visual Studio
   - Select Release configuration
   - Select x86 platform
   - Build the solution

5. **Install the ASI**:
   - The build output will be in `Release/MSAgentGTA.asi`
   - Copy this file to your GTA V installation directory

### Manual Build (Command Line)

If you prefer to build from command line:

```bash
# Using Visual Studio Developer Command Prompt
cd integrations/GTAV-ScriptHookV
cl /O2 /EHsc /LD /Fe:MSAgentGTA.asi script.cpp /link /DEF:exports.def ScriptHookV.lib
```

## Configuration

### Default Keybinding
- **[** (left bracket key) - Opens/closes the reactions menu

To change the keybinding, edit the `menuKey` value in the script (requires rebuild):
```cpp
Settings g_Settings;
// Change 0xDB to desired key code (e.g., VK_F8, VK_F9, VK_F10)
// 0xDB = '[' key, VK_F9 = F9 key
g_Settings.menuKey = 0xDB;
```

### Adjusting Commentary Frequency
The script provides random commentary every 5 minutes by default. To adjust:

1. Open `script.cpp`
2. Find the `CheckGeneralEvents()` function
3. Modify the time interval:
```cpp
if (elapsed.count() >= 5) {  // Change 5 to desired minutes
```

## How It Works

### Named Pipe Communication
The script communicates with MSAgent-AI through Windows Named Pipes:
- Pipe name: `\\.\pipe\MSAgentAI`
- Protocol: Text-based commands
- Commands used:
  - `SPEAK:text` - Quick announcements
  - `CHAT:prompt` - AI-powered contextual commentary

### Event Detection
The script continuously monitors:
1. **Player state** - Position, health, vehicle status
2. **Environment** - Weather, time, location zones
3. **Game events** - Missions, wanted level, character switches

When changes are detected, appropriate prompts are sent to MSAgent-AI for natural language responses.

### Performance
- Minimal performance impact (~0.1% CPU usage)
- Events are throttled to prevent spam
- Only active when menu is closed

## Troubleshooting

### Script Not Loading
**Problem**: Script doesn't load in GTA V
**Solutions**:
- Verify ScriptHook V is installed correctly
- Check that the ASI file is in the GTA V root directory (same folder as GTA5.exe)
- Ensure the game is running in DirectX 11 mode
- Check `ScriptHookV.log` in GTA V directory for errors

### MSAgent Not Responding
**Problem**: No reactions from MSAgent character
**Solutions**:
- Ensure MSAgent-AI is running before launching GTA V
- Check MSAgent-AI log file (`MSAgentAI.log`)
- Verify the named pipe server is started in MSAgent-AI
- Try sending a test command: `PING` should return `PONG`

### Menu Not Appearing
**Problem**: [ key doesn't open the menu
**Solutions**:
- Check if another script is using the [ key
- Verify the script is loaded (check ScriptHookV.log)
- Try a different key binding

### Build Errors
**Problem**: Compilation errors
**Solutions**:
- Verify ScriptHook V SDK files are in the `inc` folder
- Check that you're building for x86 (not x64)
- Ensure Windows SDK is installed
- Update Visual Studio to latest version

## Features in Detail

### Vehicle Reactions
When you enter a vehicle, the script:
1. Detects the vehicle model and class
2. Estimates the vehicle value
3. Sends context to MSAgent: "I just got into a [vehicle] ([class]). It's worth about $[value]. React to this!"
4. MSAgent responds with AI-generated commentary

Example responses:
- "Wow, that's a fancy sports car! Drive safely!"
- "A motorcycle? That's dangerous, be careful out there!"
- "Nice helicopter! The view from up there must be amazing!"

### Environment Reactions
The script tracks:
- **Weather**: Detects transitions between sunny, rainy, foggy, etc.
- **Time**: Announces each hour with context (morning/afternoon/evening/night)
- **Location**: Identifies 80+ zones in Los Santos and Blaine County

### Mission Reactions
- Mission start: "A mission just started! Get excited!"
- Mission end: "The mission ended. Comment on how it went!"

### Character Events
- Low health warnings: "The player's health is really low! Say something concerned!"
- Death reactions: "The player just died! React to it!"
- Character switching (Michael/Franklin/Trevor)

### Wanted Level System
- Level increases: "The player's wanted level just increased to [N] stars! React to the police chase!"
- Level cleared: "The wanted level is gone! The player escaped the cops!"

## Advanced Customization

### Adding Custom Events
To add your own event detection:

1. Create a new function in `script.cpp`:
```cpp
void CheckCustomEvent() {
    if (!g_Settings.customReaction) return;
    
    // Your detection logic here
    if (/* condition */) {
        SendChatCommand("Your prompt here");
    }
}
```

2. Add to the settings struct:
```cpp
struct Settings {
    // ... existing settings ...
    bool customReaction = true;
};
```

3. Add menu item for it in `DrawMenu()`

4. Call it in `ScriptMain()`:
```cpp
CheckCustomEvent();
```

### Integration with Other Mods
This script can coexist with other ScriptHook V mods. The menu system is non-intrusive and uses minimal screen space.

## API Reference

### MSAgent-AI Commands Used

| Command | Usage | Description |
|---------|-------|-------------|
| `SPEAK:text` | Quick announcements | Direct text-to-speech |
| `CHAT:prompt` | AI commentary | Sends prompt to Ollama for AI response |
| `PING` | Connection test | Verifies MSAgent-AI is running |

### Game Natives Used

The script uses these GTA V native functions:
- `PLAYER::PLAYER_ID()` - Get player
- `PED::IS_PED_IN_ANY_VEHICLE()` - Vehicle detection
- `ENTITY::GET_ENTITY_MODEL()` - Get vehicle/entity model
- `VEHICLE::GET_VEHICLE_CLASS()` - Get vehicle type
- `ZONE::GET_NAME_OF_ZONE()` - Location detection
- `GAMEPLAY::GET_MISSION_FLAG()` - Mission status
- Plus many more for comprehensive game state monitoring

## Known Limitations

1. **Vehicle names**: Uses internal game keys (e.g., "ADDER") instead of display names (e.g., "Truffade Adder"). For production use, implement `UI::_GET_LABEL_TEXT()` conversion.
2. **Zone names**: Currently uses a hardcoded mapping of zone codes to friendly names. The mapping covers 80+ zones but may not be complete for all DLC areas.
3. **Character detection**: Character switching detection is simplified and may not work perfectly in all scenarios. For accurate detection, use `PLAYER::GET_PLAYER_CHARACTER()` or track Ped model hashes.
4. **Weather detection**: Simplified weather tracking that may not handle all weather types correctly. Proper implementation should handle hash-to-index conversion.
5. **Mission details**: The script can detect mission start/end but not specific mission objectives or names.
6. **AI latency**: Responses may be delayed depending on Ollama processing time.
7. **Online mode**: Script only works in single-player mode (ScriptHook V requirement).

**Note:** This is a demonstration implementation focused on showcasing the integration pattern. For production use, review the inline comments marked with "NOTE:" for suggested improvements.

## Contributing

Improvements and additions are welcome! Some ideas:
- More detailed vehicle database with exact prices
- Mission name detection
- Reaction to specific story events
- Support for custom character voices
- Integration with other GTA V mods

## Credits

- **ScriptHook V** by Alexander Blade - http://www.dev-c.com/gtav/scripthookv/
- **MSAgent-AI** - Main application framework
- **Rockstar Games** - Grand Theft Auto V

## License

This integration script is provided under the MIT License, same as the main MSAgent-AI project.

## Support

For issues or questions:
1. Check the Troubleshooting section above
2. Review MSAgent-AI logs
3. Check ScriptHookV.log in GTA V directory
4. Open an issue on the GitHub repository

---

**Have fun with your AI-powered GTA V companion!** 🎮🤖
