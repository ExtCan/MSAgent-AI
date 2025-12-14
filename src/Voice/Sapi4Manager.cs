using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using MSAgentAI.Logging;

namespace MSAgentAI.Voice
{
    /// <summary>
    /// Manages SAPI4 Text-to-Speech functionality
    /// </summary>
    public class Sapi4Manager : IDisposable
    {
        private dynamic _voiceEngine;
        private bool _disposed;
        private bool _initialized;

        public int Speed { get; set; } = 150; // 75-250 typical range
        public int Pitch { get; set; } = 100; // 50-400 typical range
        public int Volume { get; set; } = 65535; // 0-65535

        public string CurrentVoiceId { get; private set; }
        public string CurrentVoiceModeId { get; private set; }

        public Sapi4Manager()
        {
            InitializeVoiceEngine();
        }

        private void InitializeVoiceEngine()
        {
            try
            {
                Logger.Log("Initializing SAPI4 voice engine...");
                
                // Try to create SAPI4 DirectSpeechSynth
                Type ttsType = Type.GetTypeFromProgID("Speech.VoiceText");
                if (ttsType != null)
                {
                    _voiceEngine = Activator.CreateInstance(ttsType);
                    // Register the voice engine
                    try
                    {
                        _voiceEngine.Register(IntPtr.Zero, "MSAgentAI");
                        _initialized = true;
                        Logger.Log("SAPI4 voice engine initialized successfully");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Failed to register SAPI4 voice engine", ex);
                    }
                }
                else
                {
                    Logger.Log("Speech.VoiceText ProgID not found, SAPI4 may not be installed");
                }
            }
            catch (COMException ex)
            {
                Logger.LogError("Failed to initialize SAPI4 voice engine", ex);
            }
        }

        /// <summary>
        /// Gets available SAPI4 TTS Modes from the registry (the way MS Agent/CyberBuddy does it)
        /// Looks in HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech\Voices\TTSMode
        /// Each voice has a ModeID GUID that is used to set the TTS mode in MS Agent
        /// </summary>
        public List<VoiceInfo> GetAvailableVoices()
        {
            var voices = new List<VoiceInfo>();

            Logger.Log("Enumerating SAPI4 TTS Modes...");

            try
            {
                // Primary location: TTS Modes in Speech registry (CyberBuddy approach)
                // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech\Voices\TTSMode
                EnumerateTTSModes(voices);

                // NOTE: EnumerateVoiceTokens is removed - only show SAPI4 voices, not SAPI5
            }
            catch (Exception ex)
            {
                Logger.LogError("Error enumerating SAPI4 voices", ex);
            }

            Logger.Log($"Found {voices.Count} SAPI4 TTS modes");

            // Add default if none found
            if (voices.Count == 0)
            {
                voices.Add(new VoiceInfo 
                { 
                    Id = "default", 
                    Name = "Default System Voice", 
                    ModeId = null,
                    IsSapi4 = true
                });
            }

            return voices;
        }

