using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MSAgentAI.Agent;
using MSAgentAI.AI;
using MSAgentAI.Config;
using MSAgentAI.Logging;
using MSAgentAI.Voice;

namespace MSAgentAI.UI
{
    /// <summary>
    /// Main application form with system tray support
    /// </summary>
    public partial class MainForm : Form
    {
        // Constants
        private const int IdleDialogChancePercent = 20; // 20% chance when idle timer ticks

        private AgentManager _agentManager;
        private Sapi4Manager _voiceManager;
        private OllamaClient _ollamaClient;
        private AppSettings _settings;

        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayMenu;
        private System.Windows.Forms.Timer _idleTimer;
        private System.Windows.Forms.Timer _randomDialogTimer;
        private Random _random = new Random();

        private CancellationTokenSource _cancellationTokenSource;

        public MainForm()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties - we make it invisible since the agent shows on desktop
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1, 1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.Text = "MSAgent AI Desktop Friend";
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.Opacity = 0;
            
            this.ResumeLayout(false);
        }

        private void InitializeApplication()
        {
            // Load settings
            _settings = AppSettings.Load();

            // Initialize managers
            InitializeManagers();

            // Create system tray icon and menu
            CreateTrayIcon();

            // Initialize timers
            InitializeTimers();

            // Load the agent if a character is selected
            LoadAgentFromSettings();

            // Show welcome message
            ShowWelcomeMessage();
        }

        private void InitializeManagers()
        {
            try
            {
                _agentManager = new AgentManager
                {
                    DefaultCharacterPath = _settings.CharacterPath
                };
                
                // Subscribe to agent events
                _agentManager.OnClick += OnAgentClicked;
                _agentManager.OnDragComplete += OnAgentMoved;
            }
            catch (Exception ex)
            {
                ShowError("Agent Initialization Error", 
                    $"Failed to initialize MS Agent.\n\n" +
                    $"Possible solutions:\n" +
                    $"1. Ensure MS Agent is installed (msagent.exe)\n" +
                    $"2. Run the application as Administrator\n" +
                    $"3. Register the MS Agent COM components manually\n\n" +
                    $"Error details: {ex.Message}");
            }

            try
            {
                _voiceManager = new Sapi4Manager
                {
                    Speed = _settings.VoiceSpeed,
                    Pitch = _settings.VoicePitch,
                    Volume = _settings.VoiceVolume
                };

                if (!string.IsNullOrEmpty(_settings.SelectedVoiceId))
                {
                    _voiceManager.SetVoice(_settings.SelectedVoiceId);
                }
            }
            catch (Exception ex)
            {
                ShowError("Voice Initialization Error",
                    $"Failed to initialize SAPI4. Please ensure SAPI4 is installed.\n\nError: {ex.Message}");
            }

            _ollamaClient = new OllamaClient
            {
                BaseUrl = _settings.OllamaUrl,
                Model = _settings.OllamaModel,
                PersonalityPrompt = _settings.PersonalityPrompt
            };

            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void CreateTrayIcon()
        {
            _trayMenu = new ContextMenuStrip();

            // Add menu items
            var showAgentItem = new ToolStripMenuItem("Show/Hide Agent", null, OnShowHideAgent);
            var settingsItem = new ToolStripMenuItem("Settings...", null, OnOpenSettings);
            var chatItem = new ToolStripMenuItem("Chat...", null, OnOpenChat);
            var speakItem = new ToolStripMenuItem("Speak", null);
            var pokeItem = new ToolStripMenuItem("Poke (Random AI)", null, OnPoke);
            var separatorItem1 = new ToolStripSeparator();

            // Speak submenu
            var speakJokeItem = new ToolStripMenuItem("Tell a Joke", null, OnSpeakJoke);
            var speakThoughtItem = new ToolStripMenuItem("Share a Thought", null, OnSpeakThought);
            var speakCustomItem = new ToolStripMenuItem("Say Something...", null, OnSpeakCustom);
            var askOllamaItem = new ToolStripMenuItem("Ask Ollama...", null, OnAskOllama);
            speakItem.DropDownItems.AddRange(new ToolStripItem[] { speakJokeItem, speakThoughtItem, speakCustomItem, askOllamaItem });

            var separatorItem2 = new ToolStripSeparator();
            var viewLogItem = new ToolStripMenuItem("View Log...", null, OnViewLog);
            var aboutItem = new ToolStripMenuItem("About", null, OnAbout);
            var exitItem = new ToolStripMenuItem("Exit", null, OnExit);

            _trayMenu.Items.AddRange(new ToolStripItem[]
            {
                showAgentItem,
                separatorItem1,
                settingsItem,
                chatItem,
                speakItem,
                pokeItem,
                separatorItem2,
                viewLogItem,
                aboutItem,
                exitItem
            });

            // Create tray icon
            _trayIcon = new NotifyIcon
            {
                Text = "MSAgent AI Desktop Friend",
                Icon = SystemIcons.Application,
                ContextMenuStrip = _trayMenu,
                Visible = true
            };

            _trayIcon.DoubleClick += OnTrayDoubleClick;
        }

        private void InitializeTimers()
        {
            // Idle timer - triggers idle lines periodically (60 seconds - more spaced out)
            _idleTimer = new System.Windows.Forms.Timer
            {
                Interval = 60000 // 60 seconds - spaced out more
            };
            _idleTimer.Tick += OnIdleTimerTick;
            _idleTimer.Start();

            // Random dialog timer - checks every second for random dialog
            _randomDialogTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000 // 1 second
            };
            _randomDialogTimer.Tick += OnRandomDialogTimerTick;
            if (_settings.EnableRandomDialog)
            {
                _randomDialogTimer.Start();
            }
        }

