using Hangman.Core.Models;
using Hangman.Core.Providers.Interface;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Hangman.Core.Localizations;
using System.Linq;
using System.Windows;

namespace Hangman.WPF.ViewModels
{
    public class HighscoreViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IStatisticsService _statisticsService;
        private readonly LocalizationProvider _strings;

        public LocalizationProvider Strings { get; }

        public ObservableCollection<HighscoreEntry> Highscores { get; } = new ObservableCollection<HighscoreEntry>();

        private string _statusMessage = "";
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }

        public ICommand BackToMenuCommand { get; }

        public HighscoreViewModel(MainViewModel mainViewModel, IStatisticsService statisticsService, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _statisticsService = statisticsService;
            _strings = strings;
            Strings = strings;

            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());

            StatusMessage = _strings.HighscoreStatusLoading;
            Task.Run(LoadHighscores);
        }

        private async Task LoadHighscores()
        {
            try
            {
                var scores = await _statisticsService.GetGlobalTopScoresAsync(10);

                Application.Current.Dispatcher.Invoke(() =>
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
                        StatusMessage = _strings.HighscoreStatusNoneFoundWpf;
                    }
                });
            }
            catch (System.Exception ex)
            {
                StatusMessage = _strings.HighscoreStatusError(ex.Message);
            }
        }
    }
}