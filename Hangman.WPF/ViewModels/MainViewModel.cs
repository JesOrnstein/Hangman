using Hangman.Core;
using Hangman.Core.Models;
using Hangman.Core.Providers.Api;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Providers.Local;
using System.Windows.Input;
using Hangman.Core.Localizations;

namespace Hangman.WPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel = default!;
        private readonly IStatisticsService _statisticsService;

        public LocalizationProvider Strings { get; }

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel(IStatisticsService statisticsService, LocalizationProvider localizationProvider)
        {
            _statisticsService = statisticsService;
            Strings = localizationProvider;

            CurrentViewModel = new LanguageSelectionViewModel(this, Strings);
        }

        public void NavigateToMenu()
        {
            CurrentViewModel = new MenuViewModel(this, Strings);
        }

        public void NavigateToHighscores()
        {
            CurrentViewModel = new HighscoreViewModel(this, _statisticsService, Strings);
        }

        public void NavigateToAddWord()
        {
            CurrentViewModel = new AddWordViewModel(this, Strings);
        }

        public void NavigateToHelp()
        {
            CurrentViewModel = new HelpViewModel(this, Strings);
        }

        public void NavigateToGameSettings(GameMode mode)
        {
            CurrentViewModel = new GameSettingsViewModel(this, mode, Strings);
        }

        public void NavigateToTournament(GameSettings settings)
        {
            IAsyncWordProvider provider = CreateProvider(settings);
            CurrentViewModel = new TournamentViewModel(this, provider, settings, Strings);
        }

        public void NavigateToGame(GameSettings settings)
        {
            IAsyncWordProvider provider = CreateProvider(settings);
            CurrentViewModel = new GameViewModel(this, provider, _statisticsService, settings.PlayerName, Strings);
        }

        private IAsyncWordProvider CreateProvider(GameSettings settings)
        {
            switch (settings.Source)
            {
                case WordSource.Api:
                    return new ApiWordProvider(settings.Difficulty);
                case WordSource.Local:
                    return new WordProvider(settings.Difficulty);
                case WordSource.CustomSwedish:
                    return new CustomWordProvider(settings.Difficulty, WordLanguage.Swedish);
                case WordSource.CustomEnglish:
                    return new CustomWordProvider(settings.Difficulty, WordLanguage.English);
                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}