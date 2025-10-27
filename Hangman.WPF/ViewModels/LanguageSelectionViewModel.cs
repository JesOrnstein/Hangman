// Ny fil: Hangman.WPF/ViewModels/LanguageSelectionViewModel.cs

using Hangman.Core.Localizations;
using System.Windows.Input;

namespace Hangman.WPF.ViewModels
{
    public class LanguageSelectionViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly LocalizationProvider _strings;

        // Exponera strängarna så att XAML kan binda till titeln (om vi vill)
        public LocalizationProvider Strings { get; }

        public ICommand SelectSwedishCommand { get; }
        public ICommand SelectEnglishCommand { get; }

        public LanguageSelectionViewModel(MainViewModel mainViewModel, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _strings = strings;
            Strings = strings; // Exponera för XAML

            SelectSwedishCommand = new RelayCommand(SelectSwedish);
            SelectEnglishCommand = new RelayCommand(SelectEnglish);
        }

        private void SelectSwedish(object? _)
        {
            // 1. Sätt språkstrategin till Svenska
            _strings.SetStrategy(new SwedishUiStrings());

            // 2. Navigera till huvudmenyn
            _mainViewModel.NavigateToMenu();
        }

        private void SelectEnglish(object? _)
        {
            // 1. Sätt språkstrategin till Engelska
            _strings.SetStrategy(new EnglishUiStrings());

            // 2. Navigera till huvudmenyn
            _mainViewModel.NavigateToMenu();
        }
    }
}