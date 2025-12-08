# Quick Start Guide - GTA V MSAgent Integration

## Building the ASI File (Required)

⚠️ **You must build the ASI file yourself** - there is no pre-built version due to ScriptHook V SDK licensing.

### Quick Build Steps (5 minutes)

1. **Download ScriptHook V SDK** from http://www.dev-c.com/gtav/scripthookv/

2. **Copy SDK files to project:**
   ```
   SDK/inc/*               → integrations/GTAV-ScriptHookV/inc/
   SDK/lib/ScriptHookV.lib → integrations/GTAV-ScriptHookV/lib/
   ```

3. **Build in Visual Studio:**
   - Open `integrations/GTAV-ScriptHookV/MSAgentGTA.sln`
   - Configuration: **Release**, Platform: **x86**
   - Press **Ctrl+Shift+B** to build
   - ASI file created at: `Release/MSAgentGTA.asi`

## Installation

### Step 1: Install Prerequisites
1. **Install ScriptHook V**:
   - Download from: http://www.dev-c.com/gtav/scripthookv/
   - Extract `ScriptHookV.dll` and `dinput8.dll` to your GTA V directory
   
2. **Setup MSAgent-AI**:
   - Make sure MSAgent-AI is installed and working
   - Configure your character and Ollama AI settings
   - Test that it works by using the Speak menu

### Step 2: Install the Script
1. Copy the compiled `MSAgentGTA.asi` from `Release/` folder
2. Paste it to your GTA V installation directory (same folder as `GTA5.exe`)
3. That's it!

### Step 3: Launch
1. **Start MSAgent-AI first** (important!)
2. Launch GTA V
3. Once in-game, press **[** (left bracket) to open the reactions menu
4. Configure which reactions you want enabled
5. Play the game and enjoy your AI companion!

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
