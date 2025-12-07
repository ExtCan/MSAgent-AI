# MSAgent-AI BeamNG Integration Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          User's Windows PC                               │
│                                                                          │
│  ┌──────────────────┐                                                   │
│  │  BeamNG.drive    │                                                   │
│  │  ─────────────   │                                                   │
│  │  Game monitors:  │                                                   │
│  │  • Vehicle info  │                                                   │
│  │  • Crashes       │                                                   │
│  │  • Damage        │         HTTP POST                                 │
│  │  • Environment   │────────────────────┐                             │
│  │                  │    (localhost:5000) │                             │
│  └──────────────────┘                     │                             │
│         ▲                                  ▼                             │
│         │                        ┌──────────────────┐                   │
│         │                        │  Bridge Server   │                   │
│         │                        │  ──────────────  │                   │
│         │                        │  Python/Flask    │                   │
│         │                        │  • HTTP Server   │                   │
│         └────────────────────────│  • Translates    │                   │
│           In-game messages       │    to Named Pipe │                   │
│                                  └──────────────────┘                   │
│                                           │                              │
│                                           │ Named Pipe                   │
│                                           │ (\\.\pipe\MSAgentAI)         │
│                                           ▼                              │
│                                  ┌──────────────────┐                   │
│                                  │   MSAgent-AI     │                   │
│                                  │   ────────────   │                   │
│                                  │   Desktop App    │                   │
│                                  │   • Named Pipe   │                   │
│                                  │     Server       │                   │
│                                  │   • MS Agent     │                   │
│                                  │     Character    │                   │
│                                  │   • SAPI4 Voice  │                   │
│                                  │   • Ollama AI    │                   │
│                                  └──────────────────┘                   │
│                                           │                              │
│                                           ▼                              │
│                                  ┌──────────────────┐                   │
│                                  │  Ollama (opt.)   │                   │
│                                  │  ──────────────  │                   │
│                                  │  LLM for AI      │                   │
│                                  │  commentary      │                   │
│                                  └──────────────────┘                   │
└─────────────────────────────────────────────────────────────────────────┘
```

## Data Flow Example: Crash Event

```
1. BeamNG.drive (Lua)
   └─> Detects sudden deceleration
       └─> msagent_ai.lua: checkForCrash() returns true

2. HTTP Request
   └─> POST http://localhost:5000/crash
       Body: {
         "vehicle_name": "D-Series",
         "speed_before": 85.5,
         "damage_level": 0.65
       }

3. Bridge Server (Python)
   └─> Receives HTTP request
       └─> Constructs AI prompt:
           "I just crashed my D-Series at 85 km/h! 
            The damage is pretty bad (0.7). React dramatically!"
       └─> Sends via Named Pipe:
           "CHAT:I just crashed my D-Series..."

4. MSAgent-AI (C#)
   └─> PipelineServer receives CHAT command
       └─> Sends prompt to Ollama
       └─> Receives AI response:
           "Woah! That was a nasty hit! Hope you're okay!"
       └─> Character speaks with SAPI4
       └─> Character plays "Surprised" animation

5. User Experience
   └─> Desktop character says the commentary aloud
       └─> BeamNG shows message in-game (top-right)
```

## Component Responsibilities

### BeamNG Mod (Lua)
- **Location**: `beamng-mod/lua/ge/extensions/msagent_ai.lua`
- **Purpose**: Game event detection
- **Responsibilities**:
  - Monitor vehicle state
  - Detect crashes (sudden deceleration)
  - Track damage accumulation
  - Collect environment data
  - Send HTTP requests to bridge
- **Update Frequency**: 2 seconds (configurable)

### Bridge Server (Python)
- **Location**: `beamng-bridge/bridge.py`
- **Purpose**: Protocol translation
- **Responsibilities**:
  - HTTP server for BeamNG requests
  - Named Pipe client for MSAgent-AI
  - Convert game events to AI prompts
  - Format commands for pipeline
- **Port**: 5000 (configurable)

### MSAgent-AI (C#)
- **Location**: `src/`
- **Purpose**: Desktop friend application
- **Responsibilities**:
  - Named Pipe server
  - MS Agent character display
  - SAPI4 text-to-speech
  - Ollama AI integration
  - Command processing
- **Pipe**: `\\.\pipe\MSAgentAI`

## Configuration Points

### 1. BeamNG Mod
```lua
-- In msagent_ai.lua
local serverUrl = "http://localhost:5000"     -- Bridge server URL
local updateInterval = 2.0                    -- Seconds between checks
local commentaryCooldown = 5.0                -- Minimum time between comments
local damageThreshold = 0.01                  -- Minimum damage to detect
```

### 2. Bridge Server
```python
# In bridge.py
PIPE_NAME = r'\\.\pipe\MSAgentAI'             # Named pipe path
port = int(os.getenv('PORT', 5000))           # HTTP server port
```

### 3. MSAgent-AI
```csharp
// In PipelineServer.cs
public const string PipeName = "MSAgentAI";   // Named pipe name

// In UI Settings
- Ollama URL: http://localhost:11434
- Model: llama3.2
- System Prompt: Define character personality
```

## Network Ports

- **5000**: Bridge server HTTP (default, configurable)
- **11434**: Ollama API (if using AI features)
- **Named Pipe**: Local IPC, no network port

## Security Considerations

- All communication is **local only** (localhost/named pipes)
- No external network access required
- No data leaves the user's PC
- Named pipe is user-accessible only
- HTTP server binds to localhost by default

## Extension Points

### Adding New Event Types

1. **BeamNG Mod**: Add event detection logic
   ```lua
   if checkForJump() then
     sendToAI("/jump", {height = jumpHeight})
   end
   ```

2. **Bridge Server**: Add endpoint
   ```python
   @app.route('/jump', methods=['POST'])
   def comment_on_jump():
       data = request.json
       prompt = f"I just jumped {data['height']:.1f} meters!"
       send_to_msagent(f"CHAT:{prompt}")
       return jsonify({'status': 'ok'})
   ```

3. **No change needed to MSAgent-AI** - it handles all CHAT commands

## Performance Metrics

- **Latency**: 100-500ms (event to speech)
  - Game detection: <10ms
  - HTTP request: 10-50ms
  - Named pipe: 1-5ms
  - AI response: 100-3000ms (depends on Ollama)
  - TTS: 50-200ms

- **Resource Usage**:
  - BeamNG mod: Negligible (<1% CPU)
  - Bridge server: ~20MB RAM, <1% CPU
  - MSAgent-AI: ~50MB RAM, 1-5% CPU
  - Ollama: Varies by model (1-4GB VRAM)

## Troubleshooting Flow

```
No commentary?
│
├─> Bridge server not running?
│   └─> Run start.bat
│
├─> MSAgent-AI not running?
│   └─> Launch MSAgent-AI.exe
│
├─> Mod not loaded in BeamNG?
│   ├─> Check: dump(extensions.msagent_ai) in console
│   └─> Verify folder structure
│
└─> Events not triggering?
    ├─> Check cooldown timer (default 5s)
    └─> Check damage threshold (default 0.01)
```

## Development Tips

- **Debug BeamNG mod**: Check in-game console (`~` key)
- **Debug bridge**: Watch terminal output
- **Debug MSAgent-AI**: Check `MSAgentAI.log`
- **Test pipeline**: Use `PIPELINE.md` examples
- **Mock testing**: Use `test_bridge.py` on non-Windows
