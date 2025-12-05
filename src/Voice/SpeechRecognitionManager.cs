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
        private const int SilenceThresholdMs = 2000; // 2 seconds of silence

        public event EventHandler<string> OnSpeechRecognized;
        public event EventHandler OnListeningStarted;
        public event EventHandler OnListeningStopped;

        public bool IsListening => _isListening;

        public SpeechRecognitionManager()
        {
            InitializeRecognizer();
        }

        private void InitializeRecognizer()
        {
            try
            {
                Logger.Log("Initializing speech recognition...");
                
                // Create a speech recognition engine using the default recognizer
                _recognizer = new SpeechRecognitionEngine();

                // Create a dictation grammar for free-form speech
                var dictationGrammar = new DictationGrammar();
                dictationGrammar.Name = "Dictation Grammar";
                _recognizer.LoadGrammar(dictationGrammar);

                // Wire up events
                _recognizer.SpeechRecognized += OnSpeechRecognizedInternal;
                _recognizer.SpeechHypothesized += OnSpeechHypothesized;
                _recognizer.SpeechDetected += OnSpeechDetected;
                _recognizer.RecognizeCompleted += OnRecognizeCompleted;

                // Set input to default microphone
                _recognizer.SetInputToDefaultAudioDevice();

                Logger.Log("Speech recognition initialized successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to initialize speech recognition", ex);
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
                Logger.Log("Starting speech recognition...");
                _currentUtterance = "";
                _lastSpeechTime = DateTime.Now;
                _isListening = true;

                // Start continuous recognition
                _recognizer.RecognizeAsync(RecognizeMode.Multiple);

                // Start silence detection timer
                _silenceTimer = new Timer(CheckSilence, null, 500, 500);

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
            if (e.Result != null && e.Result.Confidence > 0.3)
            {
                _lastSpeechTime = DateTime.Now;
                _currentUtterance += " " + e.Result.Text;
                Logger.Log($"Speech recognized: {e.Result.Text} (confidence: {e.Result.Confidence:F2})");
            }
        }

        private void OnSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            _lastSpeechTime = DateTime.Now;
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
            
            if (silenceTime >= SilenceThresholdMs && !string.IsNullOrWhiteSpace(_currentUtterance))
            {
                // 2 seconds of silence detected with accumulated speech
                var finalText = _currentUtterance.Trim();
                _currentUtterance = "";
                
                Logger.Log($"Silence detected after speech: \"{finalText}\"");
                
                // Fire the event on UI thread
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
