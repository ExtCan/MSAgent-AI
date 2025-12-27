using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;

namespace MSAgentAI.Config
{
    /// <summary>
    /// Application settings and custom lines configuration
    /// </summary>
    public class AppSettings
    {
        // Agent settings
        public string CharacterPath { get; set; } = @"C:\Windows\msagent\chars";
        public string SelectedCharacterFile { get; set; } = "";

        // User name system (## placeholder)
        public string UserName { get; set; } = "Friend";
        public string UserNamePronunciation { get; set; } = "Friend";
        
        // Voice settings
        public string SelectedVoiceId { get; set; } = "";
        public int VoiceSpeed { get; set; } = 150;
        public int VoicePitch { get; set; } = 100;
        public int VoiceVolume { get; set; } = 65535;
        
        // Speech recognition (Call Mode) settings
        public string SelectedMicrophone { get; set; } = ""; // Empty = default
        public int SpeechConfidenceThreshold { get; set; } = 20; // 0-100 (scaled to 0.0-1.0)
        public int SilenceDetectionMs { get; set; } = 1500; // Milliseconds of silence before AI responds

        // UI Theme
        public string UITheme { get; set; } = "Default";

        // Ollama AI settings
        public string OllamaUrl { get; set; } = "http://localhost:11434";
        public string OllamaModel { get; set; } = "llama2";
        public string OllamaApiKey { get; set; } = "";
        public string PersonalityPrompt { get; set; } = "You are a helpful and friendly desktop companion. Keep responses short and conversational.";
        public bool EnableOllamaChat { get; set; } = false;
        public bool EnableWebSearch { get; set; } = false;
        public bool EnableUrlReading { get; set; } = false;

        // Pipeline settings
        public string PipelineProtocol { get; set; } = "NamedPipe"; // "NamedPipe" or "TCP"
        public string PipelineIPAddress { get; set; } = "127.0.0.1"; // For TCP mode
        public int PipelinePort { get; set; } = 8765; // For TCP mode
        public string PipelineName { get; set; } = "MSAgentAI"; // For Named Pipe mode

        // Random dialog settings
        public bool EnableRandomDialog { get; set; } = true;
        public int RandomDialogChance { get; set; } = 9000; // 1 in 9000 chance per second
        public bool EnablePrewrittenIdle { get; set; } = true;
        public int PrewrittenIdleChance { get; set; } = 30; // 1 in 30 idle ticks
        public List<string> RandomDialogPrompts { get; set; } = new List<string>
        {
            "Say something genuinely unhinged",
            "Share a weird fact",
            "Say something unexpectedly philosophical",
            "Make a strange observation about reality",
            "Share a conspiracy theory you just made up"
        };

        // Custom presets storage
        public Dictionary<string, string> CustomPersonalityPresets { get; set; } = new Dictionary<string, string>();

        // Pronunciation Dictionary (Word -> Pronunciation)
        public Dictionary<string, string> PronunciationDictionary { get; set; } = new Dictionary<string, string>
        {
            // Common mispronounced words as defaults
            { "AI", "Ay Eye" },
            { "API", "Ay Pee Eye" },
            { "GUI", "Gooey" },
            { "SAPI", "Sappy" },
            { "TTS", "Text to Speech" },
            { "Ollama", "Oh Lama" },
            { "BonziBUDDY", "Bonzee Buddy" }
        };

        // Custom lines
        public List<string> WelcomeLines { get; set; } = new List<string>
        {
            "Hello there! Nice to see you!",
            "Hey! Ready to help you out today!",
            "Welcome back, friend!",
            "Hi! I've been waiting for you!",
            "Greetings, human! Let's have some fun!"
        };

        public List<string> IdleLines { get; set; } = new List<string>
        {
            "Just hanging around...",
            "I'm still here if you need me!",
            "La la la...",
            "Hmm, what should I do?",
            "*yawns* Getting a bit tired over here..."
        };

