using Hangman.Core.Models;
using System.Collections.Generic;
using System.Windows.Input;

namespace Hangman.WPF.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;

        public ICommand StartSinglePlayerCommand { get; }
        public ICommand NavigateToHighscoresCommand { get; }
        public ICommand NavigateToAddWordCommand { get; }
        public ICommand StartTournamentCommand { get; }
        public ICommand NavigateToHelpCommand { get; }

        public MenuViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

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