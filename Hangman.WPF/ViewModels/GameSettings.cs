using Hangman.Core.Models;

namespace Hangman.WPF.ViewModels
{
    // NY ENUM för att spåra spelläge
    public enum GameMode
    {
        SinglePlayer,
        Tournament
    }

    // En simple dataklass för att skicka val från menyn
    public class GameSettings
    {
        public string PlayerName { get; set; } = "Player 1";
        public string PlayerName2 { get; set; } = "Player 2"; // NY
        public WordDifficulty Difficulty { get; set; } = WordDifficulty.Medium;
        public WordSource Source { get; set; } = WordSource.Api;
    }

    public enum WordSource
    {
        Api,
        Local,
        CustomSwedish,
        CustomEnglish
    }
}