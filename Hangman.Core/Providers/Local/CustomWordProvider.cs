using Hangman.Core.Models;
using Hangman.Core.Providers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Providers.Db;
using Microsoft.EntityFrameworkCore;

namespace Hangman.Core.Providers.Local
{
    /// <summary>
    /// Hämtar och sparar anpassade ord till SQLite-databasen.
    /// Använder HangmanDbContext.
    /// </summary>
    public sealed class CustomWordProvider : IAsyncWordProvider
    {

        private readonly Random _rng = new Random();
        private readonly WordDifficulty _difficulty;

        public CustomWordProvider(WordDifficulty difficulty)
        {
            _difficulty = difficulty;
        }

        public string DifficultyName => $"Anpassad Ordlista ({_difficulty})";

        /// <summary>
        /// Sparar ett nytt ord till den anpassade ordlistan i databasen.
        /// </summary>
        public async Task AddWordAsync(string word, WordDifficulty difficulty)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentException("Ordet får inte vara tomt.", nameof(word));

            // Skapa posten. Konstruktorn gör ordet till VERSALER.
            var newEntry = new CustomWordEntry(word, difficulty)
            {
                Word = word,
                Difficulty = difficulty
            };

            using (var context = new HangmanDbContext())
            {
                // Vi kollar här för att ge ett bättre felmeddelande än DbUpdateException
                if (await context.CustomWords.AnyAsync(e => e.Word == newEntry.Word && e.Difficulty == newEntry.Difficulty))
                {
                    throw new InvalidOperationException($"Ordet '{word}' finns redan i listan för svårighetsgrad {difficulty}.");
                }

                context.CustomWords.Add(newEntry);
                await context.SaveChangesAsync();
            }
        }

        public async Task<string> GetWordAsync(CancellationToken ct = default)
        {
            List<string> availableWords;

            // Hämta från databasen istället för JSON-fil
            using (var context = new HangmanDbContext())
            {
                // Filtrera på vald svårighetsgrad i databasfrågan
                availableWords = await context.CustomWords
                    .AsNoTracking() // Vi behöver inte spåra ändringar här
                    .Where(w => w.Difficulty == _difficulty)
                    .Select(w => w.Word)
                    .ToListAsync(ct);
            }

            if (!availableWords.Any())
            {
                throw new InvalidOperationException($"Hittade inga anpassade ord i listan för svårighetsgrad {_difficulty}. Lägg till ord via menyn Inställningar/Verktyg.");
            }

            int index = _rng.Next(availableWords.Count);
            return availableWords[index];
        }
    }
}