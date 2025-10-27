using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Models;
using Hangman.Core.Providers.Interface;
using Microsoft.EntityFrameworkCore;

namespace Hangman.Core.Providers.Db
{
    /// <summary>
    /// Implementerar IStatisticsService genom att spara highscores i en SQLite-databas 
    /// med hjälp av Entity Framework Core.
    /// </summary>
    public class SqliteHangmanService : IStatisticsService
    {
        // Skapa en ny kontext-instans vid varje operation (bra för webb/konsolapplikationer utan DI)
        private HangmanDbContext CreateContext() => new HangmanDbContext();

        public async Task SaveHighscoreAsync(HighscoreEntry newScore)
        {
            if (newScore.ConsecutiveWins <= 0) return;

            using (var context = CreateContext())
            {
                // FIX: Ändrat från .Equals(..., StringComparison.OrdinalIgnoreCase) till .ToLower() == .ToLower()
                // Detta kan översättas till SQL, vilket löser felet.
                var existingScore = await context.Highscores.FirstOrDefaultAsync(s =>
                    s.PlayerName.ToLower() == newScore.PlayerName.ToLower() &&
                    s.Difficulty == newScore.Difficulty);

                bool saved = false; // Håll koll på om vi faktiskt sparade något

                if (existingScore != null)
                {
                    if (newScore.ConsecutiveWins > existingScore.ConsecutiveWins)
                    {
                        existingScore.ConsecutiveWins = newScore.ConsecutiveWins;
                        // Nollställ ID för att säkerställa att vi inte försöker lägga till en ny post med samma ID
                        newScore.Id = existingScore.Id;
                        context.Highscores.Update(existingScore);
                        await context.SaveChangesAsync();
                        saved = true;
                    }
                }
                else
                {
                    // NY: Lägg till den nya posten
                    context.Highscores.Add(newScore);
                    await context.SaveChangesAsync();
                    saved = true;
                }

                // NYTT: Rensa listan om vi har lagt till/uppdaterat något
                if (saved)
                {
                    await PruneScoresAsync(context, newScore.Difficulty, 10);
                }
            }
        }

        /// <summary>
        /// NY PRIVAT HJÄLPMETOD: Ser till att endast de X bästa poängen
        /// för en given svårighetsgrad finns kvar i databasen.
        /// </summary>
        private async Task PruneScoresAsync(HangmanDbContext context, WordDifficulty difficulty, int topN)
        {
            // Hämta alla poäng för denna svårighetsgrad, sorterade
            var scores = await context.Highscores
                .Where(s => s.Difficulty == difficulty)
                .OrderByDescending(s => s.ConsecutiveWins)
                .ToListAsync();

            // Om det finns fler än 'topN' poäng...
            if (scores.Count > topN)
            {
                // ...ta bort alla som hamnar utanför topplistan (dvs. hoppa över de 'topN' bästa)
                var scoresToRemove = scores.Skip(topN);
                context.Highscores.RemoveRange(scoresToRemove);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<HighscoreEntry>> GetHighscoresAsync(WordDifficulty difficulty)
        {
            using (var context = CreateContext())
            {
                return await context.Highscores
                    .AsNoTracking() // Vi behöver inte spåra ändringar här
                    .Where(s => s.Difficulty == difficulty)
                    .OrderByDescending(s => s.ConsecutiveWins)
                    .ToListAsync();
            }
        }

        public async Task<List<HighscoreEntry>> GetGlobalTopScoresAsync(int topN = 5)
        {
            using (var context = CreateContext())
            {
                var topScores = new List<HighscoreEntry>();

                // Använder Enum.GetValues() för att loopa genom alla WordDifficulty-värden
                foreach (WordDifficulty difficulty in Enum.GetValues<WordDifficulty>())
                {
                    var topForDifficulty = await context.Highscores
                        .AsNoTracking()
                        .Where(s => s.Difficulty == difficulty)
                        .OrderByDescending(s => s.ConsecutiveWins)
                        .Take(topN)
                        .ToListAsync();

                    topScores.AddRange(topForDifficulty);
                }

                return topScores;
            }
        }
    }
}