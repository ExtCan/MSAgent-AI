using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MSAgentAI.AI
{
    /// <summary>
    /// Manages integration with Ollama AI for dynamic chat functionality
    /// </summary>
    public class OllamaClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private bool _disposed;

        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string Model { get; set; } = "llama2";
        public string PersonalityPrompt { get; set; } = "";
        public int MaxTokens { get; set; } = 150;
        public double Temperature { get; set; } = 0.8;
        
        // Available animations for AI to use
        public List<string> AvailableAnimations { get; set; } = new List<string>();

        private List<ChatMessage> _conversationHistory = new List<ChatMessage>();

        // Enforced system prompt additions
        private const string ENFORCED_RULES = @"
IMPORTANT RULES YOU MUST FOLLOW:
1. NEVER use em dashes (—), asterisks (*), or emojis in your responses.
2. Use /emp/ before words you want to emphasize (e.g., 'This is /emp/very important').
3. You can trigger animations by including &&AnimationName in your response (e.g., '&&Surprised Oh wow!'). Use animations sparingly.
4. RARELY use whisper - only for actual secrets or suspense. To whisper, wrap text in [whisper]...[/whisper] tags. Do NOT whisper in normal conversation.
5. Keep responses short and conversational (1-3 sentences).
6. Speak naturally as a desktop companion character.
";

        public OllamaClient()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(120)
            };
        }

        /// <summary>
        /// Tests the connection to Ollama
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/tags");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets available models from Ollama
        /// </summary>
        public async Task<List<string>> GetAvailableModelsAsync()
        {
            var models = new List<string>();

            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/tags");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<OllamaTagsResponse>(content);
                    if (result?.Models != null)
                    {
                        foreach (var model in result.Models)
                        {
                            // Only add the base model name (before the colon for versions)
                            string modelName = model.Name;
                            if (!string.IsNullOrEmpty(modelName))
                            {
                                models.Add(modelName);
                            }
                        }
                    }
                }
            }
            catch { }

            return models;
        }

        /// <summary>
        /// Builds the full system prompt with personality and rules
        /// </summary>
        private string BuildSystemPrompt()
        {
            var prompt = new StringBuilder();
            
            if (!string.IsNullOrEmpty(PersonalityPrompt))
            {
                prompt.AppendLine(PersonalityPrompt);
                prompt.AppendLine();
            }
            
            prompt.AppendLine(ENFORCED_RULES);
            
            if (AvailableAnimations.Count > 0)
            {
                prompt.AppendLine();
                prompt.AppendLine("Available animations you can use with && prefix: " + string.Join(", ", AvailableAnimations.GetRange(0, Math.Min(20, AvailableAnimations.Count))));
            }
            
            return prompt.ToString();
        }

        /// <summary>
        /// Cleans the AI response to remove forbidden characters
        /// </summary>
        public static string CleanResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return response;

            // Remove em dashes
            response = response.Replace("—", "-");
            response = response.Replace("–", "-");
            
            // Remove asterisks (but keep ** for bold if needed)
            response = Regex.Replace(response, @"\*+", "");
            
            // Remove emojis (Unicode emoji ranges)
            response = Regex.Replace(response, @"[\u2600-\u26FF\u2700-\u27BF\uD83C-\uDBFF\uDC00-\uDFFF]+", "");
            
            // Clean up extra whitespace
            response = Regex.Replace(response, @"\s+", " ").Trim();
            
            return response;
        }

        /// <summary>
        /// Extracts animation triggers from text (&&AnimationName)
        /// </summary>
        public static (string text, List<string> animations) ExtractAnimations(string text)
        {
            var animations = new List<string>();
            if (string.IsNullOrEmpty(text))
                return (text, animations);

            var matches = Regex.Matches(text, @"&&(\w+)");
            foreach (Match match in matches)
            {
                animations.Add(match.Groups[1].Value);
            }

            // Remove animation triggers from text
            text = Regex.Replace(text, @"&&\w+\s*", "").Trim();
            
            return (text, animations);
        }

        /// <summary>
        /// Sends a chat message to Ollama and gets a response
        /// </summary>
        public async Task<string> ChatAsync(string message, CancellationToken cancellationToken = default)
        {
            try
            {
                // Build the messages list with personality and history
                var messages = new List<object>();

                // Add system message with personality and rules
                string systemPrompt = BuildSystemPrompt();
                if (!string.IsNullOrEmpty(systemPrompt))
                {
                    messages.Add(new { role = "system", content = systemPrompt });
                }

                // Add conversation history (limit to last 10 messages)
                int startIndex = Math.Max(0, _conversationHistory.Count - 10);
                for (int i = startIndex; i < _conversationHistory.Count; i++)
                {
                    messages.Add(new
                    {
                        role = _conversationHistory[i].Role,
                        content = _conversationHistory[i].Content
                    });
                }

                // Add the new user message
                messages.Add(new { role = "user", content = message });

                var request = new
                {
                    model = Model,
                    messages = messages,
                    stream = false,
                    options = new
                    {
                        num_predict = MaxTokens,
                        temperature = Temperature
                    }
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/api/chat", content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<OllamaChatResponse>(responseContent);

                    if (result?.Message?.Content != null)
                    {
                        string cleanedResponse = CleanResponse(result.Message.Content);
                        
                        // Add to conversation history
                        _conversationHistory.Add(new ChatMessage { Role = "user", Content = message });
                        _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = cleanedResponse });

                        return cleanedResponse;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Ollama error: {response.StatusCode} - {errorContent}");
                }

                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ollama chat error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Generates a random dialog using Ollama
        /// </summary>
        public async Task<string> GenerateRandomDialogAsync(string customPrompt = null, CancellationToken cancellationToken = default)
        {
            string prompt = customPrompt ?? "Say something short, interesting, and in-character. Use /emp/ for emphasis and optionally include an &&animation trigger.";

            try
            {
                var messages = new List<object>();

                // Add system prompt with personality and rules
                string systemPrompt = BuildSystemPrompt();
                if (!string.IsNullOrEmpty(systemPrompt))
                {
                    messages.Add(new { role = "system", content = systemPrompt });
                }

                messages.Add(new { role = "user", content = prompt });

                var request = new
                {
                    model = Model,
                    messages = messages,
                    stream = false,
                    options = new
                    {
                        num_predict = 100,
                        temperature = 1.0
                    }
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/api/chat", content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<OllamaChatResponse>(responseContent);
                    return CleanResponse(result?.Message?.Content);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Clears the conversation history
        /// </summary>
        public void ClearHistory()
        {
            _conversationHistory.Clear();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }

        // Response classes for JSON deserialization
        private class OllamaTagsResponse
        {
            [JsonProperty("models")]
            public List<OllamaModel> Models { get; set; }
        }

        private class OllamaModel
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        private class OllamaChatResponse
        {
            [JsonProperty("message")]
            public OllamaChatMessage Message { get; set; }
        }

        private class OllamaChatMessage
        {
            [JsonProperty("content")]
            public string Content { get; set; }
        }

        private class ChatMessage
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }
    }
}
