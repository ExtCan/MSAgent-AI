# MSAgent-AI

A Windows desktop friend application inspired by BonziBUDDY and CyberBuddy, using Microsoft Agent characters with SAPI4 text-to-speech and Ollama AI integration for dynamic conversations.

## Features

- **MS Agent Character Support**: Load and display Microsoft Agent characters (.acs files) from your system
- **SAPI4 Text-to-Speech**: Full SAPI4 voice support with configurable Speed, Pitch, and Volume
- **Customizable Lines**: Edit welcome, idle, moved, exit, clicked, jokes, and thoughts lines
- **Ollama AI Integration**: Connect to Ollama for dynamic AI-powered conversations with personality prompting
- **Random Dialog**: Configurable random dialog feature (1 in 9000 chance per second by default) that sends custom prompts to Ollama
- **User-Friendly GUI**: System tray application with comprehensive settings panel
- **Named Pipe API**: External application integration via Named Pipe (see [PIPELINE.md](PIPELINE.md))
- **BeamNG.drive Mod**: AI commentary for your driving experience (see [BeamNG Integration](#beamng-integration))

## Requirements

See **[REQUIREMENTS.txt](REQUIREMENTS.txt)** for detailed download links.

- Windows 10/11 with .NET Framework 4.8 or later
- **DoubleAgent** (RECOMMENDED) - Modern MS Agent replacement: https://doubleagent.sourceforge.net/
  - Or original Microsoft Agent with manual COM registration
- SAPI4 Text-to-Speech engine: https://www.microsoft.com/en-us/download/details.aspx?id=10121
- Ollama (optional, for AI chat features): https://ollama.ai

## Installation

1. **Install DoubleAgent** from https://doubleagent.sourceforge.net/ (handles all COM registration automatically)
2. Install SAPI 4.0a SDK for voices
3. Download and install Ollama if you want AI chat features: `ollama pull llama3.2`
4. Download the latest release from GitHub Actions or build with `dotnet build`
5. Run MSAgentAI.exe

### Troubleshooting

If you see "Library not registered" errors:
- **Solution**: Install DoubleAgent instead of original MS Agent
- DoubleAgent properly registers all COM components on modern Windows

Log file location: `MSAgentAI.log` (same folder as the executable)
Access via tray menu: **View Log...**

## Configuration

### Agent Settings
- **Character Folder**: Default is `C:\Windows\msagent\chars`
- Select your preferred character from the available .acs files

### Voice Settings
- **Voice**: Select from available SAPI4 voices
- **Speed**: Adjust speaking speed (50-350)
- **Pitch**: Adjust voice pitch (50-400)
- **Volume**: Adjust volume level (0-100%)

### Ollama AI Settings
- **Ollama URL**: Default is `http://localhost:11434`
- **Model**: Select from available Ollama models
- **Personality Prompt**: Customize the AI's personality
- **Enable Chat**: Toggle AI chat functionality
- **Random Dialog**: Enable random AI-generated dialog
- **Random Chance**: Set the chance of random dialog (1 in N per second)

### Custom Lines
Edit the following types of lines the agent will say:
- **Welcome Lines**: Spoken when the agent first appears
- **Idle Lines**: Spoken randomly while idle
- **Moved Lines**: Spoken when the agent is dragged
- **Clicked Lines**: Spoken when the agent is clicked
- **Exit Lines**: Spoken when exiting
- **Jokes**: Jokes the agent can tell
- **Thoughts**: Thoughts shown in thought bubbles
- **Random Prompts**: Custom prompts sent to Ollama for random dialog

## Building from Source

```bash
cd src
dotnet restore
dotnet build
```

## Usage

1. Right-click the system tray icon to access the menu
2. Go to Settings to configure your agent, voice, and AI options
3. Use Chat to have conversations with the agent (requires Ollama)
4. Use Speak menu to make the agent tell jokes, share thoughts, or say custom text

## Project Structure

```
src/
├── Agent/
│   ├── AgentInterop.cs    # MS Agent COM interop
│   └── AgentManager.cs    # Agent lifecycle management
├── Voice/
│   └── Sapi4Manager.cs    # SAPI4 TTS management
├── AI/
│   └── OllamaClient.cs    # Ollama API client
├── Config/
│   └── AppSettings.cs     # Configuration and persistence
├── UI/
│   ├── MainForm.cs        # Main application form
│   ├── SettingsForm.cs    # Settings dialog
│   ├── ChatForm.cs        # AI chat dialog
│   └── InputDialog.cs     # Simple input dialog
└── Program.cs             # Application entry point
```

## BeamNG Integration

MSAgent-AI includes a BeamNG.drive mod that brings your desktop friend into your driving experience! The AI will comment on:

- 🚗 Your vehicle when you spawn it
- 💥 Crashes and collisions
- 🔧 Damage (dents and scratches)
- 🌍 Your surroundings and driving

### Setup

1. **Install MSAgent-AI** and make sure it's running
2. **Install the bridge server**:
   ```bash
   cd beamng-bridge
   pip install -r requirements.txt
   python bridge.py
   ```

3. **Install the BeamNG mod**:
   - Copy the `beamng-mod` folder to your BeamNG.drive mods directory
   - Windows: `C:\Users\[YourUsername]\AppData\Local\BeamNG.drive\[version]\mods\msagent_ai\`

4. **Start driving!** Your desktop friend will comment on your driving adventures!

See [beamng-mod/README.md](beamng-mod/README.md) for detailed instructions.

## License

MIT License

