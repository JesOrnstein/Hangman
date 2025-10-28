using Hangman.Core.Models;
using System.Collections.Generic;
using System.Windows.Input;
using Hangman.Core.Localizations;

namespace Hangman.WPF.ViewModels
{
    public class GameSettingsViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly GameMode _gameMode;

        public LocalizationProvider Strings { get; }

        public GameSettings CurrentSettings { get; set; }

        public bool IsTournamentMode { get; }

        // NY IMPLEMENTATION: Returnerar KeyValuePair<WordDifficulty, string> för lokaliserade namn
        public IEnumerable<KeyValuePair<WordDifficulty, string>> LocalizedDifficulties
        {
            get
            {
                yield return new KeyValuePair<WordDifficulty, string>(WordDifficulty.Easy, Strings.FeedbackDifficultyEasy);
                yield return new KeyValuePair<WordDifficulty, string>(WordDifficulty.Medium, Strings.FeedbackDifficultyMedium);
                yield return new KeyValuePair<WordDifficulty, string>(WordDifficulty.Hard, Strings.FeedbackDifficultyHard);
            }
        }

        public Dictionary<WordSource, string> WordSources { get; } = new()
        {
            { WordSource.Api, "Engelska (API)" },
            { WordSource.Local, "Svenska (Lokal)" },
            { WordSource.CustomSwedish, "Anpassad Ordlista (Svenska)" },
            { WordSource.CustomEnglish, "Anpassad Ordlista (Engelska)" }
        };

        public ICommand StartGameCommand { get; }
        public ICommand BackToMenuCommand { get; }

        public GameSettingsViewModel(MainViewModel mainViewModel, GameMode mode, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _gameMode = mode;
            IsTournamentMode = (mode == GameMode.Tournament);
            Strings = strings;

            CurrentSettings = new GameSettings();

            if (IsTournamentMode)
            {
                CurrentSettings.PlayerName = Strings.DefaultPlayer1Name;
                CurrentSettings.PlayerName2 = Strings.DefaultPlayer2Name;
            }
            else
            {
                CurrentSettings.PlayerName = Strings.DefaultPlayerName;
            }

            StartGameCommand = new RelayCommand(StartGame);
            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());
        }

        private void StartGame(object? _)
        {
            if (string.IsNullOrWhiteSpace(CurrentSettings.PlayerName))
            {
                CurrentSettings.PlayerName = "Spelare 1";
            }
            if (IsTournamentMode && string.IsNullOrWhiteSpace(CurrentSettings.PlayerName2))
            {
                CurrentSettings.PlayerName2 = "Spelare 2";
            }

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