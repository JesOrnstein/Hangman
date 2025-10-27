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
        public int Id { get; set; } // Primärnyckel

        public required string PlayerName { get; set; }
        public required int ConsecutiveWins { get; set; }
        public required WordDifficulty Difficulty { get; set; }

        // Plan (pseudocode):
        // 1. Problem: CS8618 - required properties are not set by the parameterless constructor.
        // 2. Solution: Ensure the parameterless constructor sets non-null default values for all required properties.
        // 3. Mark the constructor with [SetsRequiredMembers] to inform the compiler that required members are initialized.
        // 4. Set: PlayerName = string.Empty; ConsecutiveWins = 0; Difficulty = default(WordDifficulty);
        // 5. Keep the other constructor (that sets real values) unchanged.

        // Löser CS8618/CS9035 för EF Core och deserialisering
        [SetsRequiredMembers]
        public HighscoreEntry()
        {
            PlayerName = string.Empty;
            ConsecutiveWins = 0;
            Difficulty = default(WordDifficulty);
        }

        // Denna konstruktor används i ConsoleUi.cs och sätter alla required-fält
        public HighscoreEntry(string playerName, int consecutiveWins, WordDifficulty difficulty)
        {
            PlayerName = playerName;
            ConsecutiveWins = consecutiveWins;
            Difficulty = difficulty;
        }
    }
}
