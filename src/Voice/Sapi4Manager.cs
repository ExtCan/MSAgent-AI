using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace MSAgentAI.Voice
{
    /// <summary>
    /// Manages SAPI4 Text-to-Speech functionality
    /// </summary>
    public class Sapi4Manager : IDisposable
    {
        private dynamic _voiceEngine;
        private bool _disposed;

        // SAPI4 TTSModeInfo registry location
        private const string SAPI4_VOICES_KEY = @"SOFTWARE\Microsoft\Speech\Voices\TokenEnums\Sapi4";
        private const string SAPI4_TTS_KEY = @"SOFTWARE\Microsoft\Speech\TTSEnumKey";

        public int Speed { get; set; } = 150; // 75-250 typical range
        public int Pitch { get; set; } = 100; // 50-400 typical range
        public int Volume { get; set; } = 65535; // 0-65535

        public string CurrentVoiceId { get; private set; }

        public Sapi4Manager()
        {
            InitializeVoiceEngine();
        }

        private void InitializeVoiceEngine()
        {
            try
            {
                // Try to create SAPI4 TTS engine
                Type ttsType = Type.GetTypeFromProgID("Speech.VoiceText");
                if (ttsType != null)
                {
                    _voiceEngine = Activator.CreateInstance(ttsType);
                    // Register the voice engine
                    _voiceEngine.Register(IntPtr.Zero, "MSAgentAI");
                }
            }
            catch (COMException ex)
            {
                throw new VoiceException("Failed to initialize SAPI4 voice engine. Ensure SAPI4 is installed.", ex);
            }
        }

        /// <summary>
        /// Gets available SAPI4 voices from the registry
        /// </summary>
        public List<VoiceInfo> GetAvailableVoices()
        {
            var voices = new List<VoiceInfo>();

            try
            {
                // Try SAPI4 specific registry location
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Speech\Voices\Tokens"))
                {
                    if (key != null)
                    {
                        foreach (var voiceName in key.GetSubKeyNames())
                        {
                            try
                            {
                                using (var voiceKey = key.OpenSubKey(voiceName))
                                {
                                    if (voiceKey != null)
                                    {
                                        var displayName = voiceKey.GetValue("")?.ToString() ?? voiceName;
                                        var clsid = voiceKey.GetValue("CLSID")?.ToString();
                                        
                                        voices.Add(new VoiceInfo
                                        {
                                            Id = voiceName,
                                            Name = displayName,
                                            Clsid = clsid
                                        });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error reading voice key: {ex.Message}");
                            }
                        }
                    }
                }

                // Also try legacy SAPI4 location
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Speech\Voice Enumeration"))
                {
                    if (key != null)
                    {
                        foreach (var voiceName in key.GetSubKeyNames())
                        {
                            try
                            {
                                using (var voiceKey = key.OpenSubKey(voiceName))
                                {
                                    if (voiceKey != null)
                                    {
                                        var displayName = voiceKey.GetValue("VoiceName")?.ToString() ?? voiceName;
                                        var modeId = voiceKey.GetValue("ModeID")?.ToString();

                                        if (!voices.Exists(v => v.Name == displayName))
                                        {
                                            voices.Add(new VoiceInfo
                                            {
                                                Id = modeId ?? voiceName,
                                                Name = displayName,
                                                Clsid = modeId
                                            });
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error reading legacy voice key: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enumerating voices: {ex.Message}");
            }

            // Add default/fallback voices if none found
            if (voices.Count == 0)
            {
                voices.Add(new VoiceInfo { Id = "default", Name = "Default System Voice", Clsid = null });
            }

            return voices;
        }

        /// <summary>
        /// Sets the current voice by ID
        /// </summary>
        public void SetVoice(string voiceId)
        {
            CurrentVoiceId = voiceId;

            if (_voiceEngine != null)
            {
                try
                {
                    // For SAPI4, we typically need to set the voice through the ModeID
                    // This is handled when speaking through the MS Agent
                }
                catch (Exception ex)
                {
                    throw new VoiceException($"Failed to set voice '{voiceId}'.", ex);
                }
            }
        }

        /// <summary>
        /// Speaks the specified text using SAPI4
        /// </summary>
        public void Speak(string text)
        {
            if (_voiceEngine != null && !string.IsNullOrEmpty(text))
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
                    throw new VoiceException($"Failed to speak text.", ex);
                }
            }
        }

        /// <summary>
        /// Stops any current speech
        /// </summary>
        public void Stop()
        {
            if (_voiceEngine != null)
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
            return CurrentVoiceId;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_voiceEngine != null)
                {
                    try
                    {
                        _voiceEngine.UnRegister();
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

        public override string ToString() => Name;
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
