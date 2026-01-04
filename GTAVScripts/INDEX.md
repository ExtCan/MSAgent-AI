# GTA V MSAgent-AI Integration - Documentation Index

Welcome to the MSAgent-AI GTA V integration! This index will help you find the right documentation for your needs.

## 📚 Documentation Files

### For First-Time Users

**Start Here:**
1. **[QUICKSTART.md](QUICKSTART.md)** ⭐
   - 5-minute installation guide
   - Prerequisites checklist
   - First launch instructions
   - Quick test procedure

2. **[README.md](README.md)**
   - Complete installation guide
   - Detailed feature list
   - Usage instructions
   - System requirements

### Need Help?

**Having Issues?**
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)**
  - Installation verification checklist
  - Common error scenarios and solutions
  - Step-by-step debugging guide
  - Performance tips

### Want to Learn More?

**Explore the Features:**
- **[FEATURES.md](FEATURES.md)**
  - Visual feature demonstration
  - Example reactions by category
  - Cooldown system explained
  - Typical play session walkthrough

### Advanced Users

**Customize and Extend:**
- **[ADVANCED.md](ADVANCED.md)**
  - Customizing prompts
  - Adding new reaction types
  - Advanced menu features
  - Performance optimization
  - Integration with other mods
  - Easter eggs and fun ideas

### Configuration

**Setup Files:**
- **[config.example.json](config.example.json)**
  - Configuration template
  - Setting descriptions
  - Customization options

### Building & Distribution

**Pre-Built Artifacts:**
- **[GitHub Actions](../../actions/workflows/build-gtav-script.yml)** ⭐
  - Download pre-built MSAgentGTAV.dll
  - No compilation needed
  - Automatically built on every update

**Build Documentation:**
- **[BUILD_WORKFLOW.md](BUILD_WORKFLOW.md)**
  - Automated build system explained
  - How to download artifacts
  - CI/CD pipeline details

**Development:**
- **[MSAgentGTAV.csproj](MSAgentGTAV.csproj)**
  - Visual Studio project file
  - Build configuration

- **[build.bat](build.bat)**
  - Automated build script
  - Command-line building

- **[MSAgentGTAV.cs](MSAgentGTAV.cs)**
  - Main script source code
  - ~500 lines of C# code

## 🚀 Quick Navigation

### I want to...

| Goal | Go to |
|------|-------|
| **Download pre-built DLL** | **[GitHub Actions Artifacts](../../actions/workflows/build-gtav-script.yml)** ⭐ |
| Install the script for the first time | [QUICKSTART.md](QUICKSTART.md) |
| Build from source | [README.md](README.md#building-from-source) |
| Fix a problem | [TROUBLESHOOTING.md](TROUBLESHOOTING.md) |
| See what it can do | [FEATURES.md](FEATURES.md) |
| Customize the script | [ADVANCED.md](ADVANCED.md) |
| Change keybindings | [README.md](README.md#customizing-the-key-binding) |
| Add new reactions | [ADVANCED.md](ADVANCED.md#adding-new-reaction-types) |
| Adjust cooldown times | [ADVANCED.md](ADVANCED.md#performance-optimization) |
| Understand the build system | [BUILD_WORKFLOW.md](BUILD_WORKFLOW.md) |
| Report a bug | Create an issue on GitHub |

## 📖 Recommended Reading Order

### New Users:
1. Read [QUICKSTART.md](QUICKSTART.md)
2. Install following the steps
3. Launch and test
4. If issues occur, check [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
5. Explore [FEATURES.md](FEATURES.md) to see what's possible

### Advanced Users:
1. Skim [README.md](README.md) for overview
2. Jump to [ADVANCED.md](ADVANCED.md) for customization
3. Review source code in [MSAgentGTAV.cs](MSAgentGTAV.cs)
4. Use [config.example.json](config.example.json) as a template

### Developers:
1. Read [README.md](README.md) for architecture
2. Study [MSAgentGTAV.cs](MSAgentGTAV.cs) source code
3. Check [ADVANCED.md](ADVANCED.md) for extension patterns
4. Review [../PIPELINE.md](../PIPELINE.md) for pipe protocol

## 🔗 External Resources

### Required Downloads:
- **MSAgent-AI**: Main application (this repository)
- **DoubleAgent**: https://doubleagent.sourceforge.net/
- **ScriptHookV**: http://www.dev-c.com/gtav/scripthookv/
- **ScriptHookVDotNet**: https://github.com/scripthookvdotnet/scripthookvdotnet/releases
- **Ollama** (optional): https://ollama.ai

### Related Documentation:
- **[Main README](../README.md)**: MSAgent-AI overview
- **[PIPELINE.md](../PIPELINE.md)**: Named pipe API documentation
- **[REQUIREMENTS.txt](../REQUIREMENTS.txt)**: System requirements

## 💡 Tips

### First Time Setup:
1. ⏱️ Budget 10-15 minutes for first installation
2. 📋 Install prerequisites in order
3. ✅ Test MSAgent-AI alone before adding GTA V script
4. 🎮 Make sure GTA V works with ScriptHookV before adding this script

### Getting Best Results:
1. 🤖 Enable Ollama AI for intelligent responses
2. 🎛️ Adjust reaction toggles to your preference
3. ⏰ Modify cooldowns if too chatty or too quiet
4. 🎭 Customize personality prompts in MSAgent-AI settings

### Performance:
1. 💨 Use faster Ollama models (llama3.2:1b) for quick responses
2. 🎯 Disable unused reaction types
3. 📊 Monitor MSAgent-AI CPU usage
4. 🔧 Adjust cooldowns higher if needed

## ❓ Still Have Questions?

1. **Check the FAQ** in [README.md](README.md)
2. **Search existing GitHub issues**
3. **Review the troubleshooting guide** in [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
4. **Create a new GitHub issue** with details:
   - Your GTA V version
   - ScriptHookV version
   - ScriptHookVDotNet version
   - Error messages or unexpected behavior
   - Log files (MSAgentAI.log, ScriptHookV.log)

## 🎉 Have Fun!

The script is designed to make GTA V more entertaining with live commentary. Experiment with different settings, customize the prompts, and enjoy your AI-powered gaming companion!

---

**Last Updated**: December 2024  
**Script Version**: 1.0.0  
**Compatible with**: GTA V (PC), ScriptHookV, ScriptHookVDotNet v3.x
