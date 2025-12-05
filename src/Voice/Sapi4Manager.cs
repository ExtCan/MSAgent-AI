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
        /// Gets available SAPI4 voices from the registry
        /// </summary>
        public List<VoiceInfo> GetAvailableVoices()
        {
            var voices = new List<VoiceInfo>();

            Logger.Log("Enumerating SAPI4 voices...");

            try
            {
                // SAPI4 voices are stored in the TTSEnumerator registry
                // Check multiple locations for SAPI4 voices

                // Location 1: HKLM\SOFTWARE\Microsoft\Speech\Voices (SAPI4 style)
                EnumerateVoicesFromKey(Registry.LocalMachine, @"SOFTWARE\Microsoft\Speech\Voices", voices);

                // Location 2: Legacy SAPI4 TTS Engines
                EnumerateVoicesFromKey(Registry.LocalMachine, @"SOFTWARE\Microsoft\Speech\TTS Engines", voices);

                // Location 3: Check for MS Agent compatible voices
                EnumerateSapi4ModesFromKey(Registry.ClassesRoot, @"CLSID", voices);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error enumerating SAPI4 voices", ex);
            }

            Logger.Log($"Found {voices.Count} SAPI4 voices");

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

        private void EnumerateVoicesFromKey(RegistryKey root, string path, List<VoiceInfo> voices)
        {
            try
            {
                using (var key = root.OpenSubKey(path))
                {
                    if (key == null) return;

                    foreach (var voiceName in key.GetSubKeyNames())
                    {
                        try
                        {
                            using (var voiceKey = key.OpenSubKey(voiceName))
                            {
                                if (voiceKey == null) continue;

                                // Try to get voice info
                                var displayName = voiceKey.GetValue("")?.ToString() 
                                    ?? voiceKey.GetValue("VoiceName")?.ToString() 
                                    ?? voiceName;
                                var modeId = voiceKey.GetValue("ModeID")?.ToString()
                                    ?? voiceKey.GetValue("CLSID")?.ToString();

                                // Skip if already added
                                if (voices.Exists(v => v.Name == displayName || v.ModeId == modeId))
                                    continue;

                                voices.Add(new VoiceInfo
                                {
                                    Id = voiceName,
                                    Name = displayName,
                                    ModeId = modeId,
                                    IsSapi4 = true
                                });

                                Logger.Log($"Found SAPI4 voice: {displayName} (ModeID: {modeId})");
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private void EnumerateSapi4ModesFromKey(RegistryKey root, string path, List<VoiceInfo> voices)
        {
            // Look for TTS Mode IDs which are GUIDs for SAPI4 voices
            try
            {
                // Check common SAPI4 voice CLSIDs
                string[] knownSapi4Clsids = new[]
                {
                    "{99EE9160-AFC2-11D1-A8DC-00A0C90894F3}", // MS Sam
                    "{99EE9162-AFC2-11D1-A8DC-00A0C90894F3}", // MS Mary
                    "{99EE9166-AFC2-11D1-A8DC-00A0C90894F3}", // MS Mike
                };

                foreach (var clsid in knownSapi4Clsids)
                {
                    try
                    {
                        using (var key = root.OpenSubKey($@"CLSID\{clsid}"))
                        {
                            if (key != null)
                            {
                                var name = key.GetValue("")?.ToString() ?? "Unknown Voice";
                                if (!voices.Exists(v => v.ModeId == clsid))
                                {
                                    voices.Add(new VoiceInfo
                                    {
                                        Id = clsid,
                                        Name = name,
                                        ModeId = clsid,
                                        IsSapi4 = true
                                    });
                                }
                            }
                        }
                    }
                    catch { }
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
