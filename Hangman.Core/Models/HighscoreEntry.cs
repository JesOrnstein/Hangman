using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Core.Models
{
    public class HighscoreEntry
    {
        public int Id { get; set; }

        public required string PlayerName { get; set; }
        public required int ConsecutiveWins { get; set; }
        public required WordDifficulty Difficulty { get; set; }

        [SetsRequiredMembers]
        public HighscoreEntry()
        {
            PlayerName = string.Empty;
            ConsecutiveWins = 0;
            Difficulty = default(WordDifficulty);
        }

        public HighscoreEntry(string playerName, int consecutiveWins, WordDifficulty difficulty)
        {
            PlayerName = playerName;
            ConsecutiveWins = consecutiveWins;
            Difficulty = difficulty;
        }
    }
}