using Hangman.Core;
using Hangman.Core.Models;
using Hangman.Core.Providers.Interface;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System;
using Hangman.Core.Exceptions;
using System.Windows;
using Hangman.Core.Providers.Api;
using Hangman.Core.Providers.Local;
using Hangman.Core.Localizations; // <-- NY USING

namespace Hangman.WPF.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAsyncWordProvider _wordProvider;
        private readonly IStatisticsService _statisticsService;
        private readonly Game _game;
        private readonly DispatcherTimer _timer;
        private readonly LocalizationProvider _strings; // <-- NYTT FÄLT

        private string _playerName;
        private int _consecutiveWins = 0;

        // --- Animationsramar (från ConsoleRenderer) ---
        private static readonly string[] _animFrames =
        {
            "*creak...* ", "* *creak...* ", "  *creak...* ", "   *creak...* ", "    *creak...* ", "     *creak...*",
            "    *creak...* ", "   *creak...* ", "  *creak...* ", " *creak...* ", "*creak...* ", "               "
        };
        private const int AnimFrameCount = 12;

        // --- Bindningsbara Egenskaper ---

        // NY PROPERTY: Exponera strängarna för XAML
        public LocalizationProvider Strings { get; }

        private string _maskedWord = "Laddar ord...";
        public string MaskedWord { get => _maskedWord; set { _maskedWord = value; OnPropertyChanged(); } }

        private string _usedLetters = "";
        public string UsedLetters { get => _usedLetters; set { _usedLetters = value; OnPropertyChanged(); } }

        private string _gallowsImageSource = "/Images/stage_0.png";
        public string GallowsImageSource { get => _gallowsImageSource; set { _gallowsImageSource = value; OnPropertyChanged(); } }

        private int _secondsLeft = 60;
        public int SecondsLeft { get => _secondsLeft; set { _secondsLeft = value; OnPropertyChanged(); } }

        // NY PROPERTY: Formaterar timer-texten via LocalizationProvider
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

        // UPPDATERAD KONSTRUKTOR
        public GameViewModel(MainViewModel mainViewModel, IAsyncWordProvider wordProvider, IStatisticsService statisticsService, string playerName, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _wordProvider = wordProvider;
            _statisticsService = statisticsService;
            _playerName = playerName;
            _strings = strings; // Spara för C#-logik
            Strings = strings;  // Exponera för XAML
            _game = new Game(6);

            // Prenumerera på events
            _game.LetterGuessed += OnGameUpdated;
            _game.WrongLetterGuessed += OnGameUpdated;
            _game.GameEnded += OnGameEnded;

            // Kommandon
            GuessCommand = new RelayCommand(MakeGuess, CanGuess);
            PlayAgainCommand = new RelayCommand(async _ => await StartNewRound());
            BackToMenuCommand = new RelayCommand(_ => ExitGame(saveScore: false));
            BackToMenuFinalCommand = new RelayCommand(_ => ExitGame(saveScore: true));

            // Timer
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            Task.Run(StartNewRound);
        }

        // UPPDATERAD LOGIK: Använder _strings för felmeddelanden
        private async Task StartNewRound()
        {
            IsGameInProgress = true;
            GameEndMessage = string.Empty;
            MaskedWord = _strings.FeedbackFetchingWord("..."); // "Laddar ord..."
            UsedLetters = "";
            GallowsImageSource = "/Images/stage_0.png";
            CreakAnimationText = string.Empty;

            try
            {
                string word = await _wordProvider.GetWordAsync();
                _game.StartNew(word);
            }
            catch (NoCustomWordsFoundException ex)
            {
                // ÄNDRAD
                MessageBox.Show(
                    _strings.ErrorNoCustomWordsFound(ex.Difficulty, ex.Language),
                    _strings.SelectWordSourceTitle // "Ordlistefel"
                );
                _mainViewModel.NavigateToMenu();
                return;
            }
            catch (Exception ex)
            {
                // ÄNDRAD
                MessageBox.Show(
                    _strings.ErrorCouldNotStartGame(ex.Message),
                    "API-fel" // Kan också läggas i IUiStrings
                );
                _game.StartNew("APIERROR"); // Fallback
            }

            UpdateUiProperties();
            SecondsLeft = 60;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            SecondsLeft--;
            OnPropertyChanged(nameof(TimerText)); // Meddela UI att timer-texten ändrats

            int frame = SecondsLeft % AnimFrameCount;
            CreakAnimationText = _animFrames[frame];

            if (SecondsLeft <= 0)
            {
                _timer.Stop();
                CreakAnimationText = string.Empty;
                GameEndMessage = _strings.RoundTimerExpired; // ÄNDRAD
                _game.ForceLose();
            }
        }

        private bool CanGuess(object? parameter)
        {
            // ... (logik oförändrad) ...
            if (parameter is char letter)
            {
                return IsGameInProgress && !_game.UsedLetters.Contains(letter);
            }
            return false;
        }

        private void MakeGuess(object? parameter)
        {
            // ... (logik oförändrad) ...
            if (parameter is char letter)
            {
                _game.Guess(letter);
            }
        }

        private void OnGameUpdated(object? sender, char e)
        {
            UpdateUiProperties();
        }

        // UPPDATERAD LOGIK: Använder _strings för feedback
        private async void OnGameEnded(object? sender, GameStatus status)
        {
            _timer.Stop();
            IsGameInProgress = false;
            CreakAnimationText = string.Empty;

            if (status == GameStatus.Won)
            {
                _consecutiveWins++;
                // ÄNDRAD
                GameEndMessage = $"{_strings.EndScreenCongrats} {_strings.EndScreenCorrectWord(_game.Secret)}\n{_strings.FeedbackWonRound(_consecutiveWins)}";
            }
            else // Lost
            {
                // ÄNDRAD (visar bara tiden om det var anledningen)
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
            UsedLetters = $"{_strings.RoundGuessedLetters} {string.Join(", ", _game.UsedLetters.OrderBy(c => c))}"; // ÄNDRAD
            GallowsImageSource = $"/Images/stage_{_game.Mistakes}.png";
        }

        private async void ExitGame(bool saveScore)
        {
            _timer.Stop();
            CreakAnimationText = string.Empty;
            if (saveScore && _consecutiveWins > 0)
            {
                await SaveHighscoreAsync();
            }
            _mainViewModel.NavigateToMenu();
        }

        private async Task SaveHighscoreAsync()
        {
            // ... (logik oförändrad) ...
            WordDifficulty difficulty;
            if (_wordProvider is ApiWordProvider api)
            {
                difficulty = (WordDifficulty)api.GetType().GetField("_difficulty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(api)!;
            }
            else if (_wordProvider is WordProvider local)
            {
                difficulty = (WordDifficulty)local.GetType().GetField("_difficulty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(local)!;
            }
            else if (_wordProvider is CustomWordProvider custom)
            {
                difficulty = (WordDifficulty)custom.GetType().GetField("_difficulty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(custom)!;
            }
            else
            {
                return;
            }

            var newScore = new HighscoreEntry
            {
                PlayerName = _playerName,
                ConsecutiveWins = _consecutiveWins,
                Difficulty = difficulty
            };

            await _statisticsService.SaveHighscoreAsync(newScore);
        }
    }
}