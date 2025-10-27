using Hangman.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hangman.Core.Models
{
    public class CustomWordEntry
    {
        public int Id { get; set; } // Primärnyckel för databasen

        public required string Word { get; set; }

        public required WordDifficulty Difficulty { get; set; }

        public required WordLanguage Language { get; set; } // NYTT: Språk

        [SetsRequiredMembers]
        public CustomWordEntry()
        {
            Word = string.Empty;
            Difficulty = default;
            Language = default; // NYTT
        }

        public CustomWordEntry(string word, WordDifficulty difficulty, WordLanguage language)
        {
            Word = word.ToUpperInvariant();
            Difficulty = difficulty;
            Language = language; // NYTT
        }
    }
}