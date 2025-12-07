# Quick Start Guide for BeamNG AI Commentary Mod

This guide will get you up and running in 5 minutes!

## Prerequisites Check

- [ ] Windows 10/11
- [ ] MSAgent-AI installed and running
- [ ] BeamNG.drive installed (version 0.30+)
- [ ] Python 3.8+ installed

## Installation (3 steps)

### 1. Set up the Bridge Server (1 minute)

Open Command Prompt in the `beamng-bridge` folder and run:

```cmd
setup.bat
```

This will install the required Python packages.

### 2. Install the BeamNG Mod (2 minutes)

1. Press `Win + R`, type `%LOCALAPPDATA%`, press Enter
2. Navigate to: `BeamNG.drive\[version]\mods`
3. Create a new folder called `msagent_ai`
4. Copy everything from `beamng-mod\` into `mods\msagent_ai\`

Your folder should look like:
```
mods\msagent_ai\
├── info.json
├── README.md
└── lua\
    └── ge\
        └── extensions\
            └── msagent_ai.lua
```

### 3. Start Everything (30 seconds)

1. **Launch MSAgent-AI** (the desktop application)
2. **Start the bridge server**: Double-click `beamng-bridge\start.bat`
3. **Launch BeamNG.drive**
4. **Spawn a vehicle and drive!**

## What Should Happen

✓ MSAgent-AI character appears on your desktop  
✓ Bridge server shows "Starting BeamNG to MSAgent-AI Bridge on port 5000"  
✓ When you spawn a vehicle in BeamNG, your agent comments on it  
✓ Crashes, dents, and scratches trigger AI commentary  

## Troubleshooting

### "Could not connect to MSAgent-AI"

- Make sure MSAgent-AI is running (check system tray)
- Restart MSAgent-AI if needed

### "No commentary in BeamNG"

1. Press `~` in BeamNG to open console
2. Type: `dump(extensions.msagent_ai)`
3. If you see `nil`, the mod isn't loaded - check installation folder

### "Port 5000 already in use"

Edit `bridge.py` and change:
```python
port = int(os.getenv('PORT', 5001))  # Changed from 5000 to 5001
```

Then edit `beamng-mod\lua\ge\extensions\msagent_ai.lua`:
```lua
local serverUrl = "http://localhost:5001"  -- Changed from 5000 to 5001
```

## Tips

- **Adjust commentary frequency**: Edit `commentaryCooldown` in `msagent_ai.lua`
- **Change agent personality**: Edit System Prompt in MSAgent-AI settings
- **View logs**: Check `MSAgentAI.log` in the MSAgent-AI folder

## Need Help?

See the full documentation:
- [BeamNG Mod README](../beamng-mod/README.md)
- [MSAgent-AI PIPELINE.md](../PIPELINE.md)
- [Main README](../README.md)

## Next Steps

Once everything works:
- Try different vehicles to hear varied commentary
- Crash spectacularly for dramatic reactions
- Drive in different maps for location-based comments
- Customize the AI personality in MSAgent-AI settings
