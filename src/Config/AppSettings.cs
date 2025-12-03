using System;
using System.Collections.Generic;
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

        // Voice settings
        public string SelectedVoiceId { get; set; } = "";
        public int VoiceSpeed { get; set; } = 150;
        public int VoicePitch { get; set; } = 100;
        public int VoiceVolume { get; set; } = 65535;

        // Ollama AI settings
        public string OllamaUrl { get; set; } = "http://localhost:11434";
        public string OllamaModel { get; set; } = "llama2";
        public string PersonalityPrompt { get; set; } = "You are a helpful and friendly desktop companion. Keep responses short and conversational.";
        public bool EnableOllamaChat { get; set; } = false;

        // Random dialog settings
        public bool EnableRandomDialog { get; set; } = true;
        public int RandomDialogChance { get; set; } = 9000; // 1 in 9000 chance per second
        public List<string> RandomDialogPrompts { get; set; } = new List<string>
        {
            "Say something genuinely unhinged",
            "Share a weird fact",
            "Say something unexpectedly philosophical",
            "Make a strange observation about reality",
            "Share a conspiracy theory you just made up"
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
    }
}