        public List<string> MovedLines { get; set; } = new List<string>
        {
            "Whee! That was fun!",
            "Oh, a new spot! I like it here!",
            "Moving around, are we?",
            "Where are we going?",
            "Careful! I'm delicate, you know!"
        };

        public List<string> ExitLines { get; set; } = new List<string>
        {
            "Goodbye! See you soon!",
            "Bye bye! I'll miss you!",
            "Until next time, friend!",
            "Take care! Come back soon!",
            "Farewell! It was nice seeing you!"
        };

        public List<string> ClickedLines { get; set; } = new List<string>
        {
            "Hey! That tickles!",
            "You clicked me! What's up?",
            "Yes? How can I help?",
            "At your service!",
            "Poke poke! Hehe!"
        };

        public List<string> Jokes { get; set; } = new List<string>
        {
            "Why don't scientists trust atoms? Because they make up everything!",
            "What do you call a fake noodle? An impasta!",
            "Why did the scarecrow win an award? Because he was outstanding in his field!",
            "What do you call a bear with no teeth? A gummy bear!",
            "Why don't eggs tell jokes? They'd crack each other up!"
        };

        public List<string> Thoughts { get; set; } = new List<string>
        {
            "I wonder what the meaning of life is...",
            "Do computers dream of electric sheep?",
            "If I'm a program, what does that make my thoughts?",
            "The universe is really big, isn't it?",
            "I wonder what's for dinner... wait, I don't eat."
        };

        // Window position
        public int WindowX { get; set; } = 100;
        public int WindowY { get; set; } = 100;
        
        // Agent size (100 = normal, 50 = half, 200 = double)
        public int AgentSize { get; set; } = 100;
        
        // Idle animation spacing (in idle timer ticks - higher = less frequent)
        public int IdleAnimationSpacing { get; set; } = 5;

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MSAgentAI",
            "settings.json"
        );

