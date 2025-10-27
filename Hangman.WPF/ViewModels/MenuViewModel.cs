using Hangman.Core.Models;
using System.Collections.Generic;
using System.Windows.Input;

namespace Hangman.WPF.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;

        public GameSettings CurrentSettings { get; set; }

        // Listor för ComboBoxes
        public IEnumerable<WordDifficulty> Difficulties => System.Enum.GetValues<WordDifficulty>();
        // ÄNDRAD: Denna property är nu en Dictionary som mappar enum-värdet till en sträng
        public Dictionary<WordSource, string> WordSources { get; } = new()
{
        // Vi hämtar inspiration från SwedishUiStrings.cs
        { WordSource.Api, "Engelska (API)" },
        { WordSource.Local, "Svenska (Lokal)" },
        { WordSource.CustomSwedish, "Anpassad Ordlista (Svenska)" },
        { WordSource.CustomEnglish, "Anpassad Ordlista (Engelska)" }
};

        public ICommand StartSinglePlayerCommand { get; }
        public ICommand NavigateToHighscoresCommand { get; }
        public ICommand NavigateToAddWordCommand { get; }

        public MenuViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            CurrentSettings = new GameSettings();

            StartSinglePlayerCommand = new RelayCommand(_ => _mainViewModel.NavigateToGame(CurrentSettings));
            NavigateToHighscoresCommand = new RelayCommand(_ => _mainViewModel.NavigateToHighscores());
            NavigateToAddWordCommand = new RelayCommand(_ => _mainViewModel.NavigateToAddWord());
        }
    }
}