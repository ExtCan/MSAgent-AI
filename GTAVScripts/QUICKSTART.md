# Quick Start Guide - MSAgent-AI GTA V Integration

Get MSAgent commentating on your GTA V gameplay in 5 minutes!

## Prerequisites Checklist

Before you begin, download and install these in order:

- [ ] **GTA V** - Obviously you need the game
- [ ] **MSAgent-AI** - The main application from this repository
- [ ] **DoubleAgent** - https://doubleagent.sourceforge.net/
- [ ] **ScriptHookV** - http://www.dev-c.com/gtav/scripthookv/
- [ ] **ScriptHookVDotNet** - https://github.com/scripthookvdotnet/scripthookvdotnet/releases
- [ ] **(Optional) Ollama** - https://ollama.ai for AI-powered responses

## Step-by-Step Installation

### 1. Install MSAgent-AI (5 minutes)

```bash
# Build the main application
cd src
dotnet restore
dotnet build --configuration Release
```

Or download a pre-built release from GitHub.

### 2. Install Game Mods (5 minutes)

1. **Install ScriptHookV:**
   - Download from http://www.dev-c.com/gtav/scripthookv/
   - Extract `ScriptHookV.dll` and `dinput8.dll`
   - Copy both to your GTA V folder (where `GTA5.exe` is)

2. **Install ScriptHookVDotNet:**
   - Download from https://github.com/scripthookvdotnet/scripthookvdotnet/releases
   - Extract ALL files to your GTA V folder
   - This creates a `scripts` folder automatically

### 3. Install the MSAgent Script (2 minutes)

**Option A: Use Pre-built DLL (Easier)**

1. Download `MSAgentGTAV.dll` from the releases
2. Copy it to `[GTA V]\scripts\MSAgentGTAV.dll`
3. Done!

**Option B: Build from Source**

1. Set your GTA V directory:
   ```cmd
   setx GTAV_DIR "C:\Program Files\Rockstar Games\Grand Theft Auto V"
   ```

2. Close and reopen your command prompt, then:
   ```cmd
   cd GTAVScripts
   build.bat
   ```

### 4. First Launch (1 minute)

1. **Start MSAgent-AI first:**
   - Run `MSAgentAI.exe`
   - Wait for the agent to appear
   - Configure your character if needed

2. **Launch GTA V:**
   - Start the game normally
   - Load into story mode
   - Wait for "GTA V integration loaded!" notification

3. **Test it:**
   - Press **F9** to open the menu
   - Get in a car - MSAgent should react!
   - Try changing weather with a trainer
   - Switch characters (Story Mode)

## Quick Test

To verify everything works:

1. **Start MSAgent-AI** - You should see the agent on your desktop
2. **Launch GTA V** - Load into story mode
3. **Look for the notification** - "GTA V integration loaded!"
4. **Press F9** - The menu should appear
5. **Get in any vehicle** - Within 3 seconds, MSAgent should comment

If MSAgent doesn't react, see the Troubleshooting section in README.md.

## Configuration

### MSAgent-AI Settings

Open MSAgent-AI settings and configure:

- **Character**: Choose your favorite MS Agent character
- **Voice**: Select a SAPI4 voice
- **Ollama**: (Optional) Enable for AI responses
  - Install Ollama: https://ollama.ai
  - Run: `ollama pull llama3.2`
  - Set URL to `http://localhost:11434`
  - Enable "Enable Chat"

### Script Settings (In-Game)

Press **F9** in GTA V to access the menu:

- ✅ React to Vehicles - Comments on cars, bikes, boats, planes
- ✅ React to Missions - Comments on mission events
- ✅ React to Weather - Comments on weather changes
- ✅ React to Time - Comments on sunrise, sunset, etc.
- ✅ React to Location - Comments when entering areas
- ✅ React to Player State - Comments on health, wanted level
- ✅ React to Character Switch - Comments when switching
- ✅ React to Vehicle Value - Mentions vehicle prices

Toggle any off if they're too chatty!

## Common Issues

### "Script not loading"
- Check that ScriptHookV version matches your game version
- Update ScriptHookV after GTA V updates
- Check `ScriptHookV.log` in GTA V folder

### "MSAgent not responding"
- Make sure MSAgent-AI is running BEFORE you start GTA V
- Check system tray for MSAgent icon
- Check `MSAgentAI.log` for errors

### "Too many reactions"
- Press F9 and disable some reaction types
- Or edit cooldown times in the source code

### "Game crashes on startup"
- Remove ScriptHookV temporarily to test
- Update all mods to latest versions
- Check game file integrity in Social Club/Steam

## Tips for Best Experience

1. **Enable Ollama AI** - Much better responses than static text
2. **Start MSAgent first** - Always launch it before GTA V
3. **Adjust reaction toggles** - Find what works for you
4. **Use a personality prompt** - Make MSAgent funny/serious/etc.
5. **Watch the logs** - `MSAgentAI.log` shows all communication

## Example Reactions

With Ollama AI enabled, you might hear:

**Getting in an expensive car:**
> "Whoa! A Zentorno worth $725,000? Someone's living large! Try not to scratch it!"

**3-star wanted level:**
> "Uh oh! Three stars! The cops are NOT happy with you! Floor it!"

**Switching to Trevor:**
> "Oh great, Trevor's here. Time for some chaos, I suppose!"

**Sunset in Los Santos:**
> "Beautiful sunset over Los Santos. Almost makes you forget about all the crime!"

## What's Next?

- Explore all the reaction types
- Customize the prompts in the source code
- Add new vehicle values
- Share your favorite reactions!

## Need Help?

- Check the full [README.md](README.md)
- Read the [PIPELINE.md](../PIPELINE.md) documentation
- Review MSAgent logs
- Open an issue on GitHub

Have fun!
