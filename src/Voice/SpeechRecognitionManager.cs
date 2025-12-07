using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Threading;
using System.Threading.Tasks;
using MSAgentAI.Logging;

namespace MSAgentAI.Voice
{
    /// <summary>
    /// Manages speech recognition for voice input
    /// Uses System.Speech.Recognition (built into .NET Framework)
    /// </summary>
    public class SpeechRecognitionManager : IDisposable
    {
        private SpeechRecognitionEngine _recognizer;
        private bool _disposed;
        private bool _isListening;
        private DateTime _lastSpeechTime;
        private string _currentUtterance;
        private Timer _silenceTimer;
        private int _silenceThresholdMs = 1500; // Default 1.5 seconds
        private double _minConfidenceThreshold = 0.2; // Default 0.2
        private bool _speechInProgress;
        private int _audioLevel;

        public event EventHandler<string> OnSpeechRecognized;
        public event EventHandler OnListeningStarted;
        public event EventHandler OnListeningStopped;
        public event EventHandler<int> OnAudioLevelChanged;

        public bool IsListening => _isListening;
        public int AudioLevel => _audioLevel;
        
        // Settings properties
        public int SilenceThresholdMs 
        { 
            get => _silenceThresholdMs; 
            set => _silenceThresholdMs = Math.Max(500, Math.Min(5000, value)); 
        }
        
        public double MinConfidenceThreshold 
        { 
            get => _minConfidenceThreshold; 
            set => _minConfidenceThreshold = Math.Max(0.0, Math.Min(1.0, value)); 
        }

        public SpeechRecognitionManager()
        {
            InitializeRecognizer();
        }
        
