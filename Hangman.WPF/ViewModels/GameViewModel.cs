using Hangman.Core;
using Hangman.Core.Exceptions;
using Hangman.Core.Localizations;
using Hangman.Core.Models;
using Hangman.Core.Providers.Api;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Providers.Local;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
// using System.Reflection; // Reflection är borttagen

namespace Hangman.WPF.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAsyncWordProvider _wordProvider;
        private readonly IStatisticsService _statisticsService;
        private readonly Game _game;
        private readonly DispatcherTimer _timer;
        private readonly LocalizationProvider _strings;

        private string _playerName;
        private int _consecutiveWins = 0;

        private static readonly string[] _animFrames =
        {
            "*creak...* ", "* *creak...* ", "  *creak...* ", "   *creak...* ", "    *creak...* ", "     *creak...*",
            "    *creak...* ", "   *creak...* ", "  *creak...* ", " *creak...* ", "*creak...* ", "               "
        };
        private const int AnimFrameCount = 12;

        public LocalizationProvider Strings { get; }

        private string _maskedWord = "Laddar ord...";
        public string MaskedWord { get => _maskedWord; set { _maskedWord = value; OnPropertyChanged(); } }

        private string _usedLetters = "";
        public string UsedLetters { get => _usedLetters; set { _usedLetters = value; OnPropertyChanged(); } }

        private string _gallowsImageSource = Pack("/Images/stage_0.png");
        public string GallowsImageSource { get => _gallowsImageSource; set { _gallowsImageSource = value; OnPropertyChanged(); } }

        private int _secondsLeft = 60;
        public int SecondsLeft { get => _secondsLeft; set { _secondsLeft = value; OnPropertyChanged(); } }

        public string TimerText => _strings.RoundTimerDisplay(SecondsLeft);

        private string _creakAnimationText = "";
        public string CreakAnimationText { get => _creakAnimationText; set { _creakAnimationText = value; OnPropertyChanged(); } }

        private bool _isGameInProgress = false;
        public bool IsGameInProgress { get => _isGameInProgress; set { _isGameInProgress = value; OnPropertyChanged(); } }

        private string _gameEndMessage = "";
        public string GameEndMessage { get => _gameEndMessage; set { _gameEndMessage = value; OnPropertyChanged(); } }

        public ICommand GuessCommand { get; }
        public ICommand PlayAgainCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand BackToMenuFinalCommand { get; }

        public char[] KeyboardLetters { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ".ToCharArray();

        public GameViewModel(MainViewModel mainViewModel, IAsyncWordProvider wordProvider, IStatisticsService statisticsService, string playerName, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _wordProvider = wordProvider;
            _statisticsService = statisticsService;
            _playerName = playerName;
            _strings = strings;
            Strings = strings;
            _game = new Game(6);

            _game.LetterGuessed += OnGameUpdated;
            _game.WrongLetterGuessed += OnGameUpdated;
            _game.GameEnded += OnGameEnded;

            GuessCommand = new RelayCommand(MakeGuess, CanGuess);
            PlayAgainCommand = new RelayCommand(async _ => await StartNewRound());
            BackToMenuCommand = new RelayCommand(_ => ExitGame(saveScore: false));
            BackToMenuFinalCommand = new RelayCommand(_ => ExitGame(saveScore: true));

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            MaskedWord = _strings.FeedbackFetchingWord("...");
            Task.Run(StartNewRound);
        }

        private void CleanupEvents()
        {
            _timer.Tick -= Timer_Tick;
            _game.LetterGuessed -= OnGameUpdated;
            _game.WrongLetterGuessed -= OnGameUpdated;
            _game.GameEnded -= OnGameEnded;
        }

        private async Task StartNewRound()
        {
            IsGameInProgress = true;
            GameEndMessage = string.Empty;
            MaskedWord = _strings.FeedbackFetchingWord("...");
            UsedLetters = "";
            GallowsImageSource = Pack("/Images/stage_0.png");
            CreakAnimationText = string.Empty;

            try
            {
                string word = await _wordProvider.GetWordAsync();
                _game.StartNew(word);
            }
            catch (NoCustomWordsFoundException ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        _strings.ErrorNoCustomWordsFound(ex.Difficulty, ex.Language),
                        _strings.SelectWordSourceTitle
                    );
                });
                CleanupEvents();
                _mainViewModel.NavigateToMenu();
                return;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        _strings.ErrorCouldNotStartGame(ex.Message),
                        _strings.ErrorApiGeneric
                    );
                });
                _game.StartNew("APIERROR");
            }

            UpdateUiProperties();
            SecondsLeft = 60;
            _timer.Start();

            // Fix: Tvinga UI att utvärdera GuessCommand.CanExecute igen, vilket återställer knapparna.
            if (GuessCommand is RelayCommand rc) rc.RaiseCanExecuteChanged();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            SecondsLeft--;
            OnPropertyChanged(nameof(TimerText));

            int frame = SecondsLeft % AnimFrameCount;
            CreakAnimationText = _animFrames[frame];

            if (SecondsLeft <= 0)
            {
                _timer.Stop();
                CreakAnimationText = string.Empty;
                GameEndMessage = _strings.RoundTimerExpired;
                _game.ForceLose();
            }
        }

        private bool CanGuess(object? parameter)
        {
            if (parameter is char letter)
            {
                return IsGameInProgress && !_game.UsedLetters.Contains(letter);
            }
            return false;
        }

        private void MakeGuess(object? parameter)
        {
            if (parameter is char letter)
            {
                _game.Guess(letter);
            }
        }

        private void OnGameUpdated(object? sender, char e)
        {
            UpdateUiProperties();
        }

        private async void OnGameEnded(object? sender, GameStatus status)
        {
            _timer.Stop();
            IsGameInProgress = false;
            CreakAnimationText = string.Empty;

            if (status == GameStatus.Won)
            {
                _consecutiveWins++;
                GameEndMessage = $"{_strings.EndScreenCongrats} {_strings.EndScreenCorrectWord(_game.Secret)}\n{_strings.FeedbackWonRound(_consecutiveWins)}";
            }
            else
            {
                if (string.IsNullOrEmpty(GameEndMessage))
                {
                    GameEndMessage = $"{_strings.EndScreenLost} {_strings.EndScreenCorrectWord(_game.Secret)}";
                }

                if (_consecutiveWins > 0)
                {
                    await SaveHighscoreAsync();
                    _consecutiveWins = 0;
                }
            }
            UpdateUiProperties();
        }

        private void UpdateUiProperties()
        {
            MaskedWord = string.Join(" ", _game.GetMaskedWord().ToCharArray());
            UsedLetters = $"{_strings.RoundGuessedLetters} {string.Join(", ", _game.UsedLetters.OrderBy(c => c))}";
            GallowsImageSource = Pack($"/Images/stage_{_game.Mistakes}.png");
        }

        private async void ExitGame(bool saveScore)
        {
            _timer.Stop();
            CreakAnimationText = string.Empty;

            CleanupEvents();

            if (saveScore && _consecutiveWins > 0)
            {
                await SaveHighscoreAsync();
            }
            _mainViewModel.NavigateToMenu();
        }

        private async Task SaveHighscoreAsync()
        {
            // FIX: Använder nu det publika interfacet istället för Reflection.
            WordDifficulty difficulty = _wordProvider.Difficulty;

            var newScore = new HighscoreEntry
            {
                PlayerName = _playerName,
                ConsecutiveWins = _consecutiveWins,
                Difficulty = difficulty
            };

            await _statisticsService.SaveHighscoreAsync(newScore);
        }

        private static string Pack(string relative) =>
            $"pack://application:,,,/Hangman.WPF;component{relative}";
    }
}