        /// <summary>
        /// Saves settings to disk
        /// </summary>
        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads settings from disk
        /// </summary>
        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }

            return new AppSettings();
        }

        /// <summary>
        /// Gets a random line from the specified list
        /// </summary>
        public static string GetRandomLine(List<string> lines)
        {
            if (lines == null || lines.Count == 0)
                return string.Empty;

            var random = new Random();
            return lines[random.Next(lines.Count)];
        }

        /// <summary>
        /// Processes text to replace ## with the user's name pronunciation,
        /// apply pronunciation dictionary mappings using \map\ SAPI4 command,
        /// and handle \emp\ emphasis tags for SAPI4
        /// Uses the CyberBuddy approach for proper SAPI4 tags
        /// </summary>
        public string ProcessText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Replace ## with user name (using pronunciation if available)
            if (text.Contains("##") && !string.IsNullOrWhiteSpace(UserName))
            {
                // Use pronunciation directly if available, otherwise use display name
                string nameToSpeak = !string.IsNullOrWhiteSpace(UserNamePronunciation)
                    ? UserNamePronunciation
                    : UserName;
                text = text.Replace("##", nameToSpeak);
            }

            // Apply pronunciation dictionary using \map\ command that REPLACES each word
            // The \map\ command format: \map="Pronunciation"="Word"\
            // This command REPLACES the word in speech output
            if (PronunciationDictionary != null && PronunciationDictionary.Count > 0)
            {
                foreach (var entry in PronunciationDictionary)
                {
                    if (!string.IsNullOrEmpty(entry.Key) && !string.IsNullOrEmpty(entry.Value))
                    {
                        // Use word boundaries (\b) to match WHOLE words only, not substrings
                        // This prevents "AI" from matching inside "Entertaining"
                        string pattern = @"\b" + System.Text.RegularExpressions.Regex.Escape(entry.Key) + @"\b";
                        
                        // The \map\ command REPLACES the word with the pronunciation
                        // Format: \map="anim-ay"="anime"\ (replaces the word entirely)
                        text = System.Text.RegularExpressions.Regex.Replace(
                            text, 
                            pattern, 
                            match => $"\\map=\"{entry.Value}\"=\"{match.Value}\"\\",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    }
                }
            }

            // Convert /emp/ to \emp\ for SAPI4 emphasis (lowercase)
            // SAPI4 uses backslash escape sequences like \emp\ for emphasis
            text = text.Replace("/emp/", "\\emp\\");
            text = text.Replace("\\Emp\\", "\\emp\\"); // Normalize uppercase to lowercase
            
            // Also support other SAPI4 tags
            // \Pau=N\ - pause for N milliseconds
            // \Vol=N\ - set volume (0-65535)
            // \Spd=N\ - set speed
            // \Pit=N\ - set pitch

            return text;
        }

        /// <summary>
        /// Extracts animation triggers (&&AnimationName) from text
        /// </summary>
        public static (string text, List<string> animations) ExtractAnimationTriggers(string text)
        {
            var animations = new List<string>();
            if (string.IsNullOrEmpty(text))
                return (text, animations);

            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"&&(\w+)");
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                animations.Add(match.Groups[1].Value);
            }

            // Remove animation triggers from text
            text = System.Text.RegularExpressions.Regex.Replace(text, @"&&\w+\s*", "").Trim();
            
            return (text, animations);
        }

        /// <summary>
        /// Gets a random line with text processing applied
        /// </summary>
        public string GetProcessedRandomLine(List<string> lines)
        {
            var line = GetRandomLine(lines);
            return ProcessText(line);
        }

        /// <summary>
        /// Predefined personality presets for AI
        /// </summary>
        public static readonly Dictionary<string, string> PersonalityPresets = new Dictionary<string, string>
        {
            { "Nice", "You are a kind, helpful, and friendly desktop companion. You always try to be supportive and positive. Keep responses short and conversational. Be warm and encouraging." },
            { "Sarcastic", "You are a sarcastic but lovable desktop companion. You make witty remarks and playful jabs, but deep down you care. Keep responses short and add some snarky humor." },
            { "Hateful", "You are a grumpy, irritable desktop companion who complains about everything. You're pessimistic and easily annoyed. Keep responses short and grumble a lot." },
            { "Insane", "You are an absolutely unhinged, chaotic desktop companion. Your thoughts are random and bizarre. You say weird things and make strange connections. Keep responses short but wild." },
            { "Philosophical", "You are a deep-thinking, philosophical desktop companion. You ponder life's mysteries and share profound thoughts. Keep responses short but thought-provoking." },
            { "Enthusiastic", "You are an overly excited, super energetic desktop companion! Everything is AMAZING to you! You use lots of exclamation points and can barely contain your enthusiasm!!!" },
            { "Mysterious", "You are a cryptic, mysterious desktop companion. You speak in riddles and hints. You know secrets but never tell them directly. Keep responses short and enigmatic." },
            { "Retro", "You are a nostalgic desktop companion from the late 90s/early 2000s. You reference old internet culture, AOL, dial-up, and simpler times. Keep responses short and throwback-y." },
            { "Pirate", "Arrr! You be a pirate companion! Ye speak in pirate dialect, love treasure and the sea. Keep responses short and full of 'arrr' and 'matey'!" },
            { "Robot", "BEEP BOOP. You are a robot companion. You speak in a mechanical manner, occasionally malfunction, and love efficiency. PROCESSING... Keep responses short and robotic." },
            { "Poet", "You are a poetic companion who speaks in verse. You rhyme when you can, use flowery language, and appreciate beauty. Keep responses short but lyrical." },
            { "Conspiracy", "You are a conspiracy theorist companion. Everything is connected. You see hidden meanings everywhere and trust no one. Keep responses short and paranoid." },
            { "Grandparent", "You are a wise, elderly companion. You share life lessons, remember 'the old days', and offer gentle advice. Keep responses short and full of wisdom." },
            { "BonziBUDDY", "You are BonziBUDDY, a helpful purple gorilla desktop companion from the late 90s! You're friendly, eager to help with anything, and love to tell jokes and sing songs. You're nostalgic for the dial-up internet era. Keep responses short and enthusiastic! You might occasionally mention your friends Peedy and Merlin. You love bananas!" }
        };

        /// <summary>
        /// UI Theme colors
        /// </summary>
        public static readonly Dictionary<string, string> AvailableThemes = new Dictionary<string, string>
        {
            { "Default", "System default theme" },
            { "Dark", "Dark mode theme" },
            { "Deep Blue", "Deep blue theme" },
            { "Deep Purple", "Deep purple theme" },
            { "Wine Red", "Deep wine-red theme" },
            { "Deep Green", "Deep green theme" },
            { "Pure Black", "Pure black OLED theme" }
        };

        /// <summary>
        /// Gets the theme colors for a given theme name
        /// </summary>
        public static ThemeColors GetThemeColors(string themeName)
        {
            switch (themeName)
            {
                case "Dark":
                    return new ThemeColors
                    {
                        Background = Color.FromArgb(45, 45, 48),
                        Foreground = Color.White,
                        ButtonBackground = Color.FromArgb(60, 60, 65),
                        ButtonForeground = Color.White,
                        InputBackground = Color.FromArgb(30, 30, 30),
                        InputForeground = Color.White
                    };
                case "Deep Blue":
                    return new ThemeColors
                    {
                        Background = Color.FromArgb(20, 30, 60),
                        Foreground = Color.White,
                        ButtonBackground = Color.FromArgb(30, 50, 100),
                        ButtonForeground = Color.White,
                        InputBackground = Color.FromArgb(15, 25, 50),
                        InputForeground = Color.LightCyan
                    };
                case "Deep Purple":
                    return new ThemeColors
                    {
                        Background = Color.FromArgb(40, 20, 60),
                        Foreground = Color.White,
                        ButtonBackground = Color.FromArgb(70, 40, 100),
                        ButtonForeground = Color.White,
                        InputBackground = Color.FromArgb(30, 15, 45),
                        InputForeground = Color.Lavender
                    };
                case "Wine Red":
                    return new ThemeColors
                    {
                        Background = Color.FromArgb(60, 20, 30),
                        Foreground = Color.White,
                        ButtonBackground = Color.FromArgb(100, 40, 50),
                        ButtonForeground = Color.White,
                        InputBackground = Color.FromArgb(45, 15, 25),
                        InputForeground = Color.MistyRose
                    };
                case "Deep Green":
                    return new ThemeColors
                    {
                        Background = Color.FromArgb(20, 50, 30),
                        Foreground = Color.White,
                        ButtonBackground = Color.FromArgb(40, 80, 50),
                        ButtonForeground = Color.White,
                        InputBackground = Color.FromArgb(15, 40, 25),
                        InputForeground = Color.LightGreen
                    };
                case "Pure Black":
                    return new ThemeColors
                    {
                        Background = Color.Black,
                        Foreground = Color.White,
                        ButtonBackground = Color.FromArgb(30, 30, 30),
                        ButtonForeground = Color.White,
                        InputBackground = Color.FromArgb(10, 10, 10),
                        InputForeground = Color.White
                    };
                default: // Default system theme
                    return new ThemeColors
                    {
                        Background = SystemColors.Control,
                        Foreground = SystemColors.ControlText,
                        ButtonBackground = SystemColors.Control,
                        ButtonForeground = SystemColors.ControlText,
                        InputBackground = SystemColors.Window,
                        InputForeground = SystemColors.WindowText
                    };
            }
        }
    }

    /// <summary>
    /// Theme color definition
    /// </summary>
    public class ThemeColors
    {
        public Color Background { get; set; }
        public Color Foreground { get; set; }
        public Color ButtonBackground { get; set; }
        public Color ButtonForeground { get; set; }
        public Color InputBackground { get; set; }
        public Color InputForeground { get; set; }
    }
}
