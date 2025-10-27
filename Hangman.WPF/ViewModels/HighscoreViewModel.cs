using Hangman.Core.Models;
using Hangman.Core.Providers.Interface;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Hangman.Core.Localizations; // <-- NY USING
using System.Linq; // <-- NY USING

namespace Hangman.WPF.ViewModels
{
    public class HighscoreViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IStatisticsService _statisticsService;
        private readonly LocalizationProvider _strings; // <-- NYTT FÄLT

        // NY PROPERTY: Exponera strängarna för XAML
        public LocalizationProvider Strings { get; }

        public ObservableCollection<HighscoreEntry> Highscores { get; } = new ObservableCollection<HighscoreEntry>();

        private string _statusMessage = ""; // Tom från start
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }

        public ICommand BackToMenuCommand { get; }

        // UPPDATERAD KONSTRUKTOR
        public HighscoreViewModel(MainViewModel mainViewModel, IStatisticsService statisticsService, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _statisticsService = statisticsService;
            _strings = strings; // Spara för C#-logik
            Strings = strings;  // Exponera för XAML

            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());

            StatusMessage = _strings.HighscoreStatusLoading; // Sätt laddningsmeddelande
            Task.Run(LoadHighscores);
        }

        private async Task LoadHighscores()
        {
            try
            {
                var scores = await _statisticsService.GetGlobalTopScoresAsync(10);

                App.Current.Dispatcher.Invoke(() =>
                {
                    Highscores.Clear();
                    if (scores.Any())
                    {
                        foreach (var score in scores.OrderBy(s => s.Difficulty).ThenByDescending(s => s.ConsecutiveWins))
                        {
                            Highscores.Add(score);
                        }
                        StatusMessage = string.Empty; // Rensa status om vi hittade
                    }
                    else
                    {
                        StatusMessage = _strings.HighscoreStatusNoneFoundWpf; // ÄNDRAD
                    }
                });
            }
            catch (System.Exception ex)
            {
                StatusMessage = _strings.HighscoreStatusError(ex.Message); // ÄNDRAD
            }
        }
    }
}