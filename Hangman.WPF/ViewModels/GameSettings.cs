using Hangman.Core.Models;

namespace Hangman.WPF.ViewModels
{
    public enum GameMode
    {
        SinglePlayer,
        Tournament
    }

    public class GameSettings
    {
        public string PlayerName { get; set; } = "Player 1";
        public string PlayerName2 { get; set; } = "Player 2";
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