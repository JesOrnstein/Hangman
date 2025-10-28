using Hangman.Core.Providers.Interface;
using Hangman.Core.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hangman.Core
{
    /// <summary>
    /// Hämtar slumpmässiga svenska ord från en intern, lokal array.
    /// Ordlistan filtreras baserat på den valda <see cref="WordDifficulty"/>.
    /// </summary>
    public sealed class WordProvider : IAsyncWordProvider
    {
        private static readonly string[] _words =
        {
            "IS", "BIL", "MUS", "BOK",
            "BANAN", "DATOR", "KATT", "FÅGEL", "PENNA",
            "PROGRAMMERING", "ELEFANT", "ROBOTT", "KLAVERSPEL",
            "SOCKERDRICKA", "VISUALSTUDIO", "HÄNGMAN", "DATABAS",
            "UTVECKLING", "ARKITEKTUR", "GRÄNSSNITT", "BOKHYLLA"
        };

        private readonly Random _rng = new Random();
        private readonly WordDifficulty _difficulty;

        public WordProvider(WordDifficulty difficulty)
        {
            _difficulty = difficulty;
        }

        public string DifficultyName => $"Svensk Lokal ({_difficulty})";

        // IMPLEMENTATION: Nu publik och läsbar via interfacet
        public WordDifficulty Difficulty => _difficulty;

        /// <summary>
        /// Returnerar ett slumpmässigt ord från listan, filtrerat efter svårighetsgrad.
        /// </summary>
        public Task<string> GetWordAsync(CancellationToken ct = default)
        {
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

                default:
                    minLength = 5;
                    maxLength = 7;
                    break;
            }

            var availableWords = _words
                .Where(w => w.Length >= minLength && w.Length <= maxLength)
                .ToArray();

            if (availableWords.Length == 0)
                throw new InvalidOperationException($"Hittade inga ord i den lokala listan för längd {minLength}-{maxLength}.");

            int index = _rng.Next(availableWords.Length);

            return Task.FromResult(availableWords[index]);
        }
    }
}