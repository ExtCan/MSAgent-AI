using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
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

        private List<ChatMessage> _conversationHistory = new List<ChatMessage>();

        public OllamaClient()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60)
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
                            models.Add(model.Name);
                        }
                    }
                }
            }
            catch { }

            return models;
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

                // Add system message with personality if set
                if (!string.IsNullOrEmpty(PersonalityPrompt))
                {
                    messages.Add(new { role = "system", content = PersonalityPrompt });
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
                        // Add to conversation history
                        _conversationHistory.Add(new ChatMessage { Role = "user", Content = message });
                        _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = result.Message.Content });

                        return result.Message.Content;
                    }
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
        /// Generates a random unhinged statement using Ollama
        /// </summary>
        public async Task<string> GenerateRandomDialogAsync(string customPrompt = null, CancellationToken cancellationToken = default)
        {
            string prompt = customPrompt ?? "Say something genuinely unhinged, weird, or unexpectedly philosophical. Keep it short (1-2 sentences max). Be creative and surprising.";

            try
            {
                var messages = new List<object>();

                // Add personality context if available
                if (!string.IsNullOrEmpty(PersonalityPrompt))
                {
                    messages.Add(new { role = "system", content = PersonalityPrompt });
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
                        temperature = 1.2 // Higher temperature for more randomness
                    }
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/api/chat", content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<OllamaChatResponse>(responseContent);
                    return result?.Message?.Content;
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
