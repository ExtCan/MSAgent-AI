# MSAgent-AI GTA V Integration

This script integrates MSAgent-AI with Grand Theft Auto V to provide live commentary and reactions to in-game events using the Microsoft Agent character.

## Features

### Live Commentary On:
- **Vehicles**: Cars, motorcycles, boats, planes, and helicopters
- **Vehicle Value**: Reactions based on how expensive the vehicle is
- **Missions**: Mission starts and progression
- **Weather**: Weather changes (sunny, rainy, foggy, etc.)
- **Time of Day**: Sunrise, noon, sunset, midnight transitions
- **Locations**: Entering new areas and zones
- **Player State**: Health changes, wanted level, death/respawn
- **Character Switching**: Switching between Michael, Franklin, and Trevor

### Interactive Menu
- Press **F9** to open/close the in-game menu
- Toggle individual reaction types on/off
- Real-time configuration without restarting the game

## Requirements

### 1. MSAgent-AI Application
The main MSAgent-AI desktop application must be running for the script to work.

**Download and Setup:**
1. Build or download MSAgent-AI from the main repository
2. Install DoubleAgent: https://doubleagent.sourceforge.net/
3. Install SAPI 4.0a SDK for voices
4. (Optional) Install Ollama for AI-powered responses: https://ollama.ai
5. Run MSAgentAI.exe before starting GTA V

### 2. ScriptHookV
Required for running scripts in GTA V.

**Download:** http://www.dev-c.com/gtav/scripthookv/

**Installation:**
1. Download ScriptHookV
2. Extract `ScriptHookV.dll` and `dinput8.dll` to your GTA V root folder
3. The root folder contains `GTA5.exe`

### 3. ScriptHookVDotNet
Required for running .NET scripts in GTA V.

**Download:** https://github.com/scripthookvdotnet/scripthookvdotnet/releases

**Installation:**
1. Download the latest version (v3.x recommended)
2. Extract all files to your GTA V root folder
3. This will create a `scripts` folder if it doesn't exist

## Installation

### Download Pre-Built (Recommended)

The easiest way to get started:

1. Go to the [Actions tab](../../actions/workflows/build-gtav-script.yml) on GitHub
2. Click on the latest successful workflow run (green checkmark)
3. Download the **MSAgentGTAV-Release-Package** artifact
4. Extract the ZIP file
5. Copy `MSAgentGTAV.dll` to your GTA V `scripts` folder
   - Usually: `C:\Program Files\Rockstar Games\Grand Theft Auto V\scripts\`
6. See `INSTALL.txt` in the package for detailed instructions

### Quick Install

1. **Ensure MSAgent-AI is running** before launching GTA V
2. Copy `MSAgentGTAV.dll` to your GTA V `scripts` folder
   - Usually: `C:\Program Files\Rockstar Games\Grand Theft Auto V\scripts\`
3. Launch GTA V
4. Once in-game, you should see a notification: "GTA V integration loaded!"

### Building from Source

If you want to build the script yourself:

#### Prerequisites
- Visual Studio 2019 or later
- .NET Framework 4.8 SDK
- ScriptHookVDotNet3.dll (from ScriptHookVDotNet installation)

#### Build Steps

1. **Set GTA V directory environment variable** (optional, for auto-copy):
   ```cmd
   setx GTAV_DIR "C:\Program Files\Rockstar Games\Grand Theft Auto V"
   ```

2. **Open project in Visual Studio:**
   ```cmd
   cd GTAVScripts
   # Open MSAgentGTAV.csproj in Visual Studio
   ```

3. **Add ScriptHookVDotNet reference:**
   - If you didn't set GTAV_DIR, manually add reference to `ScriptHookVDotNet3.dll`
   - Right-click project → Add → Reference → Browse
   - Navigate to your GTA V folder and select `ScriptHookVDotNet3.dll`

4. **Build the project:**
   - Build → Build Solution (or press F6)
   - The DLL will be in `bin\Release\MSAgentGTAV.dll`

5. **Copy to GTA V:**
   - If GTAV_DIR was set, the post-build event copies automatically
   - Otherwise, manually copy `MSAgentGTAV.dll` to `GTA V\scripts\`

#### Alternative: Command Line Build

```cmd
cd GTAVScripts

# Set reference path to your GTA V directory
set GTAV_DIR=C:\Program Files\Rockstar Games\Grand Theft Auto V

