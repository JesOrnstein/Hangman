using Hangman.Core.Models;
using System.Collections.Generic;
using System.Windows.Input;
using Hangman.Core.Localizations; // <-- NY USING

namespace Hangman.WPF.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;

        // NY PROPERTY: Exponera strängarna för XAML-bindning
        public LocalizationProvider Strings { get; }

        public ICommand StartSinglePlayerCommand { get; }
        public ICommand NavigateToHighscoresCommand { get; }
        public ICommand NavigateToAddWordCommand { get; }
        public ICommand StartTournamentCommand { get; }
        public ICommand NavigateToHelpCommand { get; }

        // UPPDATERAD KONSTRUKTOR
        public MenuViewModel(MainViewModel mainViewModel, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            Strings = strings; // Spara instansen

            // --- KOMMANDON ÄR UPPDATERADE ---
            // Använder den nya navigeringsmetoden
            StartSinglePlayerCommand = new RelayCommand(_ => _mainViewModel.NavigateToGameSettings(GameMode.SinglePlayer));
            StartTournamentCommand = new RelayCommand(_ => _mainViewModel.NavigateToGameSettings(GameMode.Tournament));
            // ---

            NavigateToHighscoresCommand = new RelayCommand(_ => _mainViewModel.NavigateToHighscores());
            NavigateToAddWordCommand = new RelayCommand(_ => _mainViewModel.NavigateToAddWord());
            NavigateToHelpCommand = new RelayCommand(_ => _mainViewModel.NavigateToHelp());
        }
    }
}