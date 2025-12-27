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
        
        private string _apiKey = "";
        public string ApiKey 
        { 
            get => _apiKey;
            set
            {
                _apiKey = value;
                UpdateHttpClientHeaders();
            }
        }
        
        public bool EnableWebSearch { get; set; } = false;
        public bool EnableUrlReading { get; set; } = false;
        
        // Available animations for AI to use
        public List<string> AvailableAnimations { get; set; } = new List<string>();

        private List<ChatMessage> _conversationHistory = new List<ChatMessage>();
        
        // Token usage tracking
        public int LastPromptTokens { get; private set; }
        public int LastCompletionTokens { get; private set; }
        public int LastTotalTokens { get; private set; }
        public int TotalPromptTokens { get; private set; }
        public int TotalCompletionTokens { get; private set; }
        public int TotalTokensUsed { get; private set; }
        
        // Tool definitions for Ollama
        private static readonly object[] _tools = new object[]
        {
            new
            {
                type = "function",
                function = new
                {
                    name = "web_search",
                    description = "Search the web for current information. Use this when you need up-to-date information or facts that you don't have in your training data.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            query = new
                            {
                                type = "string",
                                description = "The search query to look up"
                            }
                        },
                        required = new[] { "query" }
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "read_url",
                    description = "Read the content from a specific URL. Use this when you need to get information from a specific webpage.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            url = new
                            {
                                type = "string",
                                description = "The URL to read content from"
                            }
                        },
                        required = new[] { "url" }
                    }
                }
            }
        };

        // Enforced system prompt additions
        private const string ENFORCED_RULES = @"
