# MSAgent-AI GTA V Integration Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         GTA V (Game Process)                         │
│                                                                      │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │              ScriptHook V (Alexander Blade)                    │ │
│  │                                                                │ │
│  │  ┌──────────────────────────────────────────────────────────┐ │ │
│  │  │         MSAgentGTA.asi (This Integration)                │ │ │
│  │  │                                                          │ │ │
│  │  │  ┌───────────────────────────────────────────────────┐  │ │ │
│  │  │  │         Game State Monitor                        │  │ │ │
│  │  │  │  • Vehicle detection                              │  │ │ │
│  │  │  │  • Mission tracking                               │  │ │ │
│  │  │  │  • Environment monitoring (weather, time, zone)   │  │ │ │
│  │  │  │  • Character health & status                      │  │ │ │
│  │  │  │  • Wanted level tracking                          │  │ │ │
│  │  │  └───────────────────────────────────────────────────┘  │ │ │
│  │  │                           │                              │ │ │
│  │  │                           ▼                              │ │ │
│  │  │  ┌───────────────────────────────────────────────────┐  │ │ │
│  │  │  │         Event Processing                          │  │ │ │
│  │  │  │  • Detect state changes                           │  │ │ │
│  │  │  │  • Check toggle settings                          │  │ │ │
│  │  │  │  • Build contextual prompts                       │  │ │ │
│  │  │  │  • Throttle events                                │  │ │ │
│  │  │  └───────────────────────────────────────────────────┘  │ │ │
│  │  │                           │                              │ │ │
│  │  │                           ▼                              │ │ │
│  │  │  ┌───────────────────────────────────────────────────┐  │ │ │
│  │  │  │         Named Pipe Client                         │  │ │ │
│  │  │  │  • Connect to \\.\pipe\MSAgentAI                  │  │ │ │
│  │  │  │  • Send SPEAK/CHAT commands                       │  │ │ │
│  │  │  │  • Handle connection errors                       │  │ │ │
│  │  │  └───────────────────────────────────────────────────┘  │ │ │
│  │  │                                                          │ │ │
│  │  │  ┌───────────────────────────────────────────────────┐  │ │ │
│  │  │  │         In-Game Menu (F9)                         │  │ │ │
│  │  │  │  • Vehicle Reactions      [ON/OFF]                │  │ │ │
│  │  │  │  • Mission Reactions      [ON/OFF]                │  │ │ │
│  │  │  │  • Environment Reactions  [ON/OFF]                │  │ │ │
│  │  │  │  • Character Reactions    [ON/OFF]                │  │ │ │
│  │  │  │  • General Reactions      [ON/OFF]                │  │ │ │
│  │  │  │  • Live Commentary        [ON/OFF]                │  │ │ │
│  │  │  └───────────────────────────────────────────────────┘  │ │ │
│  │  └──────────────────────────────────────────────────────────┘ │ │
│  └────────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────┬───────────────────────────────────┘
                                   │
                                   │ Named Pipe IPC
                                   │ \\.\pipe\MSAgentAI
                                   │
┌──────────────────────────────────▼───────────────────────────────────┐
│                    MSAgent-AI Application                             │
│                                                                       │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │              Named Pipe Server (PipelineServer.cs)              │ │
│  │  • Listens on \\.\pipe\MSAgentAI                                │ │
│  │  • Accepts connections from external apps                       │ │
│  │  • Parses commands (SPEAK, CHAT, ANIMATION, etc.)              │ │
│  └─────────────────────────────────┬───────────────────────────────┘ │
│                                    │                                  │
│                                    ▼                                  │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │              Command Processing                                  │ │
│  │                                                                  │ │
│  │  SPEAK: Direct TTS    ────────►  Sapi4Manager                   │ │
│  │                                         │                        │ │
│  │  CHAT: AI Response    ────────►  OllamaClient                   │ │
│  │                                         │                        │ │
│  │  ANIMATION: Actions   ────────►  AgentManager                   │ │
│  └─────────────────────────────────────────┬───────────────────────┘ │
│                                            │                          │
│                                            ▼                          │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │              Microsoft Agent Character                           │ │
│  │  • Displays on screen                                           │ │
│  │  • Speaks with SAPI4 TTS                                        │ │
│  │  • Performs animations                                          │ │
│  │  • Shows speech bubbles                                         │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                            │                          │
│                                            ▼                          │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │              Ollama AI (Optional)                                │ │
│  │  • Generates contextual responses                               │ │
│  │  • Personality-driven commentary                                │ │
│  │  • Responds to game events with humor/emotion                   │ │
│  └─────────────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────────────┘
```

## Communication Flow Example

### Scenario: Player enters a sports car

```
1. GTA V Game State Changes
   └─► Player enters vehicle (Zentorno)

2. MSAgentGTA.asi detects change
   └─► CheckVehicleChanges() fires
       └─► Identifies: Vehicle Class = Super, Value = $500,000

3. Build contextual prompt
   └─► "I just got into a Zentorno (Super car). 
        It's worth about $500000. React to this!"

4. Send via Named Pipe
   └─► CHAT:I just got into a Zentorno...

5. MSAgent-AI receives command
   └─► PipelineServer parses CHAT command
       └─► Sends to OllamaClient

6. Ollama generates response
   └─► "Wow! That's a super expensive car! 
        Try not to crash it!"

7. MSAgent speaks
   └─► Sapi4Manager converts text to speech
       └─► AgentManager animates character
           └─► Character appears and speaks

8. Response sent back
   └─► OK:CHAT

9. MSAgentGTA.asi continues monitoring
   └─► Waits for next game event
```

## Key Benefits

### For Players
- ✅ Immersive AI companion that reacts to gameplay
- ✅ Customizable reactions via in-game menu
- ✅ No performance impact on game
- ✅ Works with existing MSAgent-AI setup

### For Developers
- ✅ Clean separation of concerns
- ✅ Simple Named Pipe protocol
- ✅ Well-documented code
- ✅ Easy to extend with new events

### For the Community
- ✅ Template for other game integrations
- ✅ Demonstrates MSAgent-AI capabilities
- ✅ Open source for contributions
- ✅ Educational example of IPC

## Technology Stack

| Component | Technology |
|-----------|-----------|
| Game Integration | C++17, ScriptHook V SDK |
| IPC Mechanism | Windows Named Pipes |
| Build System | Visual Studio 2019+, MSBuild |
| Main Application | C# .NET Framework 4.8 |
| AI Backend | Ollama (llama3.2 or similar) |
| TTS Engine | SAPI4 |
| Agent Display | Microsoft Agent / DoubleAgent |

## Event Detection Rate

| Event Type | Check Frequency | Notes |
|------------|----------------|-------|
| Vehicle | Every frame | Only sends on state change |
| Weather | Every frame | Throttled to changes only |
| Time | Every frame | Announces hourly |
| Zone | Every frame | Throttled to zone changes |
| Mission | Every frame | Start/end detection |
| Health | Every frame | Warns at <30% |
| Wanted | Every frame | All level changes |
| Commentary | Every 5 minutes | Optional periodic commentary |

All events are efficiently throttled to prevent spam and ensure only meaningful changes trigger reactions.