        /// <summary>
        /// Enumerate TTS Modes from the registry - this is the proper SAPI4 way
        /// </summary>
        private void EnumerateTTSModes(List<VoiceInfo> voices)
        {
            // Check both 32-bit and 64-bit registry locations
            string[] registryPaths = new[]
            {
                @"SOFTWARE\Microsoft\Speech\Voices\TTSMode",
                @"SOFTWARE\WOW6432Node\Microsoft\Speech\Voices\TTSMode",
                @"SOFTWARE\Microsoft\Speech\Voices\Tokens",
                @"SOFTWARE\WOW6432Node\Microsoft\Speech\Voices\Tokens"
            };

            foreach (var basePath in registryPaths)
            {
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(basePath))
                    {
                        if (key == null) continue;

                        foreach (var modeName in key.GetSubKeyNames())
                        {
                            try
                            {
                                using (var modeKey = key.OpenSubKey(modeName))
                                {
                                    if (modeKey == null) continue;

                                    // Get ModeID (GUID) - this is what MS Agent needs
                                    string modeId = modeName;
                                    
                                    // Try to get ModeID from subkey value if it exists
                                    var modeIdValue = modeKey.GetValue("ModeID");
                                    if (modeIdValue != null)
                                    {
                                        modeId = modeIdValue.ToString();
                                    }

                                    // Get display name from various possible locations
                                    string displayName = modeKey.GetValue("")?.ToString();
                                    if (string.IsNullOrEmpty(displayName))
                                    {
                                        displayName = modeKey.GetValue("VoiceName")?.ToString();
                                    }
                                    if (string.IsNullOrEmpty(displayName))
                                    {
                                        // Check Attributes subkey
                                        using (var attrKey = modeKey.OpenSubKey("Attributes"))
                                        {
                                            if (attrKey != null)
                                            {
                                                displayName = attrKey.GetValue("Name")?.ToString();
                                            }
                                        }
                                    }
                                    if (string.IsNullOrEmpty(displayName))
                                    {
                                        displayName = modeName;
                                    }

                                    // Skip duplicates
                                    if (voices.Exists(v => v.ModeId == modeId || v.Name == displayName))
                                        continue;

                                    voices.Add(new VoiceInfo
                                    {
                                        Id = modeId,
                                        Name = displayName,
                                        ModeId = modeId,
                                        IsSapi4 = true
                                    });

                                    Logger.Log($"Found SAPI4 TTS Mode: {displayName} (ModeID: {modeId})");
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Enumerate voice tokens (SAPI5 style, fallback)
        /// </summary>
        private void EnumerateVoiceTokens(List<VoiceInfo> voices)
        {
            try
            {
                // Also try OneCore voices (Windows 10+) as fallback
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens"))
                {
                    if (key == null) return;

                    foreach (var tokenName in key.GetSubKeyNames())
                    {
                        try
                        {
                            using (var tokenKey = key.OpenSubKey(tokenName))
                            {
                                if (tokenKey == null) continue;

                                string displayName = tokenKey.GetValue("")?.ToString() ?? tokenName;
                                
                                // Skip duplicates
                                if (voices.Exists(v => v.Name == displayName))
                                    continue;

                                voices.Add(new VoiceInfo
                                {
                                    Id = tokenName,
                                    Name = displayName + " (SAPI5)",
                                    ModeId = tokenName,
                                    IsSapi4 = false
                                });

                                Logger.Log($"Found SAPI5 voice: {displayName}");
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Sets the current voice by ID or ModeID
        /// </summary>
        public void SetVoice(string voiceId)
        {
            CurrentVoiceId = voiceId;
            Logger.Log($"Setting voice to: {voiceId}");

            // Find the voice info to get the ModeID
            var voices = GetAvailableVoices();
            var voice = voices.Find(v => v.Id == voiceId || v.ModeId == voiceId);
            if (voice != null)
            {
                CurrentVoiceModeId = voice.ModeId;
                Logger.Log($"Voice ModeID set to: {CurrentVoiceModeId}");
            }
        }

        /// <summary>
        /// Speaks the specified text using SAPI4
        /// </summary>
        public void Speak(string text)
        {
            if (_voiceEngine != null && !string.IsNullOrEmpty(text) && _initialized)
            {
                try
                {
                    // Set speed
                    _voiceEngine.Speed = Speed;

                    // Speak the text
                    _voiceEngine.Speak(text, 1); // 1 = SVSFDefault
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to speak text via SAPI4", ex);
                }
            }
        }

        /// <summary>
        /// Stops any current speech
        /// </summary>
        public void Stop()
        {
            if (_voiceEngine != null && _initialized)
            {
                try
                {
                    _voiceEngine.StopSpeaking();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error stopping speech: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Gets the TTS Mode ID for use with MS Agent
        /// </summary>
        public string GetTTSModeId()
        {
            return CurrentVoiceModeId ?? CurrentVoiceId;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_voiceEngine != null)
                {
                    try
                    {
                        if (_initialized)
                        {
                            _voiceEngine.UnRegister();
                        }
                        Marshal.ReleaseComObject(_voiceEngine);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error disposing voice engine: {ex.Message}");
                    }
                    _voiceEngine = null;
                }

                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Information about a SAPI4 voice
    /// </summary>
    public class VoiceInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Clsid { get; set; }
        public string ModeId { get; set; }
        public bool IsSapi4 { get; set; }

        public override string ToString() => $"{Name}{(IsSapi4 ? " (SAPI4)" : "")}";
    }

    /// <summary>
    /// Exception for voice-related errors
    /// </summary>
    public class VoiceException : Exception
    {
        public VoiceException(string message) : base(message) { }
        public VoiceException(string message, Exception inner) : base(message, inner) { }
    }
}
