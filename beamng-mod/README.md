# MSAgent-AI BeamNG.drive Mod

Turn your desktop friend into a driving companion! This BeamNG.drive mod connects to MSAgent-AI to provide real-time, AI-powered commentary on your driving experience.

## Features

- 🚗 **Vehicle Commentary**: AI comments when you spawn a new vehicle
- 💥 **Crash Detection**: Reacts to crashes with witty commentary
- 🔧 **Damage Tracking**: Comments on dents and paint scratches
- 🌍 **Environment Awareness**: Observations about your location and driving conditions
- 🤖 **AI-Powered**: Uses MSAgent-AI's Ollama integration for dynamic, personality-driven responses

## How It Works

```
┌─────────────────┐         HTTP          ┌──────────────────┐      Named Pipe      ┌──────────────┐
│  BeamNG.drive   │ ─────────────────────▶ │  Bridge Server   │ ───────────────────▶ │  MSAgent-AI  │
│   (Lua Mod)     │ ◀───────────────────── │   (Python)       │ ◀─────────────────── │   (Desktop)  │
└─────────────────┘      JSON Response     └──────────────────┘      Pipe Commands    └──────────────┘
```

The mod monitors game events in BeamNG.drive and sends them to a bridge server, which forwards them to MSAgent-AI via Named Pipe. Your desktop friend then speaks AI-generated commentary!

## Installation

### Prerequisites

1. **MSAgent-AI**: Install and run the main application (see [main README](../README.md))
2. **BeamNG.drive**: Version 0.30 or higher
3. **Python 3.8+**: For the bridge server
4. **Ollama**: (Optional) For AI-generated commentary. Without it, MSAgent-AI will use predefined responses.

### Step 1: Install the Bridge Server

The bridge server translates HTTP requests from BeamNG into Named Pipe commands for MSAgent-AI.

```bash
cd beamng-bridge
pip install -r requirements.txt
```

### Step 2: Install the BeamNG Mod

1. Locate your BeamNG.drive mods folder:
   - Windows: `C:\Users\[YourUsername]\AppData\Local\BeamNG.drive\[version]\mods`
   - Create the `mods` folder if it doesn't exist

2. Copy the mod files:
   ```
   Copy the beamng-mod folder contents to:
   mods/msagent_ai/
   ├── info.json
   └── lua/
       └── ge/
           └── extensions/
               └── msagent_ai.lua
   ```

## Usage

### Starting the System

1. **Launch MSAgent-AI** (the desktop application)
2. **Start the bridge server**:
   ```bash
   cd beamng-bridge
   python bridge.py
   ```
   You should see:
   ```
   Starting BeamNG to MSAgent-AI Bridge on port 5000
   Connecting to Named Pipe: \\.\pipe\MSAgentAI
   Make sure MSAgent-AI is running!
   ```

3. **Launch BeamNG.drive**
4. **Load any map and spawn a vehicle**

### What to Expect

Once you start driving, your desktop friend will:

- **Welcome your vehicle**: When you spawn a car, the AI will comment on it
- **React to crashes**: Hit a wall? The AI will have something to say!
- **Notice damage**: Small scratches and big dents both get commentary
- **Observe surroundings**: Periodic comments about where you're driving

All commentary appears as:
- In-game messages in BeamNG (top-right corner)
- Spoken by your MSAgent character on your desktop

## Configuration

### Bridge Server

Edit `beamng-bridge/bridge.py` if needed:

```python
PIPE_NAME = r'\\.\pipe\MSAgentAI'  # Named pipe to MSAgent-AI
PIPE_TIMEOUT = 5000  # Connection timeout in milliseconds
```

### BeamNG Mod

Edit `beamng-mod/lua/ge/extensions/msagent_ai.lua` to customize:

```lua
-- Server URL (where the bridge server is running)
local serverUrl = "http://localhost:5000"

-- How often to check surroundings (seconds)
local updateInterval = 2.0

-- Minimum damage to trigger commentary
local damageThreshold = 0.01

-- Minimum time between any comments (seconds)
local commentaryCooldown = 5.0
```

### MSAgent-AI Personality

To customize how your agent responds:

