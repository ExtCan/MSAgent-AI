using System;
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
        private const int SilenceThresholdMs = 1500; // 1.5 seconds of silence (reduced for better responsiveness)
        private const double MinConfidenceThreshold = 0.2; // Lower threshold for better detection
        private bool _speechInProgress;
        private int _audioLevel;

        public event EventHandler<string> OnSpeechRecognized;
        public event EventHandler OnListeningStarted;
        public event EventHandler OnListeningStopped;
        public event EventHandler<int> OnAudioLevelChanged;

        public bool IsListening => _isListening;
        public int AudioLevel => _audioLevel;

        public SpeechRecognitionManager()
        {
            InitializeRecognizer();
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
            if (_recognizer == null || _isListening) return;

            try
            {
                Logger.Log("Starting speech recognition with improved detection...");
                _currentUtterance = "";
                _lastSpeechTime = DateTime.Now;
                _isListening = true;
                _speechInProgress = false;

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
            if (_recognizer == null || !_isListening) return;

            try
            {
                Logger.Log("Stopping speech recognition...");
                _isListening = false;

                _silenceTimer?.Dispose();
                _silenceTimer = null;

                _recognizer.RecognizeAsyncStop();

                // If we have accumulated speech, process it
                if (!string.IsNullOrWhiteSpace(_currentUtterance))
                {
                    var finalText = _currentUtterance.Trim();
                    _currentUtterance = "";
                    OnSpeechRecognized?.Invoke(this, finalText);
                }

                OnListeningStopped?.Invoke(this, EventArgs.Empty);
                Logger.Log("Speech recognition stopped");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to stop speech recognition", ex);
            }
        }

        private void OnSpeechRecognizedInternal(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result != null && e.Result.Confidence >= MinConfidenceThreshold)
            {
                _lastSpeechTime = DateTime.Now;
                _speechInProgress = true;
                _currentUtterance += " " + e.Result.Text;
                Logger.Log($"Speech recognized: \"{e.Result.Text}\" (confidence: {e.Result.Confidence:F2})");
            }
            else if (e.Result != null)
            {
                Logger.Log($"Low confidence speech ignored: \"{e.Result.Text}\" (confidence: {e.Result.Confidence:F2})");
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
            if (_speechInProgress && silenceTime >= SilenceThresholdMs && !string.IsNullOrWhiteSpace(_currentUtterance))
            {
                // Silence detected after speech - process the utterance
                var finalText = _currentUtterance.Trim();
                _currentUtterance = "";
                _speechInProgress = false;
                
                Logger.Log($"Silence detected (1.5s) after speech: \"{finalText}\"");
                
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
