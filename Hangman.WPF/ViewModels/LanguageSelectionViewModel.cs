using Hangman.Core.Localizations;
using System.Windows.Input;

namespace Hangman.WPF.ViewModels
{
    public class LanguageSelectionViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly LocalizationProvider _strings;

        public LocalizationProvider Strings { get; }

        public ICommand SelectSwedishCommand { get; }
        public ICommand SelectEnglishCommand { get; }

        public LanguageSelectionViewModel(MainViewModel mainViewModel, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _strings = strings;
            Strings = strings;

            SelectSwedishCommand = new RelayCommand(SelectSwedish);
            SelectEnglishCommand = new RelayCommand(SelectEnglish);
        }

        private void SelectSwedish(object? _)
        {
            _strings.SetStrategy(new SwedishUiStrings());
            _mainViewModel.NavigateToMenu();
        }

        private void SelectEnglish(object? _)
        {
            _strings.SetStrategy(new EnglishUiStrings());
            _mainViewModel.NavigateToMenu();
        }
    }
}