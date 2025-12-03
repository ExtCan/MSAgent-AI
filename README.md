# MSAgent-AI

A Windows desktop friend application inspired by BonziBUDDY and CyberBuddy, using Microsoft Agent characters with SAPI4 text-to-speech and Ollama AI integration for dynamic conversations.

## Features

- **MS Agent Character Support**: Load and display Microsoft Agent characters (.acs files) from your system
- **SAPI4 Text-to-Speech**: Full SAPI4 voice support with configurable Speed, Pitch, and Volume
- **Customizable Lines**: Edit welcome, idle, moved, exit, clicked, jokes, and thoughts lines
- **Ollama AI Integration**: Connect to Ollama for dynamic AI-powered conversations with personality prompting
- **Random Dialog**: Configurable random dialog feature (1 in 9000 chance per second by default) that sends custom prompts to Ollama
- **User-Friendly GUI**: System tray application with comprehensive settings panel

## Requirements

- Windows with .NET Framework 4.8 or later
- Microsoft Agent (msagent.exe) installed
- SAPI4 Text-to-Speech engine installed
- Ollama (optional, for AI chat features) - https://ollama.ai

## Installation

1. Install Microsoft Agent on your Windows system
2. Install SAPI4 and at least one SAPI4 voice
3. Download and install Ollama if you want AI chat features
4. Build the application using Visual Studio or `dotnet build`
5. Run the MSAgentAI.exe

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

## License

MIT License

