using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MSAgentAI.AI
{
    /// <summary>
    /// Manages the AI's memory system - storing, retrieving, and managing memories
    /// </summary>
    public class MemoryManager
    {
        private List<Memory> _memories;
        private readonly string _memoriesPath;
        private const int MaxMemories = 1000; // Limit to prevent unbounded growth

        /// <summary>
        /// Whether memory system is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Threshold for creating memories (0.1 to 10.0)
        /// Lower = easier to create memories, Higher = only important things are remembered
        /// </summary>
        public double MemoryThreshold { get; set; }

        public MemoryManager()
        {
            _memoriesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MSAgentAI",
                "memories.json"
            );

            _memories = new List<Memory>();
            Enabled = false;
            MemoryThreshold = 5.0; // Default: moderate threshold
            LoadMemories();
        }

        /// <summary>
        /// Gets all memories
        /// </summary>
        public List<Memory> GetAllMemories()
        {
            return new List<Memory>(_memories);
        }

        /// <summary>
        /// Gets memories filtered by category
        /// </summary>
        public List<Memory> GetMemoriesByCategory(string category)
        {
            return _memories.Where(m => m.Category == category).ToList();
        }

        /// <summary>
        /// Gets the most relevant memories for the AI context
        /// Returns up to maxCount memories, sorted by importance and recency
        /// </summary>
        public List<Memory> GetRelevantMemories(int maxCount = 10)
        {
            if (!Enabled || _memories.Count == 0)
                return new List<Memory>();

            // Score memories based on importance, recency, and access frequency
            var scoredMemories = _memories.Select(m => new
            {
                Memory = m,
                Score = CalculateRelevanceScore(m)
            })
            .OrderByDescending(x => x.Score)
            .Take(maxCount)
            .Select(x => x.Memory)
            .ToList();

            // Mark memories as accessed
            foreach (var memory in scoredMemories)
            {
                memory.MarkAccessed();
            }

            return scoredMemories;
        }

        /// <summary>
        /// Calculates a relevance score for a memory
        /// </summary>
        private double CalculateRelevanceScore(Memory memory)
        {
            // Base score is importance
            double score = memory.Importance;

            // Bonus for recent memories (decay over 30 days)
            var daysSinceCreation = (DateTime.Now - memory.Timestamp).TotalDays;
            var recencyBonus = Math.Max(0, 1.0 - (daysSinceCreation / 30.0));
            score += recencyBonus;

            // Bonus for frequently accessed memories
            var accessBonus = Math.Min(2.0, memory.AccessCount * 0.1);
            score += accessBonus;

            return score;
        }

        /// <summary>
        /// Adds a new memory if it meets the threshold
        /// </summary>
        public bool AddMemory(string content, double importance, string category = "general", string[] tags = null)
        {
            if (!Enabled)
                return false;

            // Check if importance meets threshold
            if (importance < MemoryThreshold)
                return false;

            var memory = new Memory
            {
                Content = content,
                Importance = importance,
                Category = category,
                Tags = tags ?? new string[0]
            };

            _memories.Add(memory);

            // Limit memory count by removing oldest, least important memories
            if (_memories.Count > MaxMemories)
            {
                var toRemove = _memories
                    .OrderBy(m => CalculateRelevanceScore(m))
                    .First();
                _memories.Remove(toRemove);
            }

            SaveMemories();
            return true;
        }

        /// <summary>
        /// Updates an existing memory
        /// </summary>
        public bool UpdateMemory(string id, string content, double importance, string category, string[] tags)
        {
            var memory = _memories.FirstOrDefault(m => m.Id == id);
            if (memory == null)
                return false;

            memory.Content = content;
            memory.Importance = importance;
            memory.Category = category;
            memory.Tags = tags;

            SaveMemories();
            return true;
        }

        /// <summary>
        /// Removes a memory by ID
        /// </summary>
        public bool RemoveMemory(string id)
        {
            var memory = _memories.FirstOrDefault(m => m.Id == id);
            if (memory == null)
                return false;

            _memories.Remove(memory);
            SaveMemories();
            return true;
        }

        /// <summary>
        /// Clears all memories
        /// </summary>
        public void ClearAllMemories()
        {
            _memories.Clear();
            SaveMemories();
        }

        /// <summary>
        /// Searches memories by content
        /// </summary>
        public List<Memory> SearchMemories(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllMemories();

            return _memories
                .Where(m => m.Content.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderByDescending(m => CalculateRelevanceScore(m))
                .ToList();
        }

        /// <summary>
        /// Gets memory statistics
        /// </summary>
        public MemoryStats GetStats()
        {
            return new MemoryStats
            {
                TotalMemories = _memories.Count,
                AverageImportance = _memories.Count > 0 ? _memories.Average(m => m.Importance) : 0,
                OldestMemory = _memories.Count > 0 ? _memories.Min(m => m.Timestamp) : DateTime.Now,
                NewestMemory = _memories.Count > 0 ? _memories.Max(m => m.Timestamp) : DateTime.Now,
                CategoriesCount = _memories.Select(m => m.Category).Distinct().Count()
            };
        }

        /// <summary>
        /// Saves memories to disk
        /// </summary>
        private void SaveMemories()
        {
            try
            {
                var directory = Path.GetDirectoryName(_memoriesPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(_memories, Formatting.Indented);
                File.WriteAllText(_memoriesPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save memories: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads memories from disk
        /// </summary>
        private void LoadMemories()
        {
            try
            {
                if (File.Exists(_memoriesPath))
                {
                    var json = File.ReadAllText(_memoriesPath);
                    _memories = JsonConvert.DeserializeObject<List<Memory>>(json) ?? new List<Memory>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load memories: {ex.Message}");
                _memories = new List<Memory>();
            }
        }

        /// <summary>
        /// Exports memories to a file
        /// </summary>
        public void ExportMemories(string filePath)
        {
            var json = JsonConvert.SerializeObject(_memories, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Imports memories from a file
        /// </summary>
        public void ImportMemories(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var importedMemories = JsonConvert.DeserializeObject<List<Memory>>(json);
            if (importedMemories != null)
            {
                // Avoid duplicates by checking if a memory with the same ID already exists
                var existingIds = new HashSet<string>(_memories.Select(m => m.Id));
                
                foreach (var memory in importedMemories)
                {
                    if (!existingIds.Contains(memory.Id))
                    {
                        _memories.Add(memory);
                    }
                }
                
                SaveMemories();
            }
        }
    }

    /// <summary>
    /// Statistics about the memory system
    /// </summary>
    public class MemoryStats
    {
        public int TotalMemories { get; set; }
        public double AverageImportance { get; set; }
        public DateTime OldestMemory { get; set; }
        public DateTime NewestMemory { get; set; }
        public int CategoriesCount { get; set; }
    }
}
