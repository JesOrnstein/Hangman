using Hangman.Core;
using Hangman.Core.Models;
using Hangman.Core.Providers.Api;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Providers.Local;
using System.Windows.Input;
using Hangman.Core.Localizations; // <-- NY USING

namespace Hangman.WPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // KORRIGERING: Vi initierar med 'default!' för att lösa CS8618-varningen.
        private BaseViewModel _currentViewModel = default!;
        private readonly IStatisticsService _statisticsService;

        // NY PROPERTY: Exponerar vår LocalizationProvider
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

        // UPPDATERAD KONSTRUKTOR: Tar nu emot LocalizationProvider
        public MainViewModel(IStatisticsService statisticsService, LocalizationProvider localizationProvider)
        {
            _statisticsService = statisticsService;
            Strings = localizationProvider; // Spara instansen

            // --- ÄNDRA DENNA RAD ---
            // Före: CurrentViewModel = new MenuViewModel(this, Strings);
            // Efter:
            CurrentViewModel = new LanguageSelectionViewModel(this, Strings);
        }

        // --- Navigationsmetoder som anropas av andra ViewModels ---
        // (Alla metoder skickar nu med 'Strings' där det behövs)

        public void NavigateToMenu()
        {
            CurrentViewModel = new MenuViewModel(this, Strings);
        }

        public void NavigateToHighscores()
        {
            // HighscoreViewModel behöver ingen dynamisk text just nu,
            // men vi kan skicka med den om vi vill.
            CurrentViewModel = new HighscoreViewModel(this, _statisticsService);
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

        // ---

        // --- NY METOD FÖR STEG 1 ---
        public void NavigateToTournament(GameSettings settings)
        {
            IAsyncWordProvider provider = CreateProvider(settings);
            CurrentViewModel = new TournamentViewModel(this, provider, settings, Strings);
        }
        // ---

        public void NavigateToGame(GameSettings settings)
        {
            IAsyncWordProvider provider = CreateProvider(settings);

            // Just nu stöder vi bara Single Player-läge i denna implementation
            // (Tournament-logiken skulle behöva en egen TwoPlayerGameViewModel)

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