using System.Windows.Input;
using Hangman.Core.Localizations; // <-- NY USING

namespace Hangman.WPF.ViewModels
{
    // MODIFIERAD: Ärv från BaseViewModel
    public class HelpViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        public ICommand BackToMenuCommand { get; }

        // NY PROPERTY: Exponera strängarna för XAML-bindning
        public LocalizationProvider Strings { get; }

        // UPPDATERAD KONSTRUKTOR
        public HelpViewModel(MainViewModel mainViewModel, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            Strings = strings; // Spara instansen
            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());
        }
    }
}