        /// <summary>
        /// Gets available audio input devices (microphones)
        /// </summary>
        public static List<string> GetAvailableMicrophones()
        {
            var mics = new List<string>();
            mics.Add("(Default Device)");
            
            try
            {
                // Get all recognizer info to find audio inputs
                foreach (var recognizerInfo in SpeechRecognitionEngine.InstalledRecognizers())
                {
                    // System.Speech doesn't expose individual microphones easily
                    // But we can add recognizer cultures as options
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to enumerate microphones", ex);
            }
            
            return mics;
        }

        private void InitializeRecognizer()
        {
            try
            {
                Logger.Log("Initializing speech recognition with improved detection...");
                
                // Create a speech recognition engine using the default recognizer
                _recognizer = new SpeechRecognitionEngine();

                // Configure for better accuracy
                _recognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(0); // No initial timeout
                _recognizer.BabbleTimeout = TimeSpan.FromSeconds(0); // No babble timeout
                _recognizer.EndSilenceTimeout = TimeSpan.FromSeconds(0.5); // Short end silence
                _recognizer.EndSilenceTimeoutAmbiguous = TimeSpan.FromSeconds(0.75);

                // Create a dictation grammar for free-form speech
                var dictationGrammar = new DictationGrammar();
                dictationGrammar.Name = "Dictation Grammar";
                _recognizer.LoadGrammar(dictationGrammar);

                // Wire up events
                _recognizer.SpeechRecognized += OnSpeechRecognizedInternal;
                _recognizer.SpeechHypothesized += OnSpeechHypothesized;
                _recognizer.SpeechDetected += OnSpeechDetected;
                _recognizer.RecognizeCompleted += OnRecognizeCompleted;
                _recognizer.AudioLevelUpdated += OnAudioLevelUpdated;
                _recognizer.AudioStateChanged += OnAudioStateChanged;

                // Set input to default microphone
                _recognizer.SetInputToDefaultAudioDevice();

                Logger.Log("Speech recognition initialized with improved detection settings");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to initialize speech recognition", ex);
            }
        }

        private void OnAudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
        {
            _audioLevel = e.AudioLevel;
            OnAudioLevelChanged?.Invoke(this, e.AudioLevel);
            
            // If audio level is significant, mark as speech in progress
            if (e.AudioLevel > 10)
            {
                _lastSpeechTime = DateTime.Now;
                _speechInProgress = true;
            }
        }

        private void OnAudioStateChanged(object sender, AudioStateChangedEventArgs e)
        {
            Logger.Log($"Audio state changed: {e.AudioState}");
            if (e.AudioState == AudioState.Speech)
            {
                _speechInProgress = true;
                _lastSpeechTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Start listening for speech input
        /// </summary>
        public void StartListening()
        {
            if (_recognizer == null) 
            {
                Logger.Log("Cannot start listening - recognizer is null, reinitializing...");
                InitializeRecognizer();
                if (_recognizer == null)
                {
                    Logger.LogError("Failed to reinitialize recognizer", null);
                    return;
                }
            }
            
            if (_isListening) 
            {
                Logger.Log("Already listening, ignoring StartListening call");
                return;
            }

            try
            {
                Logger.Log("Starting speech recognition...");
                _currentUtterance = "";
                _lastSpeechTime = DateTime.Now;
                _speechInProgress = false;
                
                // Stop any previous timer
                _silenceTimer?.Dispose();
                _silenceTimer = null;

                // Mark as listening BEFORE starting recognition
                _isListening = true;

                // Start continuous recognition
                _recognizer.RecognizeAsync(RecognizeMode.Multiple);

                // Start silence detection timer (check more frequently)
                _silenceTimer = new Timer(CheckSilence, null, 250, 250);

                OnListeningStarted?.Invoke(this, EventArgs.Empty);
                Logger.Log("Speech recognition started - listening for input");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to start speech recognition", ex);
                _isListening = false;
            }
        }

        /// <summary>
        /// Stop listening for speech input
        /// </summary>
        public void StopListening()
        {
            Logger.Log($"StopListening called, _isListening={_isListening}");
            
            // Stop the timer first
            _silenceTimer?.Dispose();
            _silenceTimer = null;
            
            // If we have accumulated speech, process it before stopping
            if (!string.IsNullOrWhiteSpace(_currentUtterance))
            {
                var finalText = _currentUtterance.Trim();
                _currentUtterance = "";
                Logger.Log($"Processing accumulated speech before stopping: \"{finalText}\"");
                OnSpeechRecognized?.Invoke(this, finalText);
            }
            
            // Mark as not listening BEFORE stopping the recognizer
            _isListening = false;
            
            if (_recognizer != null)
            {
                try
                {
                    _recognizer.RecognizeAsyncStop();
                    Logger.Log("Speech recognition stopped");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error stopping speech recognition", ex);
                }
            }

            OnListeningStopped?.Invoke(this, EventArgs.Empty);
        }

        private void OnSpeechRecognizedInternal(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result != null && e.Result.Confidence >= _minConfidenceThreshold)
            {
                _lastSpeechTime = DateTime.Now;
                _speechInProgress = true;
                _currentUtterance += " " + e.Result.Text;
                Logger.Log($"Speech recognized: \"{e.Result.Text}\" (confidence: {e.Result.Confidence:F2})");
            }
            else if (e.Result != null)
            {
                Logger.Log($"Low confidence speech ignored: \"{e.Result.Text}\" (confidence: {e.Result.Confidence:F2}, threshold: {_minConfidenceThreshold:F2})");
            }
        }

        private void OnSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            _lastSpeechTime = DateTime.Now;
            _speechInProgress = true;
            // Log hypothesized speech for debugging
            if (e.Result != null && e.Result.Confidence >= 0.1)
            {
                Logger.Log($"Speech hypothesized: \"{e.Result.Text}\" (confidence: {e.Result.Confidence:F2})");
            }
        }

        private void OnSpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            _lastSpeechTime = DateTime.Now;
        }

        private void OnRecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Logger.LogError("Recognition error", e.Error);
            }
        }

        private void CheckSilence(object state)
        {
            if (!_isListening) return;

            var silenceTime = (DateTime.Now - _lastSpeechTime).TotalMilliseconds;
            
            // Only trigger if we had speech in progress and now have silence
            if (_speechInProgress && silenceTime >= _silenceThresholdMs && !string.IsNullOrWhiteSpace(_currentUtterance))
            {
                // Silence detected after speech - process the utterance
                var finalText = _currentUtterance.Trim();
                _currentUtterance = "";
                _speechInProgress = false;
                
                Logger.Log($"Silence detected ({_silenceThresholdMs}ms) after speech: \"{finalText}\"");
                
                // Fire the event
                OnSpeechRecognized?.Invoke(this, finalText);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            StopListening();
            _silenceTimer?.Dispose();
            _recognizer?.Dispose();
        }
    }
}
