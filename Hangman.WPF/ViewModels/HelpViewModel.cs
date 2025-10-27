using System.Windows.Input;

namespace Hangman.WPF.ViewModels
{
    // MODIFIERAD: Ärv från BaseViewModel
    public class HelpViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        public ICommand BackToMenuCommand { get; }

        // --- Text från SwedishUiStrings.cs ---
        public string HelpTitle => "--- HJÄLP / HUR MAN SPELAR ---";
        public string HelpLine1 => "Spelet går ut på att gissa det hemliga ordet, bokstav för bokstav.";
        public string HelpLine2 => "Du har 6 gissningar på dig innan gubben hängs (MAX 6 fel).";
        public string HelpModesTitle => "Lägen:";
        public string HelpModesLine1 => "  1. Enspelare (Highscore): Spelet fortsätter tills du förlorar.";
        public string HelpModesLine2 => "  2. Turnering (Tvåspelare): Ni har 3 liv var. Liv återställs vid vinst.";
        public string HelpSourcesTitle => "Ordkällor:";
        public string HelpSourcesLine1 => "  API: Engelska ord med längd baserat på svårighetsgrad.";
        public string HelpSourcesLine2 => "  Lokal: Svenska ord från en inbyggd lista.";
        public string HelpSourcesLine3 => "  Anpassad: Ord som du själv har lagt till (både svenska och engelska).";
        // ---

        public HelpViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());
        }
    }
}