1. Open MSAgent-AI settings
2. Navigate to **AI Settings**
3. Adjust the **System Prompt** to define the character's personality
4. Example: "You are an enthusiastic car enthusiast who loves commenting on driving. Be witty and energetic!"

## Troubleshooting

### "No commentary appearing"

**Check the bridge server:**
```bash
# Test if bridge server is running
curl http://localhost:5000/health
```

Expected response:
```json
{
  "status": "ok",
  "msagent_connected": true,
  "msagent_response": "PONG"
}
```

**If `msagent_connected` is `false`:**
- Ensure MSAgent-AI is running
- Check that the Named Pipe server is enabled in MSAgent-AI settings

### "Bridge server won't start"

**Error: `No module named 'win32pipe'`**
```bash
pip install pywin32
```

**Error: `Port 5000 already in use`**
```bash
# Use a different port
set PORT=8080
python bridge.py

# Then update the BeamNG mod's serverUrl to match
```

### "Mod not loading in BeamNG"

1. Press `~` to open the BeamNG console
2. Type: `dump(extensions.msagent_ai)`
3. If you see `nil`, check:
   - Folder structure is correct
   - `info.json` is in the mod root
   - BeamNG version is 0.30+

**Check BeamNG logs:**
- Location: BeamNG.drive installation folder
- File: `BeamNG.log`
- Look for: "msagent_ai" or error messages

### "Commentary is AI-generated but sounds generic"

This means MSAgent-AI isn't using Ollama:

1. Install Ollama: https://ollama.ai
2. Pull a model: `ollama pull llama3.2`
3. In MSAgent-AI settings:
   - Enable **Use Ollama for Chat**
   - Set Ollama URL: `http://localhost:11434`
   - Set Model: `llama3.2`

## Advanced Usage

### Custom Events

You can add your own commentary triggers by editing `msagent_ai.lua`:

```lua
-- Example: Comment when reaching high speed
if env.speed > 200 then
  sendToAI("/custom_event", {
    event = "high_speed",
    speed = env.speed,
    vehicle = vehicleInfo.name
  })
end
```

Then add a handler in `bridge.py`:

```python
@app.route('/custom_event', methods=['POST'])
def custom_event():
    data = request.json
    event = data.get('event')
    speed = data.get('speed')
    
    if event == 'high_speed':
        prompt = f"I just hit {speed:.0f} km/h! This is incredibly fast!"
        send_to_msagent(f"CHAT:{prompt}")
    
    return jsonify({'status': 'ok'})
```

### Multiple Monitors

If MSAgent-AI is on a different monitor, it will still work! The character will speak from wherever it's positioned.

### Streaming Integration

The bridge server could be extended to:
- Log events for stream overlays
- Trigger OBS scenes on crashes
- Send events to chat bots

## Examples

### Typical Session

```
[You spawn an ETK 800-Series]
Agent: "Nice! An ETK 800-Series! That's a beautiful machine. Let's see what it can do!"

[You accelerate to 120 km/h]
Agent: "Looking good out here on the highway! The weather's perfect for a drive."

[You crash into a wall at 80 km/h]
Agent: "Ouch! That was a hard hit! The front end is definitely feeling that one!"

[You scratch the paint on a barrier]
Agent: "Eh, just a little scratch. Adds character to the car!"
```

## Performance

- **CPU Impact**: Minimal - events are sent asynchronously
- **Network**: Local HTTP only (localhost:5000)
- **Memory**: <5MB for bridge server
- **Latency**: Commentary appears 1-3 seconds after events (depending on AI response time)

## Privacy

- All communication is local (localhost)
- No data is sent to external servers (except Ollama API if configured)
- BeamNG events are processed in real-time and not stored

## Contributing

Want to improve the mod? Ideas:

- Add more event types (jumps, flips, near-misses)
- Support for multiplayer events
- Integration with BeamNG.drive's damage model
- Custom animations based on events

Submit pull requests to the main repository!

## Credits

- Built for **MSAgent-AI** by ExtCan
- Compatible with **BeamNG.drive** (BeamNG GmbH)
- Uses **Ollama** for AI commentary (optional)

## License

MIT License - Same as main MSAgent-AI project
