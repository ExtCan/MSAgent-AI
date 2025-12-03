using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using MSAgentAI.Logging;

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
            
            Logger.Log("Initializing MS Agent...");
            
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
            
            Logger.LogError("Failed to initialize MS Agent", lastException);
            
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
                Logger.Log($"Trying to create agent server with ProgID: {progId}");
                
                Type agentType = Type.GetTypeFromProgID(progId, false);
                if (agentType == null)
                {
                    Logger.Log($"ProgID {progId} not found");
                    return false;
                }
                
                _agentServer = Activator.CreateInstance(agentType);
                if (_agentServer == null)
                {
                    Logger.Log($"Failed to create instance for ProgID {progId}");
                    return false;
                }
                
                // Try to set Connected = true using InvokeMember (avoids type library requirement)
                try
                {
                    agentType.InvokeMember("Connected", 
                        System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
                        null, _agentServer, new object[] { true });
                    Logger.Log($"Set Connected = true for {progId}");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Connected property not available or failed: {ex.Message}");
                    // Connected property may not exist on AgentServer - continue anyway
                }
                
                Logger.Log($"Successfully initialized MS Agent using ProgID: {progId}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to create agent server with ProgID {progId}", ex);
                lastException = ex;
                _agentServer = null;
                return false;
            }
        }
        
        private bool TryCreateAgentServerByCLSID(Guid clsid, ref Exception lastException)
        {
            try
            {
                Logger.Log($"Trying to create agent server with CLSID: {clsid}");
                
                Type agentType = Type.GetTypeFromCLSID(clsid, false);
                if (agentType == null)
                {
                    Logger.Log($"CLSID {clsid} not found");
                    return false;
                }
                
                _agentServer = Activator.CreateInstance(agentType);
                if (_agentServer == null)
                {
                    Logger.Log($"Failed to create instance for CLSID {clsid}");
                    return false;
                }
                
                // Try to set Connected = true using InvokeMember (avoids type library requirement)
                try
                {
                    agentType.InvokeMember("Connected", 
                        System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
                        null, _agentServer, new object[] { true });
                    Logger.Log($"Set Connected = true for CLSID {clsid}");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Connected property not available or failed: {ex.Message}");
                    // Connected property may not exist on AgentServer - continue anyway
                }
                
                Logger.Log($"Successfully initialized MS Agent using CLSID: {clsid}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to create agent server with CLSID {clsid}", ex);
                lastException = ex;
                _agentServer = null;
                return false;
            }
        }
        
        private string GetMSAgentDiagnostics()
        {
            var diagnostics = new List<string>();
            
            Logger.Log("Running MS Agent diagnostics...");
            
            // Check for Agent Server registration
            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey(@"CLSID\{D45FD31B-5C6E-11D1-9EC1-00C04FD7081F}"))
                {
                    var msg = key != null ? "✓ AgentServer CLSID registered" : "✗ AgentServer CLSID NOT registered";
                    diagnostics.Add(msg);
                    Logger.LogDiagnostic("Registry", msg);
                }
            }
            catch (Exception ex)
            {
                diagnostics.Add("✗ Cannot check AgentServer CLSID");
                Logger.LogError("Cannot check AgentServer CLSID", ex);
            }
            
            // Check for Agent Control registration
            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey(@"CLSID\{D45FD31D-5C6E-11D1-9EC1-00C04FD7081F}"))
                {
                    var msg = key != null ? "✓ Agent.Control CLSID registered" : "✗ Agent.Control CLSID NOT registered";
                    diagnostics.Add(msg);
                    Logger.LogDiagnostic("Registry", msg);
                }
            }
            catch (Exception ex)
            {
                diagnostics.Add("✗ Cannot check Agent.Control CLSID");
                Logger.LogError("Cannot check Agent.Control CLSID", ex);
            }
            
            // Check for AgentServer.Agent ProgID
            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey(@"AgentServer.Agent"))
                {
                    var msg = key != null ? "✓ AgentServer.Agent ProgID registered" : "✗ AgentServer.Agent ProgID NOT registered";
                    diagnostics.Add(msg);
                    Logger.LogDiagnostic("Registry", msg);
                }
            }
            catch (Exception ex)
            {
                diagnostics.Add("✗ Cannot check AgentServer.Agent ProgID");
                Logger.LogError("Cannot check AgentServer.Agent ProgID", ex);
            }
            
            // Check for Agent.Control.2 ProgID
            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey(@"Agent.Control.2"))
                {
                    var msg = key != null ? "✓ Agent.Control.2 ProgID registered" : "✗ Agent.Control.2 ProgID NOT registered";
                    diagnostics.Add(msg);
                    Logger.LogDiagnostic("Registry", msg);
                }
            }
            catch (Exception ex)
            {
                diagnostics.Add("✗ Cannot check Agent.Control.2 ProgID");
                Logger.LogError("Cannot check Agent.Control.2 ProgID", ex);
            }
            
            // Check for Type Library registration (TYPE_E_LIBNOTREGISTERED fix)
            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey(@"TypeLib\{A7B93C73-7B81-11D0-AC5F-00C04FD97575}"))
                {
                    var msg = key != null ? "✓ MS Agent TypeLib registered" : "✗ MS Agent TypeLib NOT registered (causes TYPE_E_LIBNOTREGISTERED)";
                    diagnostics.Add(msg);
                    Logger.LogDiagnostic("Registry", msg);
                }
            }
            catch (Exception ex)
            {
                diagnostics.Add("✗ Cannot check MS Agent TypeLib");
                Logger.LogError("Cannot check MS Agent TypeLib", ex);
            }
            
            // Check for MS Agent DLLs
            string sysDir = Environment.SystemDirectory;
            string agentDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "msagent");
            
            string[] filesToCheck = new[]
            {
                Path.Combine(sysDir, "agentsvr.exe"),
                Path.Combine(sysDir, "agentctl.dll"),
                Path.Combine(agentDir, "agentsvr.exe"),
                Path.Combine(agentDir, "agentctl.dll"),
                Path.Combine(agentDir, "agentdpv.dll"),
                Path.Combine(agentDir, "agtctl15.tlb") // Type library file
            };
            
            foreach (var file in filesToCheck)
            {
                var msg = File.Exists(file) ? $"✓ {file} exists" : $"✗ {file} NOT found";
                diagnostics.Add(msg);
                Logger.LogDiagnostic("Files", msg);
            }
            
            // Check for character files
            string charsDir = Path.Combine(agentDir, "chars");
            if (Directory.Exists(charsDir))
            {
                var charFiles = Directory.GetFiles(charsDir, "*.acs");
                var msg = $"✓ Character directory exists with {charFiles.Length} character(s)";
                diagnostics.Add(msg);
                Logger.LogDiagnostic("Files", msg);
            }
            else
            {
                diagnostics.Add("✗ Character directory NOT found");
                Logger.LogDiagnostic("Files", "Character directory NOT found");
            }
            
            // Add fix instructions for TYPE_E_LIBNOTREGISTERED
            diagnostics.Add("");
            diagnostics.Add("=== Fix for TYPE_E_LIBNOTREGISTERED ===");
            diagnostics.Add("Run these commands as Administrator:");
            diagnostics.Add($"  regsvr32 \"{Path.Combine(agentDir, "agentsvr.exe")}\"");
            diagnostics.Add($"  regsvr32 \"{Path.Combine(agentDir, "agentctl.dll")}\"");
            diagnostics.Add("");
            diagnostics.Add("If the above fails, try re-installing MS Agent or use DoubleAgent.");
            
            var result = string.Join("\n", diagnostics);
            Logger.Log("Diagnostics complete. Results:\n" + result);
            
            return result;
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
            Logger.Log($"Loading character from: {characterPath}");
            
            if (_agentServer == null)
            {
                Logger.LogError("Agent server not initialized");
                throw new AgentException("Agent server not initialized.");
            }

            Exception firstException = null;
            Exception secondException = null;
            Exception thirdException = null;
            Exception fourthException = null;
            
            Type serverType = _agentServer.GetType();
            
            try
            {
                // Unload any existing character
                if (_isLoaded && _characterId != 0)
                {
                    UnloadCharacter();
                }

                // Method 1: Try using InvokeMember to call Load (avoids type library requirement)
                try
                {
                    Logger.Log("Trying Method 1: InvokeMember Load");
                    object[] loadArgs = new object[] { characterPath, 0, 0 };
                    serverType.InvokeMember("Load",
                        System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
                        null, _agentServer, loadArgs);
                    
                    // Get the character ID from the output parameters
                    _characterId = Convert.ToInt32(loadArgs[1]);
                    
                    // Get the character object
                    _character = serverType.InvokeMember("Character",
                        System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
                        null, _agentServer, new object[] { _characterId });
                    
                    _isLoaded = true;
                    Logger.Log($"SUCCESS: Character loaded using InvokeMember Load");
                    return;
                }
                catch (Exception ex)
                {
                    firstException = ex;
                    Logger.LogError("Method 1 (InvokeMember Load) failed", ex);
                }

                // Method 2: Try AgentServer.Load with dynamic (IAgentEx interface)
                try
                {
                    Logger.Log("Trying Method 2: dynamic AgentServer.Load");
                    object charId = null;
                    object requestId = null;
                    _agentServer.Load(characterPath, ref charId, ref requestId);
                    _characterId = Convert.ToInt32(charId);
                    _character = _agentServer.Character(_characterId);
                    _isLoaded = true;
                    Logger.Log($"SUCCESS: Character loaded using dynamic AgentServer.Load");
                    return;
                }
                catch (Exception ex)
                {
                    secondException = ex;
                    Logger.LogError("Method 2 (dynamic AgentServer.Load) failed", ex);
                }

                // Method 3: Try Characters collection using InvokeMember
                try
                {
                    Logger.Log("Trying Method 3: InvokeMember Characters.Load");
                    string charName = Path.GetFileNameWithoutExtension(characterPath);
                    
                    // Get Characters collection
                    object characters = serverType.InvokeMember("Characters",
                        System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
                        null, _agentServer, null);
                    
                    if (characters != null)
                    {
                        Type charsType = characters.GetType();
                        
                        // Call Load on Characters collection
                        charsType.InvokeMember("Load",
                            System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
                            null, characters, new object[] { charName, characterPath });
                        
                        // Get the character by name
                        _character = charsType.InvokeMember("Character",
                            System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
                            null, characters, new object[] { charName });
                        
                        _characterId = charName.GetHashCode();
                        _isLoaded = true;
                        Logger.Log($"SUCCESS: Character loaded using InvokeMember Characters.Load");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    thirdException = ex;
                    Logger.LogError("Method 3 (InvokeMember Characters.Load) failed", ex);
                }
                
                // Method 4: Try Agent.Control style with dynamic (Characters collection)
                try
                {
                    Logger.Log("Trying Method 4: dynamic Characters.Load");
                    string charName = Path.GetFileNameWithoutExtension(characterPath);
                    dynamic characters = _agentServer.Characters;
                    characters.Load(charName, characterPath);
                    _character = characters[charName];
                    _characterId = charName.GetHashCode();
                    _isLoaded = true;
                    Logger.Log($"SUCCESS: Character loaded using dynamic Characters.Load");
                    return;
                }
                catch (Exception ex)
                {
                    fourthException = ex;
                    Logger.LogError("Method 4 (dynamic Characters.Load) failed", ex);
                }

                // All methods failed
                string errorMsg = $"Failed to load character from '{characterPath}'.\n\n";
                if (firstException != null)
                    errorMsg += $"Method 1 (InvokeMember Load): {firstException.Message}\n";
                if (secondException != null)
                    errorMsg += $"Method 2 (dynamic Load): {secondException.Message}\n";
                if (thirdException != null)
                    errorMsg += $"Method 3 (InvokeMember Characters): {thirdException.Message}\n";
                if (fourthException != null)
                    errorMsg += $"Method 4 (dynamic Characters): {fourthException.Message}\n";
                
                errorMsg += $"\nSee log file for details: {Logger.LogFilePath}";
                
                Logger.LogError("All character loading methods failed");
                throw new AgentException(errorMsg);
            }
            catch (AgentException)
            {
                throw;
            }
            catch (COMException ex)
            {
                Logger.LogError($"COM error loading character", ex);
                throw new AgentException($"COM error loading character from '{characterPath}': 0x{ex.ErrorCode:X8} - {ex.Message}\n\nSee log: {Logger.LogFilePath}", ex);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Unexpected error loading character", ex);
                throw new AgentException($"Unexpected error loading character from '{characterPath}': {ex.Message}\n\nSee log: {Logger.LogFilePath}", ex);
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
