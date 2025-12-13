# Quick Start Guide - GTA V MSAgent Integration (ScriptHookDotNet)

## Installation (5 minutes)

⚠️ **Much simpler than ScriptHook V!** No SDK needed, just copy DLL files.

### Step 1: Install ScriptHookDotNet

1. **Download ScriptHookDotNet v3**:
   - Get it from: https://github.com/scripthookvdotnet/scripthookvdotnet/releases
   - Download the latest release ZIP

2. **Extract to GTA V directory:**
   - Copy `ScriptHookVDotNet3.dll` to your GTA V root folder
   - Copy `ScriptHookV.dll` to your GTA V root folder (included with SHVDN)

### Step 2: Install Prerequisites
   
1. **Setup MSAgent-AI**:
   - Make sure MSAgent-AI is installed and working
   - Configure your character and Ollama AI settings
   - Test that it works by using the Speak menu

### Step 3: Install the Script

**Option A: Download Pre-built DLL from GitHub Actions (Recommended)**
1. Go to the [Actions tab](../../actions/workflows/build-gtav-dotnet.yml)
2. Click on the latest successful workflow run
3. Scroll down to "Artifacts" section
4. Download `MSAgentGTA-ScriptHookDotNet-*` artifact (it's a ZIP file)
5. Extract the ZIP file
6. Create a `scripts` folder in your GTA V directory if it doesn't exist
7. Copy `MSAgentGTA.dll` from the extracted files to the `scripts` folder
8. That's it!

**Option B: Build from Source (Optional)**
1. Open `MSAgentGTA.csproj` in Visual Studio
2. Add reference to `ScriptHookVDotNet3.dll` from your GTA V directory
3. Build (Ctrl+Shift+B)
4. Copy `bin/Release/MSAgentGTA.dll` to `GTA V/scripts/` folder

### Step 4: Launch
1. **Start MSAgent-AI first** (important!)
2. Launch GTA V
3. Once in-game, press **[** (left bracket) to open the reactions menu
4. Configure which reactions you want enabled
5. Play the game and enjoy your AI companion!

## Why ScriptHookDotNet is Better

✅ **No SDK required** - Just reference a single DLL  
✅ **C# instead of C++** - Easier to read and modify  
✅ **Standard .NET project** - Familiar build process  
✅ **Better APIs** - Modern, well-documented  
✅ **Faster development** - No manual header/lib setup

### Step 4: Install & Test
1. Copy `Release/MSAgentGTA.asi` to your GTA V directory
2. Follow "For Users" Step 3 above

## Keybindings

| Key | Action |
|-----|--------|
| [ | Open/Close Menu |
| Arrow Up/Down | Navigate Menu |
| Enter | Toggle Setting |

## Features Overview

### What Gets Detected?
- ✅ Entering/exiting vehicles
- ✅ Vehicle type and estimated value
- ✅ Weather changes
- ✅ Time of day (hourly)
- ✅ Location/zone changes
- ✅ Mission start/end
- ✅ Wanted level changes
- ✅ Health status
- ✅ Character switches

### Reaction Categories
1. **Vehicle Reactions**: Comments on cars, bikes, boats, planes, helicopters
2. **Mission Reactions**: Announces mission events
3. **Environment Reactions**: Weather and time commentary
4. **Character Reactions**: Health and character switching
5. **General Reactions**: Wanted level and misc events
6. **Live Commentary**: Random observations every 5 minutes

## Troubleshooting

### "Script not loading"
- Check that ScriptHook V is installed correctly
- Verify the .asi file is in the GTA V root directory
- Look at `ScriptHookV.log` in GTA V directory for errors

### "No reactions from MSAgent"
- Ensure MSAgent-AI is running BEFORE launching GTA V
- Check the MSAgent-AI log file
- Test the connection: the script announces "GTA 5 MSAgent integration is now active!" when loaded

### "Menu doesn't appear"
- Make sure you're pressing [ in-game
- Check if another mod is using the same key
- Verify script is loaded (check ScriptHookV.log)

### "Build errors"
- Verify all SDK files are copied to the right locations
- Make sure you're building for x86, not x64
- Check that ScriptHookV.lib is in the lib folder

## Next Steps

After successful installation:
1. Experiment with different reaction toggles
2. Try different in-game scenarios
3. Adjust your MSAgent-AI personality for funny responses
4. Share your favorite reactions!

## Support

Need help? Check:
1. Main README.md for detailed documentation
2. MSAgentAI.log for application errors
3. ScriptHookV.log for script loading errors
4. GitHub Issues for known problems

---

Enjoy your AI-powered GTA V experience! 🎮🤖
