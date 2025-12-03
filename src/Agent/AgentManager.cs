using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MSAgentAI.Agent
{
    /// <summary>
    /// Manages MS Agent character loading, display, and interactions
    /// </summary>
    public class AgentManager : IDisposable
    {
        private dynamic _agentServer;
        private dynamic _character;
        private int _characterId;
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
        public string CharacterName => _isLoaded && _character != null ? GetCharacterName() : string.Empty;
        public string CharacterDescription => _isLoaded && _character != null ? GetCharacterDescription() : string.Empty;
        
        private string GetCharacterName()
        {
            try { return _character.Name; } catch { return string.Empty; }
        }
        
        private string GetCharacterDescription()
        {
            try { return _character.Description; } catch { return string.Empty; }
        }

        public AgentManager()
        {
            InitializeAgent();
        }
        
        private void InitializeAgent()
        {
            Exception lastException = null;
            
            // Method 1: Try AgentServer.Agent (the COM server, not the ActiveX control)
            if (TryCreateAgentServer("AgentServer.Agent", ref lastException))
                return;
            
            // Method 2: Try Agent.Control.2 (ActiveX style, may work in some cases)
            if (TryCreateAgentServer("Agent.Control.2", ref lastException))
                return;
                
            // Method 3: Try Agent.Control.1 
            if (TryCreateAgentServer("Agent.Control.1", ref lastException))
                return;
            
            // Method 4: Try by CLSID for AgentServer
            // AgentServer CLSID: {D45FD31B-5C6E-11D1-9EC1-00C04FD7081F}
            if (TryCreateAgentServerByCLSID(new Guid("D45FD31B-5C6E-11D1-9EC1-00C04FD7081F"), ref lastException))
                return;
            
            // Method 5: Try by CLSID for Agent Control
            // Agent.Control CLSID: {D45FD31D-5C6E-11D1-9EC1-00C04FD7081F}
            if (TryCreateAgentServerByCLSID(new Guid("D45FD31D-5C6E-11D1-9EC1-00C04FD7081F"), ref lastException))
                return;
            
            // Check if MS Agent is installed by looking at registry
            string diagnosticInfo = GetMSAgentDiagnostics();
            
            throw new AgentException(
                $"Failed to initialize MS Agent.\n\n" +
                $"Diagnostic Information:\n{diagnosticInfo}\n\n" +
                $"Please ensure:\n" +
                $"1. Microsoft Agent is installed (run regsvr32 agentsvr.exe as Admin)\n" +
                $"2. AgentServer is registered (regsvr32 agentctl.dll as Admin)\n" +
                $"3. You're running on a compatible Windows version\n\n" +
                $"Last Error: {lastException?.Message ?? "Unknown"}", lastException);
        }
        
        private bool TryCreateAgentServer(string progId, ref Exception lastException)
        {
            try
            {
                Type agentType = Type.GetTypeFromProgID(progId, false);
                if (agentType == null) return false;
                
                _agentServer = Activator.CreateInstance(agentType);
                if (_agentServer == null) return false;
                
                // Try to access the Characters collection to verify it works
                try
                {
                    var test = _agentServer.Characters;
                }
                catch
                {
                    // Some ProgIDs need Connected = true first
                    try
                    {
                        _agentServer.Connected = true;
                    }
                    catch
                    {
                        // Connected property may not exist on AgentServer
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Successfully initialized MS Agent using ProgID: {progId}");
                return true;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _agentServer = null;
                return false;
            }
        }
        
        private bool TryCreateAgentServerByCLSID(Guid clsid, ref Exception lastException)
        {
            try
            {
                Type agentType = Type.GetTypeFromCLSID(clsid, false);
                if (agentType == null) return false;
                
                _agentServer = Activator.CreateInstance(agentType);
                if (_agentServer == null) return false;
                
                // Try to access the Characters collection to verify it works
                try
                {
                    var test = _agentServer.Characters;
                }
                catch
                {
                    // Some CLSIDs need Connected = true first
                    try
                    {
                        _agentServer.Connected = true;
                    }
                    catch
                    {
                        // Connected property may not exist on AgentServer
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Successfully initialized MS Agent using CLSID: {clsid}");
                return true;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _agentServer = null;
                return false;
            }
        }
        
        private string GetMSAgentDiagnostics()
        {
            var diagnostics = new List<string>();
            
            // Check for Agent Server registration
            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey(@"CLSID\{D45FD31B-5C6E-11D1-9EC1-00C04FD7081F}"))
                {
                    diagnostics.Add(key != null ? "✓ AgentServer CLSID registered" : "✗ AgentServer CLSID NOT registered");
                }
            }
            catch
            {
                diagnostics.Add("✗ Cannot check AgentServer CLSID");
            }
            
            // Check for Agent Control registration
            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey(@"CLSID\{D45FD31D-5C6E-11D1-9EC1-00C04FD7081F}"))
                {
                    diagnostics.Add(key != null ? "✓ Agent.Control CLSID registered" : "✗ Agent.Control CLSID NOT registered");
                }
            }
            catch
            {
                diagnostics.Add("✗ Cannot check Agent.Control CLSID");
            }
            
            // Check for AgentServer.Agent ProgID
            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey(@"AgentServer.Agent"))
                {
                    diagnostics.Add(key != null ? "✓ AgentServer.Agent ProgID registered" : "✗ AgentServer.Agent ProgID NOT registered");
                }
            }
            catch
            {
                diagnostics.Add("✗ Cannot check AgentServer.Agent ProgID");
            }
            
            // Check for Agent.Control.2 ProgID
            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey(@"Agent.Control.2"))
                {
                    diagnostics.Add(key != null ? "✓ Agent.Control.2 ProgID registered" : "✗ Agent.Control.2 ProgID NOT registered");
                }
            }
            catch
            {
                diagnostics.Add("✗ Cannot check Agent.Control.2 ProgID");
            }
            
            // Check for MS Agent DLLs
            string sysDir = Environment.SystemDirectory;
            string agentDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "msagent");
            
            string[] filesToCheck = new[]
            {
                Path.Combine(sysDir, "agentsvr.exe"),
                Path.Combine(sysDir, "agentctl.dll"),
                Path.Combine(agentDir, "agentctl.dll"),
                Path.Combine(agentDir, "agentdpv.dll")
            };
            
            foreach (var file in filesToCheck)
            {
                diagnostics.Add(File.Exists(file) ? $"✓ {file} exists" : $"✗ {file} NOT found");
            }
            
            // Check for character files
            string charsDir = Path.Combine(agentDir, "chars");
            if (Directory.Exists(charsDir))
            {
                var charFiles = Directory.GetFiles(charsDir, "*.acs");
                diagnostics.Add($"✓ Character directory exists with {charFiles.Length} character(s)");
            }
            else
            {
                diagnostics.Add("✗ Character directory NOT found");
            }
            
            return string.Join("\n", diagnostics);
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
            if (_agentServer == null)
            {
                throw new AgentException("Agent server not initialized.");
            }

            Exception firstException = null;
            Exception secondException = null;
            
            try
            {
                // Unload any existing character
                if (_isLoaded && _characterId != 0)
                {
                    UnloadCharacter();
                }

                // Method 1: Try AgentServer.Load (IAgentEx interface)
                try
                {
                    object charId = null;
                    object requestId = null;
                    _agentServer.Load(characterPath, ref charId, ref requestId);
                    _characterId = Convert.ToInt32(charId);
                    _character = _agentServer.Character(_characterId);
                    _isLoaded = true;
                    System.Diagnostics.Debug.WriteLine($"Character loaded using AgentServer.Load: {characterPath}");
                    return;
                }
                catch (Exception ex)
                {
                    firstException = ex;
                    System.Diagnostics.Debug.WriteLine($"AgentServer.Load failed: {ex.Message}");
                }

                // Method 2: Try Agent.Control style (Characters collection)
                try
                {
                    string charName = Path.GetFileNameWithoutExtension(characterPath);
                    dynamic characters = _agentServer.Characters;
                    characters.Load(charName, characterPath);
                    _character = characters[charName];
                    _characterId = charName.GetHashCode();
                    _isLoaded = true;
                    System.Diagnostics.Debug.WriteLine($"Character loaded using Characters.Load: {characterPath}");
                    return;
                }
                catch (Exception ex)
                {
                    secondException = ex;
                    System.Diagnostics.Debug.WriteLine($"Characters.Load failed: {ex.Message}");
                }
                
                // Method 3: Try loading with just the filename (some versions expect this)
                try
                {
                    string charName = Path.GetFileNameWithoutExtension(characterPath);
                    dynamic characters = _agentServer.Characters;
                    characters.Load(charName, (object)characterPath);
                    _character = characters.Character(charName);
                    _characterId = charName.GetHashCode();
                    _isLoaded = true;
                    System.Diagnostics.Debug.WriteLine($"Character loaded using Characters.Character: {characterPath}");
                    return;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Characters.Character failed: {ex.Message}");
                }

                // All methods failed
                string errorMsg = $"Failed to load character from '{characterPath}'.\n\n";
                if (firstException != null)
                    errorMsg += $"Method 1 (AgentServer.Load): {firstException.Message}\n";
                if (secondException != null)
                    errorMsg += $"Method 2 (Characters.Load): {secondException.Message}\n";
                
                throw new AgentException(errorMsg);
            }
            catch (AgentException)
            {
                throw;
            }
            catch (COMException ex)
            {
                throw new AgentException($"COM error loading character from '{characterPath}': 0x{ex.ErrorCode:X8} - {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new AgentException($"Unexpected error loading character from '{characterPath}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Unloads the current character
        /// </summary>
        public void UnloadCharacter()
        {
            if (_agentServer != null && _characterId != 0)
            {
                try
                {
                    // Try AgentServer.Unload first
                    try
                    {
                        _agentServer.Unload(_characterId);
                    }
                    catch
                    {
                        // Fall back to Characters.Unload for Agent.Control style
                        try
                        {
                            if (_character != null)
                            {
                                string name = _character.Name;
                                _agentServer.Characters.Unload(name);
                            }
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error unloading character: {ex.Message}");
                }

                _character = null;
                _characterId = 0;
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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error playing animation '{animationName}': {ex.Message}");
                }
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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting animations: {ex.Message}");
                }
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

                if (_agentServer != null)
                {
                    try
                    {
                        // Try to disconnect if the property exists
                        try
                        {
                            _agentServer.Connected = false;
                        }
                        catch { }
                        
                        Marshal.ReleaseComObject(_agentServer);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error disposing agent server: {ex.Message}");
                    }
                    _agentServer = null;
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