# Build using MSBuild
msbuild MSAgentGTAV.csproj /p:Configuration=Release

# Copy to GTA V scripts folder
copy bin\Release\MSAgentGTAV.dll "%GTAV_DIR%\scripts\"
```

## Configuration

The script can be configured using the `MSAgentGTAV.ini` file in your `scripts` folder.

### Configuration File (`MSAgentGTAV.ini`)

Copy the `MSAgentGTAV.ini` file to your GTA V `scripts` folder alongside the DLL.

```ini
[Connection]
; Protocol: NamedPipe or TCP
Protocol=TCP

; TCP settings (used when Protocol=TCP)
IPAddress=127.0.0.1
Port=8765

; Named Pipe settings (used when Protocol=NamedPipe)
PipeName=MSAgentAI

[Cooldowns]
; Cooldown between reactions in milliseconds
SlowCooldown=10000
FastCooldown=3000

[Features]
; Enable/disable reaction categories
ReactToVehicles=true
ReactToMissions=true
ReactToWeather=true
ReactToTime=true
ReactToLocation=true
ReactToPlayerState=true
ReactToCharacterSwitch=true
ReactToVehicleValue=true

[Menu]
; F9 key to toggle menu
MenuKey=F9
```

### Connection Modes

**TCP Mode (Default - Recommended):**
- Works locally and over network
- More flexible than Named Pipes
- Can connect to MSAgent-AI on another computer
- Configure MSAgent-AI to listen on TCP (Settings → Pipeline → Protocol: TCP)

**Named Pipe Mode:**
- Local machine only
- Legacy mode (use if you prefer)
- Configure MSAgent-AI to use Named Pipe mode (Settings → Pipeline → Protocol: NamedPipe)

**Important:** The script's protocol setting must match MSAgent-AI's pipeline protocol!

## Usage

### First Launch

1. **Copy MSAgentGTAV.ini** to your GTA V `scripts` folder (alongside MSAgentGTAV.dll)
2. **Configure connection** in MSAgentGTAV.ini to match your MSAgent-AI setup
3. **Start MSAgent-AI** - The desktop application must be running first
4. **Configure MSAgent-AI Pipeline**:
   - Open Settings → Pipeline tab
   - Set Protocol to match your INI file (TCP or NamedPipe)
   - If using TCP: verify IP address and port match
5. **Launch GTA V**
6. Wait for the game to load
7. You should see: "GTA V integration loaded!" notification
8. MSAgent should say: "GTA V integration loaded! I'm ready to commentate!"

### In-Game Controls

**F9** - Open/Close the MSAgent menu

### Menu Options

The in-game menu allows you to toggle different reaction types:

- **React to Vehicles** - Comments when entering/exiting vehicles
- **React to Missions** - Comments on mission events
- **React to Weather** - Comments on weather changes
- **React to Time** - Comments on time of day transitions
- **React to Location** - Comments when entering new areas
- **React to Player State** - Comments on health, wanted level, death
- **React to Character Switch** - Comments when switching protagonists
- **React to Vehicle Value** - Includes vehicle price in reactions

Each option can be toggled on/off independently without restarting the game.

## How It Works

The script monitors various game events and sends them to MSAgent-AI through either a TCP socket connection or Named Pipe (configured in `MSAgentGTAV.ini`).

**Default Connection (TCP):**
- Protocol: TCP
- Address: 127.0.0.1
- Port: 8765

**Alternative (Named Pipe):**
- Pipe: `\\.\pipe\MSAgentAI`

### Communication Protocol

The script uses these commands:

- `SPEAK:text` - Make MSAgent speak directly
- `CHAT:prompt` - Send a prompt to Ollama AI for intelligent responses
- `ANIMATION:name` - Play a specific animation
- `HIDE` / `SHOW` - Hide or show the agent
- `POKE` - Trigger random AI dialog

### Examples of Reactions

**Vehicle Entry:**
```
CHAT:The player just entered a car called Zentorno worth $725,000. React to this vehicle!
```

**Weather Change:**
```
CHAT:The weather changed to Rainy. Comment on the weather!
```

**Wanted Level:**
```
CHAT:The player now has 3 wanted stars! React to the police chase!
```

**Character Switch:**
```
CHAT:The player switched to Trevor Philips. React to this character!
```

### Cooldown System

To prevent spam, the script implements cooldowns:
- **Standard cooldown**: 10 seconds between most reactions
- **Fast cooldown**: 3 seconds for frequent events (vehicles, wanted level)
- **Location cooldown**: 10 seconds between location changes

## Troubleshooting

### Script Not Loading

1. **Check ScriptHookV is installed:**
   - `ScriptHookV.dll` in GTA V root folder
   - `dinput8.dll` in GTA V root folder

2. **Check ScriptHookVDotNet is installed:**
   - `ScriptHookVDotNet.asi` in GTA V root folder
   - `ScriptHookVDotNet3.dll` in GTA V root folder

3. **Check MSAgentGTAV.dll location:**
   - Must be in `GTA V\scripts\` folder
   - Create the folder if it doesn't exist

4. **Check game version:**
   - ScriptHookV must match your GTA V version
   - Update ScriptHookV after game updates

### MSAgent Not Responding

1. **Verify MSAgent-AI is running:**
   - Check system tray for MSAgent icon
   - Open MSAgent settings to confirm it's active

2. **Check pipe communication:**
   - MSAgent-AI starts a pipe server on launch
   - Look for "Pipeline server started" in MSAgent logs
   - Log location: `MSAgentAI.log` (same folder as executable)

3. **Test the pipe manually:**
   - Use PowerShell to test the connection (see PIPELINE.md)

### No Reactions In Game

1. **Check menu settings:**
   - Press F9 to open menu
   - Ensure reaction types are enabled (checkboxes)

2. **Check cooldowns:**
   - Reactions have 3-10 second cooldowns
   - Wait a bit between events

3. **Check MSAgent chat settings:**
   - If using Ollama, ensure it's running
   - Check Ollama URL in MSAgent settings
   - Verify a model is loaded (e.g., `ollama pull llama3.2`)

### Performance Issues

If the game lags:

1. **Disable unused reactions:**
   - Open menu (F9)
   - Disable reaction types you don't need

2. **Increase cooldown times:**
   - Edit `MSAgentGTAV.cs`
   - Increase `COOLDOWN_MS` and `FAST_COOLDOWN_MS` values
   - Rebuild the script

## Known Limitations

1. **Mission Detection**: Full mission dialog detection is limited by game API access
2. **Online Mode**: Script is designed for single-player mode only
3. **Pipe Communication**: One-way communication (game → MSAgent only)
4. **AI Response Time**: CHAT commands may take a few seconds depending on Ollama model

## Customization

### Adding New Vehicle Values

Edit the `InitializeVehicleValues()` method in `MSAgentGTAV.cs`:

```csharp
vehicleValues[VehicleHash.YourVehicle] = 500000;
```

### Changing Reaction Messages

Modify the `SendChatPrompt()` calls throughout the script to customize what prompts are sent to MSAgent.

### Adding New Events

Add new event checks in the `OnTick()` method:

```csharp
private void CheckYourEvent()
{
    if (!reactToYourEvent) return;
    
    if (/* your condition */)
    {
        if (CanReact(ref lastYourEventTime, COOLDOWN_MS))
        {
            SendChatPrompt("Your custom prompt here!");
        }
    }
}
```

## Version History

### v1.0.0 (Initial Release)
- Full vehicle detection (cars, bikes, boats, planes, helicopters)
- Vehicle value tracking and reactions
- Weather and time of day monitoring
- Location/zone change detection
- Player state tracking (health, wanted level, death)
- Character switching detection
- Mission state monitoring
- In-game menu with F9 hotkey
- Toggleable reaction categories
- Cooldown system to prevent spam
- Named Pipe integration with MSAgent-AI

## Credits

- **MSAgent-AI**: Desktop agent application
- **ScriptHookV**: By Alexander Blade
- **ScriptHookVDotNet**: By crosire and contributors
- **GTA V**: Rockstar Games

## License

MIT License - Same as MSAgent-AI project

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review MSAgent-AI logs: `MSAgentAI.log`
3. Review ScriptHookV logs: `ScriptHookV.log` (in GTA V folder)
4. Open an issue on the MSAgent-AI repository

## Related Documentation

- [MSAgent-AI Main README](../README.md)
- [Pipeline Documentation](../PIPELINE.md)
- [ScriptHookVDotNet Documentation](https://github.com/scripthookvdotnet/scripthookvdotnet)
