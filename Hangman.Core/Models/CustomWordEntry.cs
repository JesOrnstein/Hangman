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
        public required string Word { get; set; }

        public required WordDifficulty Difficulty { get; set; }

        // Löser CS8618/CS9035 för JSON deserialisering
        [SetsRequiredMembers]
        public CustomWordEntry()
        {
            // Provide non-null defaults to satisfy the compiler.
            // JSON deserializers will overwrite these values when materializing the object.
            Word = string.Empty;
            Difficulty = default;
        }

        // Denna konstruktor används i CustomWordProvider.cs:AddWordAsync
        public CustomWordEntry(string word, WordDifficulty difficulty)
        {
            Word = word.ToUpperInvariant();
            Difficulty = difficulty;
        }
    }
}