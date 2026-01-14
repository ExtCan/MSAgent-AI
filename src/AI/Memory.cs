using System;
using Newtonsoft.Json;

namespace MSAgentAI.AI
{
    /// <summary>
    /// Represents a memory item stored by the AI
    /// </summary>
    public class Memory
    {
        /// <summary>
        /// Unique identifier for the memory
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The content of the memory
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// When the memory was created
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Importance score of the memory (higher = more important)
        /// Used to determine if memory should be created and retained
        /// </summary>
        [JsonProperty("importance")]
        public double Importance { get; set; }

        /// <summary>
        /// Category of the memory (e.g., "user_info", "preference", "event", "fact")
        /// </summary>
        [JsonProperty("category")]
        public string Category { get; set; }

        /// <summary>
        /// Optional tags for organizing memories
        /// </summary>
        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        /// <summary>
        /// Number of times this memory has been accessed/used
        /// </summary>
        [JsonProperty("access_count")]
        public int AccessCount { get; set; }

        /// <summary>
        /// Last time this memory was accessed
        /// </summary>
        [JsonProperty("last_accessed")]
        public DateTime LastAccessed { get; set; }

        public Memory()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTime.Now;
            LastAccessed = DateTime.Now;
            AccessCount = 0;
            Tags = new string[0];
        }

        /// <summary>
        /// Increments the access count and updates last accessed time
        /// </summary>
        public void MarkAccessed()
        {
            AccessCount++;
            LastAccessed = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Category}] {Content} (Importance: {Importance:F1})";
        }
    }
}
