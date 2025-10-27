using Hangman.Core.Models;
using Hangman.Core.Providers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hangman.Core.Providers.Local
{
    /// <summary>
    /// Hämtar och sparar anpassade ord till en lokal JSON-fil.
    /// Använder AppContext.BaseDirectory för att spara i den körbara mappen.
    /// </summary>
    public sealed class CustomWordProvider : IAsyncWordProvider
    {
        private const string FileName = "customwords.json";
        private readonly string _filePath;
        private readonly Random _rng = new Random();
        private readonly WordDifficulty _difficulty;

        public CustomWordProvider(WordDifficulty difficulty)
        {
            _difficulty = difficulty;

            // Spara filen i samma AppContext.BaseDirectory som SQLite-databasen
            string baseDir = AppContext.BaseDirectory;
            _filePath = Path.Combine(baseDir, FileName);
        }

        public string DifficultyName => $"Anpassad Ordlista ({_difficulty})";

        /*
        PSEUDOCODE / PLAN (step-by-step):
        1. Validate input 'word' is not null/whitespace; throw ArgumentException with parameter name if invalid.
        2. Create a new CustomWordEntry instance and ensure C# 'required' members are set:
           - Use the existing constructor (if any) and also set the required properties via an object initializer,
             or use an object initializer alone. This satisfies the CS9035 compiler rule.
        3. Load all existing custom words from storage via LoadAllWordsAsync().
        4. Check for duplicates by comparing words case-insensitively.
           - If duplicate exists, throw InvalidOperationException.
        5. Add the new entry to the list and persist using SaveAllWordsAsync().
        6. Keep method async and return a completed Task.

        This replaces the problematic line that caused CS9035 by explicitly setting the required members.
        */

        /// <summary>
        /// Sparar ett nytt ord till den anpassade ordlistan.
        /// </summary>
        public async Task AddWordAsync(string word, WordDifficulty difficulty)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentException("Ordet får inte vara tomt.", nameof(word));

            // Skapa posten och sätt uttryckligen de required-medlemmarna så att CS9035 försvinner.
            // Behåll konstruktorn (om den gör andra initialiseringar) och sätt krävda properties via objektinitierare.
            var newEntry = new CustomWordEntry(word, difficulty)
            {
                Word = word,
                Difficulty = difficulty
            };

            var allWords = await LoadAllWordsAsync();

            // Undvik dubbletter
            if (allWords.Any(e => e.Word.Equals(newEntry.Word, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Ordet '{word}' finns redan i den anpassade listan.");
            }

            allWords.Add(newEntry);
            await SaveAllWordsAsync(allWords);
        }

        private async Task<List<CustomWordEntry>> LoadAllWordsAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<CustomWordEntry>();
            }

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                return JsonSerializer.Deserialize<List<CustomWordEntry>>(json) ?? new List<CustomWordEntry>();
            }
            catch
            {
                // Returnera en tom lista vid deserialiseringsfel
                return new List<CustomWordEntry>();
            }
        }

        private async Task SaveAllWordsAsync(List<CustomWordEntry> words)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(words, options);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<string> GetWordAsync(CancellationToken ct = default)
        {
            var allWords = await LoadAllWordsAsync();

            // Filtrera på vald svårighetsgrad
            var availableWords = allWords
                .Where(w => w.Difficulty == _difficulty)
                .Select(w => w.Word)
                .ToList();

            if (!availableWords.Any())
            {
                throw new InvalidOperationException($"Hittade inga anpassade ord i listan för svårighetsgrad {_difficulty}. Lägg till ord via menyn Inställningar/Verktyg.");
            }

            int index = _rng.Next(availableWords.Count);
            return availableWords[index];
        }
    }
}