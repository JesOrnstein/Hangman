using Hangman.Core;
using Hangman.Core.Models;
using Hangman.Core.Providers.Api;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Providers.Local;
using System.Windows.Input;

namespace Hangman.WPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // KORRIGERING: Vi initierar med 'default!' för att lösa CS8618-varningen.
        private BaseViewModel _currentViewModel = default!;
        private readonly IStatisticsService _statisticsService;

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;

            // Starta på huvudmenyn
            CurrentViewModel = new MenuViewModel(this);
        }

        // --- Navigationsmetoder som anropas av andra ViewModels ---

        public void NavigateToMenu()
        {
            CurrentViewModel = new MenuViewModel(this);
        }

        public void NavigateToHighscores()
        {
            CurrentViewModel = new HighscoreViewModel(this, _statisticsService);
        }

        public void NavigateToAddWord()
        {
            CurrentViewModel = new AddWordViewModel(this);
        }

        // --- NY METOD FÖR STEG 2 ---
        public void NavigateToHelp()
        {
            CurrentViewModel = new HelpViewModel(this);
        }

        // --- NY METOD FÖR STEG 1 ---
        public void NavigateToTournament(GameSettings settings)
        {
            IAsyncWordProvider provider = CreateProvider(settings);
            CurrentViewModel = new TournamentViewModel(this, provider, settings);
        }
        // ---

        public void NavigateToGame(GameSettings settings)
        {
            IAsyncWordProvider provider = CreateProvider(settings);

            // Just nu stöder vi bara Single Player-läge i denna implementation
            // (Tournament-logiken skulle behöva en egen TwoPlayerGameViewModel)

            CurrentViewModel = new GameViewModel(this, provider, _statisticsService, settings.PlayerName);
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