using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
        /// Gets available SAPI4 voices using proper COM enumeration
        /// </summary>
        public List<VoiceInfo> GetAvailableVoices()
        {
            var voices = new List<VoiceInfo>();

            Logger.Log("Enumerating SAPI4 voices via COM...");

            try
            {
                // Use COM enumeration - the proper SAPI4 way
                EnumerateViaCOM(voices);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error enumerating SAPI4 voices", ex);
            }

            Logger.Log($"Found {voices.Count} SAPI4 voices");

            // Add default if none found
            if (voices.Count == 0)
            {
                Logger.Log("No SAPI4 voices found. Adding default placeholder.");
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
        /// Enumerate SAPI4 voices using COM ITTSEnum interface - the proper way
        /// </summary>
        private void EnumerateViaCOM(List<VoiceInfo> voices)
        {
            ITTSEnum ttsEnum = null;
            
            try
            {
                // Create SAPI4 TTS Enumerator
                Type ttsEnumType = Type.GetTypeFromCLSID(Sapi4Constants.CLSID_TTSEnumerator);
                if (ttsEnumType == null)
                {
                    Logger.Log("SAPI4 TTSEnumerator not found. SAPI4 may not be installed.");
                    return;
                }

                object ttsEnumObj = Activator.CreateInstance(ttsEnumType);
                ttsEnum = (ITTSEnum)ttsEnumObj;

                // Reset to start of enumeration
                ttsEnum.Reset();

                // Enumerate all voices
                TTSMODEINFO modeInfo;
                uint fetched;
                int hr;

                while (true)
                {
                    hr = ttsEnum.Next(1, out modeInfo, out fetched);
                    
                    // S_FALSE (1) means no more items
                    if (hr != 0 || fetched == 0)
                        break;

                    // Add voice to list
                    string modeName = modeInfo.szModeName ?? "";
                    string mfgName = modeInfo.szMfgName ?? "";
                    string productName = modeInfo.szProductName ?? "";
                    
                    // Build display name
                    string displayName = modeName;
                    if (string.IsNullOrEmpty(displayName))
                        displayName = productName;
                    if (string.IsNullOrEmpty(displayName))
                        displayName = modeInfo.gModeID.ToString();

                    // Skip duplicates
                    string modeIdStr = modeInfo.gModeID.ToString("B"); // Format as {GUID}
                    if (voices.Exists(v => v.ModeId == modeIdStr))
                        continue;

                    voices.Add(new VoiceInfo
                    {
                        Id = modeIdStr,
                        Name = displayName,
                        ModeId = modeIdStr,
                        IsSapi4 = true
                    });

                    Logger.Log($"Found SAPI4 voice: {displayName} (ModeID: {modeIdStr})");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in COM enumeration", ex);
            }
            finally
            {
                // Release COM object
                if (ttsEnum != null)
                {
                    try
                    {
                        Marshal.ReleaseComObject(ttsEnum);
                    }
                    catch { }
                }
            }
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
