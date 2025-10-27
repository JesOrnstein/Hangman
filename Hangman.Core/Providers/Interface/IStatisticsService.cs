using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Models;

namespace Hangman.Core.Providers.Interface
{
    /// <summary>
    /// Definierar kontraktet för hantering av highscores (sparar, hämtar).
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// Sparar ett highscore. Implementationen ska avgöra om den nya posten är bättre än den gamla.
        /// </summary>
        /// <param name="newScore">Den nya highscore-posten att utvärdera och spara.</param>
        Task SaveHighscoreAsync(HighscoreEntry newScore);

        /// <summary>
        /// Hämtar de bästa highscoresen för en specifik svårighetsgrad, sorterade efter vinster i fallande ordning.
        /// </summary>
        /// <param name="difficulty">Svårighetsgraden att filtrera på.</param>
        /// <returns>En lista med HighscoreEntry.</returns>
        Task<List<HighscoreEntry>> GetHighscoresAsync(WordDifficulty difficulty);

        /// <summary>
        /// Hämtar de N bästa resultaten för alla svårighetsgrader.
        /// </summary>
        /// <param name="topN">Antal poster att returnera per svårighetsgrad.</param>
        /// <returns>En platt lista med toppresultat.</returns>
        Task<List<HighscoreEntry>> GetGlobalTopScoresAsync(int topN = 5);
    }
}