        private void LoadAgentFromSettings()
        {
            if (_agentManager != null && !string.IsNullOrEmpty(_settings.SelectedCharacterFile))
            {
                try
                {
                    if (File.Exists(_settings.SelectedCharacterFile))
                    {
                        _agentManager.LoadCharacter(_settings.SelectedCharacterFile);
                        _agentManager.Show(false);
                        _agentManager.IdleOn = true;

                        // Set TTS mode if voice is selected
                        if (!string.IsNullOrEmpty(_settings.SelectedVoiceId))
                        {
                            _agentManager.SetTTSModeID(_settings.SelectedVoiceId);
                        }
                        
                        // Apply voice speed, pitch, and volume
                        ApplyVoiceSettingsToAgent();
                        
                        // Apply agent size
                        if (_settings.AgentSize != 100)
                        {
                            _agentManager.SetSize(_settings.AgentSize);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError("Character Load Error", $"Failed to load character: {ex.Message}");
                }
            }
        }

        private void ShowWelcomeMessage()
        {
            if (_agentManager?.IsLoaded == true)
            {
                var welcomeLine = GetRandomLine(_settings.WelcomeLines);
                if (!string.IsNullOrEmpty(welcomeLine))
                {
                    SpeakWithAnimations(welcomeLine, "Greet");
                }
            }
        }
        
        /// <summary>
        /// Speaks text with animation support. Extracts &&Animation triggers from text,
        /// plays ONLY the first animation, then speaks the processed text.
        /// MS Agent limitation: can only play one animation at a time before speaking.
        /// </summary>
        private void SpeakWithAnimations(string text, string defaultAnimation = null)
        {
            if (_agentManager?.IsLoaded != true || string.IsNullOrEmpty(text))
                return;
                
            // Extract animation triggers (&&AnimationName)
            var (cleanText, animations) = AppSettings.ExtractAnimationTriggers(text);
            
            // Process text for ## name replacement and /emp/ emphasis
            cleanText = _settings.ProcessText(cleanText);
            
            // Play ONLY THE FIRST animation (MS Agent limitation)
            if (animations.Count > 0)
            {
                _agentManager.PlayAnimation(animations[0]);
            }
            else if (!string.IsNullOrEmpty(defaultAnimation))
            {
                _agentManager.PlayAnimation(defaultAnimation);
            }
            
            // Speak the processed text
            _agentManager.Speak(cleanText);
        }
        
        /// <summary>
        /// Gets a random line from the specified list (static helper)
        /// </summary>
        private static string GetRandomLine(List<string> lines)
        {
            return AppSettings.GetRandomLine(lines);
        }

        #region Event Handlers

        private void OnShowHideAgent(object sender, EventArgs e)
        {
            if (_agentManager?.IsLoaded == true)
            {
                _agentManager.Hide(false);
                _agentManager.Show(false);
            }
            else
            {
                MessageBox.Show("No agent is currently loaded. Please go to Settings and select a character.",
                    "No Agent", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnOpenSettings(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm(_settings, _agentManager, _voiceManager, _ollamaClient))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Reload settings
                    ApplySettings();
                }
            }
        }

        private void OnOpenChat(object sender, EventArgs e)
        {
            if (!_settings.EnableOllamaChat)
            {
                MessageBox.Show("Ollama chat is not enabled. Please enable it in Settings.",
                    "Chat Disabled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var chatForm = new ChatForm(_ollamaClient, _agentManager, _settings))
            {
                chatForm.ShowDialog();
            }
        }

        private void OnSpeakJoke(object sender, EventArgs e)
        {
            if (_agentManager?.IsLoaded == true)
            {
                var joke = GetRandomLine(_settings.Jokes);
                if (!string.IsNullOrEmpty(joke))
                {
                    SpeakWithAnimations(joke, "Pleased");
                }
            }
        }

        private void OnSpeakThought(object sender, EventArgs e)
        {
            if (_agentManager?.IsLoaded == true)
            {
                var thought = GetRandomLine(_settings.Thoughts);
                if (!string.IsNullOrEmpty(thought))
                {
                    // Process text for name and emphasis
                    thought = _settings.ProcessText(thought);
                    _agentManager.Think(thought);
                }
            }
        }

        private void OnSpeakCustom(object sender, EventArgs e)
        {
            if (_agentManager?.IsLoaded != true)
            {
                MessageBox.Show("No agent is currently loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new InputDialog("Speak", "Enter text for the agent to say:\n(Use &&Animation for animations, /emp/ for emphasis, ## for name)"))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dialog.InputText))
                {
                    SpeakWithAnimations(dialog.InputText);
                }
            }
        }

        private async void OnAskOllama(object sender, EventArgs e)
        {
            if (!_settings.EnableOllamaChat)
            {
                MessageBox.Show("Ollama chat is not enabled. Please enable it in Settings.",
                    "Chat Disabled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new InputDialog("Ask Ollama", "Enter a prompt for Ollama AI:"))
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dialog.InputText))
                {
                    try
                    {
                        var response = await _ollamaClient.ChatAsync(dialog.InputText, _cancellationTokenSource.Token);
                        if (!string.IsNullOrEmpty(response) && _agentManager?.IsLoaded == true)
                        {
                            SpeakWithAnimations(response);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to get response from Ollama: {ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Poke - triggers a random AI-generated dialog on demand
        /// </summary>
        private async void OnPoke(object sender, EventArgs e)
        {
            if (!_settings.EnableOllamaChat)
            {
                MessageBox.Show("Ollama chat is not enabled. Please enable it in Settings to use Poke.",
                    "Chat Disabled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var prompt = GetRandomLine(_settings.RandomDialogPrompts);
                if (!string.IsNullOrEmpty(prompt))
                {
                    var response = await _ollamaClient.GenerateRandomDialogAsync(prompt, _cancellationTokenSource.Token);
                    if (!string.IsNullOrEmpty(response) && _agentManager?.IsLoaded == true)
                    {
                        SpeakWithAnimations(response);
                    }
                }
                else
                {
                    MessageBox.Show("No random prompts configured. Please add some in Settings > Lines > Random Prompts.",
                        "No Prompts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get response from Ollama: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnViewLog(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Logger.LogFilePath) && File.Exists(Logger.LogFilePath))
                {
                    Logger.OpenLogFile();
                }
                else
                {
                    MessageBox.Show($"Log file not found at:\n{Logger.LogFilePath}", "Log", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open log file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnAbout(object sender, EventArgs e)
        {
            MessageBox.Show(
                "MSAgent AI Desktop Friend\n\n" +
                "A desktop companion using MS Agents and SAPI4 TTS\n" +
                "with Ollama AI integration for dynamic conversations.\n\n" +
                "Version 1.0.0\n\n" +
                "Inspired by BonziBUDDY and CyberBuddy.",
                "About MSAgent AI",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OnExit(object sender, EventArgs e)
        {
            // Show exit message
            if (_agentManager?.IsLoaded == true)
            {
                var exitLine = GetRandomLine(_settings.ExitLines);
                if (!string.IsNullOrEmpty(exitLine))
                {
                    SpeakWithAnimations(exitLine, "Wave");
                    // Use a timer to wait for speech before exiting instead of blocking the UI
                    var exitTimer = new System.Windows.Forms.Timer { Interval = 2000 };
                    exitTimer.Tick += (s, args) =>
                    {
                        exitTimer.Stop();
                        exitTimer.Dispose();
                        _agentManager?.Hide(false);
                        CleanUp();
                        Application.Exit();
                    };
                    exitTimer.Start();
                    return;
                }
                _agentManager.Hide(false);
            }

            // Clean up and exit
            CleanUp();
            Application.Exit();
        }

        private void OnTrayDoubleClick(object sender, EventArgs e)
        {
            OnOpenSettings(sender, e);
        }

        private void OnIdleTimerTick(object sender, EventArgs e)
        {
            // Only use prewritten idle if enabled
            if (_agentManager?.IsLoaded == true && _settings.EnablePrewrittenIdle)
            {
                // 1 in N chance (configurable)
                if (_random.Next(_settings.PrewrittenIdleChance) == 0)
                {
                    var idleLine = GetRandomLine(_settings.IdleLines);
                    if (!string.IsNullOrEmpty(idleLine))
                    {
                        SpeakWithAnimations(idleLine, "Idle1_1");
                    }
                }
            }
        }

        private async void OnRandomDialogTimerTick(object sender, EventArgs e)
        {
            if (!_settings.EnableRandomDialog || !_settings.EnableOllamaChat)
                return;

            // 1 in N chance (default 9000)
            if (_random.Next(_settings.RandomDialogChance) == 0)
            {
                try
                {
                    var prompt = GetRandomLine(_settings.RandomDialogPrompts);
                    if (!string.IsNullOrEmpty(prompt))
                    {
                        var response = await _ollamaClient.GenerateRandomDialogAsync(prompt, _cancellationTokenSource.Token);
                        if (!string.IsNullOrEmpty(response) && _agentManager?.IsLoaded == true)
                        {
                            SpeakWithAnimations(response);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Random dialog error: {ex.Message}");
                }
            }
        }

        private void OnAgentClicked(object sender, Agent.AgentEventArgs e)
        {
            if (_agentManager?.IsLoaded == true)
            {
                var clickedLine = GetRandomLine(_settings.ClickedLines);
                if (!string.IsNullOrEmpty(clickedLine))
                {
                    SpeakWithAnimations(clickedLine, "Surprised");
                }
            }
        }

        private void OnAgentMoved(object sender, Agent.AgentEventArgs e)
        {
            if (_agentManager?.IsLoaded == true)
            {
                var movedLine = GetRandomLine(_settings.MovedLines);
                if (!string.IsNullOrEmpty(movedLine))
                {
                    SpeakWithAnimations(movedLine);
                }
            }
        }

        #endregion

        private void ApplySettings()
        {
            // Update voice settings
            if (_voiceManager != null)
            {
                _voiceManager.Speed = _settings.VoiceSpeed;
                _voiceManager.Pitch = _settings.VoicePitch;
                _voiceManager.Volume = _settings.VoiceVolume;

                if (!string.IsNullOrEmpty(_settings.SelectedVoiceId))
                {
                    _voiceManager.SetVoice(_settings.SelectedVoiceId);
                }
            }
            
            // Apply voice settings to agent for actual TTS
            ApplyVoiceSettingsToAgent();

            // Update Ollama settings
            if (_ollamaClient != null)
            {
                _ollamaClient.BaseUrl = _settings.OllamaUrl;
                _ollamaClient.Model = _settings.OllamaModel;
                _ollamaClient.PersonalityPrompt = _settings.PersonalityPrompt;
                
                // Update available animations for AI to use
                if (_agentManager?.IsLoaded == true)
                {
                    _ollamaClient.AvailableAnimations = _agentManager.GetAnimations();
                }
            }

            // Update random dialog timer
            if (_settings.EnableRandomDialog)
            {
                _randomDialogTimer.Start();
            }
            else
            {
                _randomDialogTimer.Stop();
            }

            // Reload character if changed
            if (!string.IsNullOrEmpty(_settings.SelectedCharacterFile))
            {
                try
                {
                    if (_agentManager.IsLoaded)
                    {
                        string currentPath = _settings.SelectedCharacterFile;
                        // Check if we need to reload
                        _agentManager.LoadCharacter(currentPath);
                        _agentManager.Show(false);
                        
                        // Apply agent size
                        _agentManager.SetSize(_settings.AgentSize);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to reload character: {ex.Message}");
                }
            }

            // Save settings
            _settings.Save();
        }
        
        /// <summary>
        /// Applies voice speed, pitch, and volume settings to the agent
        /// </summary>
        private void ApplyVoiceSettingsToAgent()
        {
            if (_agentManager?.IsLoaded == true)
            {
                // Convert 75-250 slider range to SAPI4 85-400 range
                // Settings slider uses 75-250, SAPI4 uses 85-400
                // Linear interpolation: output = 85 + (input - 75) * (400 - 85) / (250 - 75)
                int speed = 85 + (_settings.VoiceSpeed - 75) * 315 / 175;
                _agentManager.SetSpeechSpeed(speed);
                
                // Pitch: settings uses 50-400, same as SAPI4
                _agentManager.SetSpeechPitch(_settings.VoicePitch);
                
                // Volume: settings uses 0-65535, same as SAPI4
                _agentManager.SetSpeechVolume(_settings.VoiceVolume);
                
                // Also set the TTS mode (voice) if selected
                if (!string.IsNullOrEmpty(_settings.SelectedVoiceId))
                {
                    _agentManager.SetTTSModeID(_settings.SelectedVoiceId);
                }
            }
        }

        private void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void CleanUp()
        {
            _cancellationTokenSource?.Cancel();
            _idleTimer?.Stop();
            _randomDialogTimer?.Stop();

            _trayIcon?.Dispose();
            _agentManager?.Dispose();
            _voiceManager?.Dispose();
            _ollamaClient?.Dispose();

            _settings?.Save();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Minimize to tray instead of closing
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                CleanUp();
            }
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CleanUp();
            }
            base.Dispose(disposing);
        }
    }
}
