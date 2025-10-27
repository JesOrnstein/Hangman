using Hangman.Core.Providers.Interface;
using Hangman.Core.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hangman.Core
{
    /// Hämtar slumpmässiga svenska ord från en intern, lokal array.
    /// Ordlistan filtreras baserat på den valda <see cref="WordDifficulty"/>.
    public sealed class WordProvider : IAsyncWordProvider
    {
        // 1. Din ordlista med svenska ord
        private static readonly string[] _words =
        {
            // Lätta (3-4 bokstäver)
            "IS", "BIL", "MUS", "BOK",
            // Medium (5-7 bokstäver)
            "BANAN", "DATOR", "KATT", "FÅGEL", "PENNA",
            // Svåra (8-11 bokstäver)
            "PROGRAMMERING", "ELEFANT", "ROBOTT", "KLAVERSPEL",
            "SOCKERDRICKA", "VISUALSTUDIO", "HÄNGMAN", "DATABAS",
            "UTVECKLING", "ARKITEKTUR", "GRÄNSSNITT", "BOKHYLLA"
        };

        // 2. Random-generator
        private readonly Random _rng = new Random();

        // 3. Svårighetsgrad
        private readonly WordDifficulty _difficulty;

        // 4. Konstruktor för att ta emot svårighetsgrad
        public WordProvider(WordDifficulty difficulty)
        {
            _difficulty = difficulty;
        }

        // 5. DifficultyName kan nu reflektera det valda läget
        public string DifficultyName => $"Svensk Lokal ({_difficulty})";

        // 6. Returnera ett slumpmässigt ord från listan, filtrerat efter svårighetsgrad
        public Task<string> GetWordAsync(CancellationToken ct = default)
        {
            // Bestäm ordlängd baserat på svårighetsgrad (återanvänder samma intervall som API)
            int minLength, maxLength;

            switch (_difficulty)
            {
                case WordDifficulty.Easy:
                    minLength = 3;
                    maxLength = 4;
                    break;

                case WordDifficulty.Medium:
                    minLength = 5;
                    maxLength = 7;
                    break;

                case WordDifficulty.Hard:
                    minLength = 8;
                    maxLength = 11;
                    break;

                default: // Fallback
                    minLength = 5;
                    maxLength = 7;
                    break;
            }

            // Filtrera ordlistan baserat på längd
            var availableWords = _words
                .Where(w => w.Length >= minLength && w.Length <= maxLength)
                .ToArray();

            if (availableWords.Length == 0)
                throw new InvalidOperationException($"Hittade inga ord i den lokala listan för längd {minLength}-{maxLength}.");

            int index = _rng.Next(availableWords.Length); // slumpad index

            // Returnera resultatet som en komplett Task, eftersom arbetet är synkront
            return Task.FromResult(availableWords[index]);
        }
    }
}