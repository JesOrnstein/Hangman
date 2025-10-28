using System.Windows.Input;
using Hangman.Core.Localizations;

namespace Hangman.WPF.ViewModels
{
    public class HelpViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        public ICommand BackToMenuCommand { get; }

        public LocalizationProvider Strings { get; }

        public HelpViewModel(MainViewModel mainViewModel, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            Strings = strings;
            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());
        }
    }
}