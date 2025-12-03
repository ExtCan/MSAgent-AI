using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace MSAgentAI.Agent
{
    /// <summary>
    /// Manages MS Agent character loading, display, and interactions
    /// </summary>
    public class AgentManager : IDisposable
    {
        private dynamic _agentControl;
        private dynamic _character;
        private string _characterId;
        private bool _isLoaded;
        private bool _disposed;

#pragma warning disable CS0067 // Event is never used - these are placeholders for future functionality
        public event EventHandler<AgentEventArgs> OnClick;
        public event EventHandler<AgentEventArgs> OnDragStart;
        public event EventHandler<AgentEventArgs> OnDragComplete;
        public event EventHandler<AgentEventArgs> OnIdle;
#pragma warning restore CS0067

        public string DefaultCharacterPath { get; set; } = @"C:\Windows\msagent\chars";

        public bool IsLoaded => _isLoaded;
        public string CharacterName => _character?.Name ?? string.Empty;
        public string CharacterDescription => _character?.Description ?? string.Empty;

        public AgentManager()
        {
            try
            {
                // Create the MS Agent control
                Type agentType = Type.GetTypeFromProgID("Agent.Control.2");
                if (agentType != null)
                {
                    _agentControl = Activator.CreateInstance(agentType);
                    _agentControl.Connected = true;
                }
            }
            catch (COMException ex)
            {
                throw new AgentException("Failed to create MS Agent control. Ensure MS Agent is installed.", ex);
            }
        }

        /// <summary>
        /// Gets available character files from the default or specified directory
        /// </summary>
        public List<string> GetAvailableCharacters(string path = null)
        {
            var characters = new List<string>();
            string searchPath = path ?? DefaultCharacterPath;

            if (Directory.Exists(searchPath))
            {
                foreach (var file in Directory.GetFiles(searchPath, "*.acs"))
                {
                    characters.Add(file);
                }
            }

            return characters;
        }

        /// <summary>
        /// Loads a character from the specified file path
        /// </summary>
        public void LoadCharacter(string characterPath)
        {
            if (_agentControl == null)
            {
                throw new AgentException("Agent control not initialized.");
            }

            try
            {
                // Unload any existing character
                if (_isLoaded && !string.IsNullOrEmpty(_characterId))
                {
                    UnloadCharacter();
                }

                // Generate a unique ID for this character
                _characterId = Path.GetFileNameWithoutExtension(characterPath);

                // Load the character
                _agentControl.Characters.Load(_characterId, characterPath);
                _character = _agentControl.Characters[_characterId];
                _isLoaded = true;
            }
            catch (COMException ex)
            {
                throw new AgentException($"Failed to load character from '{characterPath}'.", ex);
            }
        }

        /// <summary>
        /// Unloads the current character
        /// </summary>
        public void UnloadCharacter()
        {
            if (_agentControl != null && !string.IsNullOrEmpty(_characterId))
            {
                try
                {
                    _agentControl.Characters.Unload(_characterId);
                }
                catch { }

                _character = null;
                _characterId = null;
                _isLoaded = false;
            }
        }

        /// <summary>
        /// Shows the character on screen
        /// </summary>
        public void Show(bool fast = false)
        {
            EnsureLoaded();
            _character.Show(fast);
        }

        /// <summary>
        /// Hides the character
        /// </summary>
        public void Hide(bool fast = false)
        {
            EnsureLoaded();
            _character.Hide(fast);
        }

        /// <summary>
        /// Makes the character speak the specified text
        /// </summary>
        public void Speak(string text)
        {
            EnsureLoaded();
            if (!string.IsNullOrEmpty(text))
            {
                _character.Speak(text, null);
            }
        }

        /// <summary>
        /// Makes the character think the specified text (shows in thought balloon)
        /// </summary>
        public void Think(string text)
        {
            EnsureLoaded();
            if (!string.IsNullOrEmpty(text))
            {
                _character.Think(text);
            }
        }

        /// <summary>
        /// Plays the specified animation
        /// </summary>
        public void PlayAnimation(string animationName)
        {
            EnsureLoaded();
            if (!string.IsNullOrEmpty(animationName))
            {
                try
                {
                    _character.Play(animationName);
                }
                catch { }
            }
        }

        /// <summary>
        /// Stops all current actions
        /// </summary>
        public void StopAll()
        {
            EnsureLoaded();
            _character.StopAll(null);
        }

        /// <summary>
        /// Moves the character to the specified position
        /// </summary>
        public void MoveTo(int x, int y, int speed = 100)
        {
            EnsureLoaded();
            _character.MoveTo((short)x, (short)y, speed);
        }

        /// <summary>
        /// Gets or sets the character's idle mode
        /// </summary>
        public bool IdleOn
        {
            get
            {
                EnsureLoaded();
                return _character.IdleOn;
            }
            set
            {
                EnsureLoaded();
                _character.IdleOn = value;
            }
        }

        /// <summary>
        /// Gets or sets the character's sound effects mode
        /// </summary>
        public bool SoundEffectsOn
        {
            get
            {
                EnsureLoaded();
                return _character.SoundEffectsOn;
            }
            set
            {
                EnsureLoaded();
                _character.SoundEffectsOn = value;
            }
        }

        /// <summary>
        /// Gets the list of available animations for the current character
        /// </summary>
        public List<string> GetAnimations()
        {
            var animations = new List<string>();

            if (_isLoaded && _character != null)
            {
                try
                {
                    dynamic animNames = _character.AnimationNames;
                    foreach (var anim in animNames)
                    {
                        animations.Add(anim.ToString());
                    }
                }
                catch { }
            }

            // Return common MS Agent animations if we couldn't get them dynamically
            if (animations.Count == 0)
            {
                animations.AddRange(new[]
                {
                    "Idle1_1", "Idle1_2", "Idle1_3", "Idle2_1", "Idle2_2", "Idle3_1", "Idle3_2",
                    "Greet", "Wave", "GestureRight", "GestureLeft", "GestureUp", "GestureDown",
                    "Think", "Explain", "Pleased", "Sad", "Surprised", "Uncertain", "Announce",
                    "Congratulate", "Decline", "DoMagic1", "DoMagic2", "GetAttention",
                    "Hearing_1", "Hearing_2", "Hearing_3", "Hearing_4", "Hide",
                    "Read", "Reading", "RestPose", "Search", "Searching",
                    "Show", "Suggest", "Write", "Writing"
                });
            }

            return animations;
        }

        /// <summary>
        /// Sets the TTS mode ID for the character
        /// </summary>
        public void SetTTSModeID(string modeID)
        {
            EnsureLoaded();
            if (!string.IsNullOrEmpty(modeID))
            {
                _character.TTSModeID = modeID;
            }
        }

        private void EnsureLoaded()
        {
            if (!_isLoaded || _character == null)
            {
                throw new AgentException("No character is currently loaded.");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                UnloadCharacter();

                if (_agentControl != null)
                {
                    try
                    {
                        _agentControl.Connected = false;
                        Marshal.ReleaseComObject(_agentControl);
                    }
                    catch { }
                    _agentControl = null;
                }

                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Event arguments for agent events
    /// </summary>
    public class AgentEventArgs : EventArgs
    {
        public string CharacterId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>
    /// Exception for agent-related errors
    /// </summary>
    public class AgentException : Exception
    {
        public AgentException(string message) : base(message) { }
        public AgentException(string message, Exception inner) : base(message, inner) { }
    }
}