IMPORTANT RULES YOU MUST FOLLOW:
1. NEVER use em dashes (—), asterisks (*), or emojis in your responses.
2. Use /emp/ before words you want to emphasize (e.g., 'This is /emp/very important').
3. You may include ONE animation per response by putting &&AnimationName at the start (e.g., '&&Surprised Oh wow!'). Only use ONE animation maximum.
4. Keep responses short and conversational (1-3 sentences).
5. Speak naturally as a desktop companion character.
";
        
        // URL content reading limit (characters)
        private const int MAX_URL_CONTENT_LENGTH = 2000;

        public OllamaClient()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(120)
            };
        }
        
        /// <summary>
        /// Sets the API key for authentication
        /// </summary>
        public void SetApiKey(string apiKey)
        {
            ApiKey = apiKey;
        }
        
        /// <summary>
        /// Updates HttpClient headers with API key if set
        /// </summary>
        private void UpdateHttpClientHeaders()
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            if (!string.IsNullOrEmpty(ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
            }
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

                // Build request object
                object request;
                if (EnableWebSearch || EnableUrlReading)
                {
                    // Filter tools based on enabled features
                    var enabledTools = new List<object>();
                    if (EnableWebSearch && _tools.Length > 0)
                    {
                        enabledTools.Add(_tools[0]); // web_search
                    }
                    if (EnableUrlReading && _tools.Length > 1)
                    {
                        enabledTools.Add(_tools[1]); // read_url
                    }
                    
                    // Include only enabled tools in the request
                    if (enabledTools.Count > 0)
                    {
                        request = new
                        {
                            model = Model,
                            messages = messages,
                            stream = false,
                            tools = enabledTools.ToArray(),
                            options = new
                            {
                                num_predict = MaxTokens,
                                temperature = Temperature
                            }
                        };
                    }
                    else
                    {
                        request = new
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
                    }
                }
                else
                {
                    request = new
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
                }

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/api/chat", content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<OllamaChatResponse>(responseContent);

                    // Track token usage
                    LastPromptTokens = result.PromptEvalCount;
                    LastCompletionTokens = result.EvalCount;
                    LastTotalTokens = LastPromptTokens + LastCompletionTokens;
                    TotalPromptTokens += LastPromptTokens;
                    TotalCompletionTokens += LastCompletionTokens;
                    TotalTokensUsed += LastTotalTokens;

                    // Check if the model wants to use tools
                    if (result?.Message?.ToolCalls != null && result.Message.ToolCalls.Count > 0)
                    {
                        // Execute tool calls
                        foreach (var toolCall in result.Message.ToolCalls)
                        {
                            if (toolCall?.Function != null)
                            {
                                var toolResult = await ExecuteToolAsync(
                                    toolCall.Function.Name,
                                    toolCall.Function.Arguments ?? new Dictionary<string, object>()
                                );

                                // Add tool result to messages
                                messages.Add(new
                                {
                                    role = "tool",
                                    content = toolResult
                                });
                            }
                        }

                        // Make another request with tool results
                        var followUpRequest = new
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

                        var followUpJson = JsonConvert.SerializeObject(followUpRequest);
                        var followUpContent = new StringContent(followUpJson, Encoding.UTF8, "application/json");
                        var followUpResponse = await _httpClient.PostAsync($"{BaseUrl}/api/chat", followUpContent, cancellationToken);

                        if (followUpResponse.IsSuccessStatusCode)
                        {
                            var followUpResponseContent = await followUpResponse.Content.ReadAsStringAsync();
                            var followUpResult = JsonConvert.DeserializeObject<OllamaChatResponse>(followUpResponseContent);

                            if (followUpResult?.Message?.Content != null)
                            {
                                string cleanedResponse = CleanResponse(followUpResult.Message.Content);

                                // Update token usage with follow-up request
                                // Add follow-up tokens to the cumulative totals
                                TotalPromptTokens += followUpResult.PromptEvalCount;
                                TotalCompletionTokens += followUpResult.EvalCount;
                                TotalTokensUsed += followUpResult.PromptEvalCount + followUpResult.EvalCount;
                                
                                // Update last request to include both initial and follow-up tokens
                                LastPromptTokens += followUpResult.PromptEvalCount;
                                LastCompletionTokens += followUpResult.EvalCount;
                                LastTotalTokens += followUpResult.PromptEvalCount + followUpResult.EvalCount;

                                // Add to conversation history
                                _conversationHistory.Add(new ChatMessage { Role = "user", Content = message });
                                _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = cleanedResponse });

                                return cleanedResponse;
                            }
                        }
                    }
                    
                    // If we haven't returned yet, check if the original response has content
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
        
        /// <summary>
        /// Resets token usage counters
        /// </summary>
        public void ResetTokenCounters()
        {
            LastPromptTokens = 0;
            LastCompletionTokens = 0;
            LastTotalTokens = 0;
            TotalPromptTokens = 0;
            TotalCompletionTokens = 0;
            TotalTokensUsed = 0;
        }
        
        /// <summary>
        /// Gets a summary of token usage
        /// </summary>
        public string GetTokenUsageSummary()
        {
            return $"Last Request: {LastPromptTokens} prompt + {LastCompletionTokens} completion = {LastTotalTokens} total\n" +
                   $"Total Session: {TotalPromptTokens} prompt + {TotalCompletionTokens} completion = {TotalTokensUsed} total";
        }
        
        /// <summary>
        /// Performs a web search using DuckDuckGo
        /// </summary>
        private async Task<string> WebSearchAsync(string query)
        {
            try
            {
                // Use DuckDuckGo's instant answer API (no API key required)
                var searchUrl = $"https://api.duckduckgo.com/?q={Uri.EscapeDataString(query)}&format=json&no_html=1&skip_disambig=1";
                var response = await _httpClient.GetAsync(searchUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<DuckDuckGoResponse>(content);
                    
                    if (!string.IsNullOrEmpty(result?.AbstractText))
                    {
                        return result.AbstractText;
                    }
                    
                    if (!string.IsNullOrEmpty(result?.Abstract))
                    {
                        return result.Abstract;
                    }
                    
                    // If we have related topics, return the first one
                    if (result?.RelatedTopics?.Count > 0)
                    {
                        var topic = result.RelatedTopics[0];
                        if (topic.Text != null)
                        {
                            return topic.Text;
                        }
                    }
                    
                    return "No relevant information found for this query.";
                }
                
                return "Web search failed.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Web search error: {ex.Message}");
                return $"Error performing web search: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Reads content from a URL
        /// </summary>
        private async Task<string> ReadUrlAsync(string url)
        {
            try
            {
                // Validate URL
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    return "Invalid URL format. Only HTTP and HTTPS URLs are supported.";
                }
                
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    // Basic HTML stripping (simple approach)
                    content = Regex.Replace(content, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
                    content = Regex.Replace(content, @"<style[^>]*>[\s\S]*?</style>", "", RegexOptions.IgnoreCase);
                    content = Regex.Replace(content, @"<[^>]+>", " ");
                    content = Regex.Replace(content, @"\s+", " ");
                    content = System.Net.WebUtility.HtmlDecode(content).Trim();
                    
                    // Limit content length to avoid excessive token usage
                    if (content.Length > MAX_URL_CONTENT_LENGTH)
                    {
                        content = content.Substring(0, MAX_URL_CONTENT_LENGTH) + "... (truncated)";
                    }
                    
                    return content;
                }
                
                return $"Failed to read URL. Status: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"URL reading error: {ex.Message}");
                return $"Error reading URL: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Executes a tool call
        /// </summary>
        private async Task<string> ExecuteToolAsync(string toolName, Dictionary<string, object> arguments)
        {
            switch (toolName)
            {
                case "web_search":
                    if (arguments.ContainsKey("query"))
                    {
                        return await WebSearchAsync(arguments["query"].ToString());
                    }
                    return "Missing 'query' parameter for web_search";
                    
                case "read_url":
                    if (arguments.ContainsKey("url"))
                    {
                        return await ReadUrlAsync(arguments["url"].ToString());
                    }
                    return "Missing 'url' parameter for read_url";
                    
                default:
                    return $"Unknown tool: {toolName}";
            }
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
            
            [JsonProperty("prompt_eval_count")]
            public int PromptEvalCount { get; set; }
            
            [JsonProperty("eval_count")]
            public int EvalCount { get; set; }
            
            [JsonProperty("total_duration")]
            public long TotalDuration { get; set; }
            
            [JsonProperty("load_duration")]
            public long LoadDuration { get; set; }
            
            [JsonProperty("prompt_eval_duration")]
            public long PromptEvalDuration { get; set; }
            
            [JsonProperty("eval_duration")]
            public long EvalDuration { get; set; }
        }

        private class OllamaChatMessage
        {
            [JsonProperty("content")]
            public string Content { get; set; }
            
            [JsonProperty("tool_calls")]
            public List<OllamaToolCall> ToolCalls { get; set; }
        }
        
        private class OllamaToolCall
        {
            [JsonProperty("function")]
            public OllamaToolFunction Function { get; set; }
        }
        
        private class OllamaToolFunction
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            
            [JsonProperty("arguments")]
            public Dictionary<string, object> Arguments { get; set; }
        }

        private class ChatMessage
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }
        
        // DuckDuckGo API response classes
        private class DuckDuckGoResponse
        {
            [JsonProperty("AbstractText")]
            public string AbstractText { get; set; }
            
            [JsonProperty("Abstract")]
            public string Abstract { get; set; }
            
            [JsonProperty("RelatedTopics")]
            public List<DuckDuckGoTopic> RelatedTopics { get; set; }
        }
        
        private class DuckDuckGoTopic
        {
            [JsonProperty("Text")]
            public string Text { get; set; }
        }
    }
}
