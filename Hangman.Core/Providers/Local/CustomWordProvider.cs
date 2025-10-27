using Hangman.Core.Exceptions;
using Hangman.Core.Models;
using Hangman.Core.Providers.Db;
using Hangman.Core.Providers.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly WordLanguage _language; // NYTT

        public CustomWordProvider(WordDifficulty difficulty, WordLanguage language)
        {
            _difficulty = difficulty;
            _language = language; // NYTT
        }

        // NYTT: Visar nu även språk
        public string DifficultyName => $"Anpassad Ordlista ({_language} - {_difficulty})";

        /// <summary>
        /// Sparar ett nytt ord till den anpassade ordlistan i databasen.
        /// </summary>
        public async Task AddWordAsync(string word, WordDifficulty difficulty, WordLanguage language)
        {
            if (string.IsNullOrWhiteSpace(word))
                // ÄNDRAD: Generiskt meddelande för loggning/intern användning
                throw new ArgumentException("Word cannot be null or whitespace.", nameof(word));

            // Skapa posten. Konstruktorn gör ordet till VERSALER.
            var newEntry = new CustomWordEntry(word, difficulty, language)
            {
                Word = word,
                Difficulty = difficulty,
                Language = language // NYTT
            };

            using (var context = new HangmanDbContext())
            {
                // Vi kollar här för att ge ett bättre felmeddelande än DbUpdateException
                // NYTT: Kollar även 'Language'
                if (await context.CustomWords.AnyAsync(e =>
                    e.Word == newEntry.Word &&
                    e.Difficulty == newEntry.Difficulty &&
                    e.Language == newEntry.Language))
                {
                    // ÄNDRAD: Kasta ditt nya, specifika undantag med data
                    throw new WordAlreadyExistsException(newEntry.Word, newEntry.Difficulty, newEntry.Language);
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
                // Filtrera på vald svårighetsgrad OCH SPRÅK i databasfrågan
                availableWords = await context.CustomWords
                    .AsNoTracking() // Vi behöver inte spåra ändringar här
                    .Where(w => w.Difficulty == _difficulty && w.Language == _language) // MODIFIERAD
                    .Select(w => w.Word)
                    .ToListAsync(ct);
            }

            if (!availableWords.Any())
            {
                // ÄNDRAD: Kasta ditt andra nya, specifika undantag med data
                throw new NoCustomWordsFoundException(_difficulty, _language);
            }

            int index = _rng.Next(availableWords.Count);
            return availableWords[index];
        }
    }
}