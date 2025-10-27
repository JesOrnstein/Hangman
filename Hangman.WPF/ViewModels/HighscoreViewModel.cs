using Hangman.Core.Models;
using Hangman.Core.Providers.Interface;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hangman.WPF.ViewModels
{
    public class HighscoreViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IStatisticsService _statisticsService;

        public ObservableCollection<HighscoreEntry> Highscores { get; } = new ObservableCollection<HighscoreEntry>();

        private string _statusMessage = "Hämtar highscores...";
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }

        public ICommand BackToMenuCommand { get; }

        public HighscoreViewModel(MainViewModel mainViewModel, IStatisticsService statisticsService)
        {
            _mainViewModel = mainViewModel;
            _statisticsService = statisticsService;
            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());

            Task.Run(LoadHighscores);
        }

        private async Task LoadHighscores()
        {
            try
            {
                //
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
                        StatusMessage = string.Empty;
                    }
                    else
                    {
                        StatusMessage = "Inga highscores hittades."; //
                    }
                });
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"Kunde inte ladda highscores: {ex.Message}";
            }
        }
    }
}