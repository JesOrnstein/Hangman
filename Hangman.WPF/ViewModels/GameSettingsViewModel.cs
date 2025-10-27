using Hangman.Core.Models;
using System.Collections.Generic;
using System.Windows.Input;

namespace Hangman.WPF.ViewModels
{
    // NY FIL
    public class GameSettingsViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly GameMode _gameMode;

        // --- Egenskaper för inställningar ---

        // Håller de val som görs i UI:t
        public GameSettings CurrentSettings { get; set; }

        // Används för att visa/dölja "Spelare 2"-fältet
        public bool IsTournamentMode { get; }

        // --- Listor för ComboBoxes (Flyttade från MenuViewModel) ---
        public IEnumerable<WordDifficulty> Difficulties => System.Enum.GetValues<WordDifficulty>();
        public Dictionary<WordSource, string> WordSources { get; } = new()
        {
            { WordSource.Api, "Engelska (API)" },
            { WordSource.Local, "Svenska (Lokal)" },
            { WordSource.CustomSwedish, "Anpassad Ordlista (Svenska)" },
            { WordSource.CustomEnglish, "Anpassad Ordlista (Engelska)" }
        };

        // --- Kommandon ---
        public ICommand StartGameCommand { get; }
        public ICommand BackToMenuCommand { get; }

        public GameSettingsViewModel(MainViewModel mainViewModel, GameMode mode)
        {
            _mainViewModel = mainViewModel;
            _gameMode = mode;
            IsTournamentMode = (mode == GameMode.Tournament);

            // Skapa standardinställningar
            CurrentSettings = new GameSettings();

            // Sätt en titel baserat på läget
            if (IsTournamentMode)
            {
                CurrentSettings.PlayerName = "Spelare 1";
            }
            else
            {
                CurrentSettings.PlayerName = "Spelare";
            }

            // Definiera kommandona
            StartGameCommand = new RelayCommand(StartGame);
            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());
        }

        private void StartGame(object? _)
        {
            // Validera att namn inte är tomma (kan göras mer robust)
            if (string.IsNullOrWhiteSpace(CurrentSettings.PlayerName))
            {
                CurrentSettings.PlayerName = "Spelare 1";
            }
            if (IsTournamentMode && string.IsNullOrWhiteSpace(CurrentSettings.PlayerName2))
            {
                CurrentSettings.PlayerName2 = "Spelare 2";
            }

            // Anropa rätt navigationsmetod på MainViewModel
            if (IsTournamentMode)
            {
                _mainViewModel.NavigateToTournament(CurrentSettings);
            }
            else
            {
                _mainViewModel.NavigateToGame(CurrentSettings);
            }
        }
    }
}