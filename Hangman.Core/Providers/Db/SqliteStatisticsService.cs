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
    public class SqliteStatisticsService : IStatisticsService
    {
        // Skapa en ny kontext-instans vid varje operation (bra för webb/konsolapplikationer utan DI)
        private StatisticsDbContext CreateContext() => new StatisticsDbContext();

        public async Task SaveHighscoreAsync(HighscoreEntry newScore)
        {
            if (newScore.ConsecutiveWins <= 0) return;

            using (var context = CreateContext())
            {
                // Hitta befintlig post baserat på namn och svårighetsgrad
                var existingScore = await context.Highscores.FirstOrDefaultAsync(s =>
                    s.PlayerName.Equals(newScore.PlayerName, StringComparison.OrdinalIgnoreCase) &&
                    s.Difficulty == newScore.Difficulty);

                if (existingScore != null)
                {
                    // UPPDATERING: Om den nya poängen är bättre, uppdatera den
                    if (newScore.ConsecutiveWins > existingScore.ConsecutiveWins)
                    {
                        existingScore.ConsecutiveWins = newScore.ConsecutiveWins;
                        // Nollställ ID för att säkerställa att vi inte försöker lägga till en ny post med samma ID
                        newScore.Id = existingScore.Id;
                        context.Highscores.Update(existingScore);
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    // NY: Lägg till den nya posten
                    context.Highscores.Add(newScore);
                    await context.SaveChangesAsync();
                }
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
