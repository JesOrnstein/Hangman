using Hangman.Core;
using Hangman.Core.Exceptions;
using Hangman.Core.Models;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Localizations;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Hangman.WPF.ViewModels
{
    public class TournamentViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAsyncWordProvider _wordProvider;
        private readonly Game _game;
        private readonly TwoPlayerGame _tournament;
        private readonly DispatcherTimer _timer;
        private readonly LocalizationProvider _strings;

        private static readonly string[] _animFrames =
        {
            "*creak...* ","* *creak...* ","  *creak...* ","   *creak...* ","    *creak...* ","     *creak...* ",
            "    *creak...* ","   *creak...* ","  *creak...* "," *creak...* ","*creak...* ","               "
        };
        private const int AnimFrameCount = 12;

        public LocalizationProvider Strings { get; }

        public Player Player1 => _tournament.Player1;
        public Player Player2 => _tournament.Player2;

        private string _currentGuesserName = "";
        public string CurrentGuesserName { get => _currentGuesserName; set { _currentGuesserName = value; OnPropertyChanged(); } }

        // "Aktiv spelare" utan kolon – bindas i XAML via Run
        public string ActivePlayerText => _strings.RoundActivePlayer.Replace(":", "");

        private string _tournamentStatusMessage = "";
        public string TournamentStatusMessage { get => _tournamentStatusMessage; set { _tournamentStatusMessage = value; OnPropertyChanged(); } }

        private string _maskedWord = "Laddar...";
        public string MaskedWord { get => _maskedWord; set { _maskedWord = value; OnPropertyChanged(); } }

        private string _usedLetters = "";
        public string UsedLetters { get => _usedLetters; set { _usedLetters = value; OnPropertyChanged(); } }

        // Använd pack-URI så resurser alltid hittas
        private string _gallowsImageSource = Pack("/Images/stage_0.png");
        public string GallowsImageSource { get => _gallowsImageSource; set { _gallowsImageSource = value; OnPropertyChanged(); } }

        private int _secondsLeft = 60;
        public int SecondsLeft { get => _secondsLeft; set { _secondsLeft = value; OnPropertyChanged(); OnPropertyChanged(nameof(TimerText)); } }

        public string TimerText => _strings.RoundTimerDisplay(SecondsLeft);

        private string _creakAnimationText = "";
        public string CreakAnimationText { get => _creakAnimationText; set { _creakAnimationText = value; OnPropertyChanged(); } }

        private bool _isRoundInProgress = false;
        public bool IsRoundInProgress
        {
            get => _isRoundInProgress;
            set
            {
                _isRoundInProgress = value;
                OnPropertyChanged();
                // Om din RelayCommand har RaiseCanExecuteChanged – trigga om knapparna
                if (GuessCommand is RelayCommand rc) rc.RaiseCanExecuteChanged();
            }
        }

        private bool _isTournamentInProgress = true;
        public bool IsTournamentInProgress { get => _isTournamentInProgress; set { _isTournamentInProgress = value; OnPropertyChanged(); } }

        private string _tournamentEndMessage = "";
        public string TournamentEndMessage { get => _tournamentEndMessage; set { _tournamentEndMessage = value; OnPropertyChanged(); } }

        public ICommand GuessCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand NextRoundCommand { get; }

        public char[] KeyboardLetters { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ".ToCharArray();

        public TournamentViewModel(MainViewModel mainViewModel, IAsyncWordProvider provider, GameSettings settings, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _wordProvider = provider;
            _strings = strings;
            Strings = strings;

            _tournament = new TwoPlayerGame(settings.PlayerName, settings.PlayerName2, _wordProvider);
            _game = new Game(6);

            _game.LetterGuessed += OnGameUpdated;
            _game.WrongLetterGuessed += OnGameUpdated;
            _game.GameEnded += OnRoundEnded;

            GuessCommand = new RelayCommand(MakeGuess, CanGuess);
            // ÄNDRAD: Anropar ExitTournament-metoden
            BackToMenuCommand = new RelayCommand(_ => ExitTournament());
            NextRoundCommand = new RelayCommand(async _ => await StartNewRound());

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            MaskedWord = _strings.FeedbackFetchingWord("...");

            // VIKTIGT: starta rundan asynkront på UI-tråden, inte Task.Run
            _ = StartNewRound();
        }

        // --- NY METOD: Rensa events för att undvika minnesläckor ---
        private void CleanupEvents()
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;

            if (_game != null)
            {
                _game.LetterGuessed -= OnGameUpdated;
                _game.WrongLetterGuessed -= OnGameUpdated;
                _game.GameEnded -= OnRoundEnded;
            }
        }
        // -----------------------------------------------------------

        // --- NY METOD: Hanterar avslutning och navigering ---
        private void ExitTournament()
        {
            CleanupEvents();
            _mainViewModel.NavigateToMenu();
        }
        // -----------------------------------------------------

        private async Task StartNewRound()
        {
            IsRoundInProgress = true;
            TournamentStatusMessage = string.Empty;
            MaskedWord = _strings.FeedbackFetchingWord("...");
            UsedLetters = "";
            GallowsImageSource = Pack("/Images/stage_0.png");
            CreakAnimationText = string.Empty;
            CurrentGuesserName = _tournament.CurrentPlayerName;

            OnPropertyChanged(nameof(ActivePlayerText));
            OnPropertyChanged(nameof(Player1));
            OnPropertyChanged(nameof(Player2));

            try
            {
                string word = await _tournament.StartNewRoundAsync(); // återupptas på UI-tråd
                _game.StartNew(word);
            }
            catch (NoCustomWordsFoundException ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        _strings.ErrorNoCustomWordsFound(ex.Difficulty, ex.Language),
                        _strings.SelectWordSourceTitle);
                });
                CleanupEvents(); // Rensa events innan navigering
                _mainViewModel.NavigateToMenu();
                return;
            }
            catch (InvalidOperationException)
            {
                EndTournament();
                return;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        _strings.ErrorCouldNotFetchTournamentWord(ex.Message),
                        _strings.ErrorApiGeneric);
                });
                _game.StartNew("APIERROR");
            }

            UpdateUiProperties();
            SecondsLeft = 60;
            _timer.Start();
        }

        private void EndTournament()
        {
            IsTournamentInProgress = false;
            IsRoundInProgress = false;

            // ÄNDRAD: Rensa events här också, ifall turneringen avslutas naturligt (utan ExitTournament)
            CleanupEvents();

            CreakAnimationText = string.Empty;

            if (_tournament.TournamentStatus == GameStatus.Draw)
            {
                TournamentEndMessage = _strings.FeedbackTournamentDraw;
            }
            else
            {
                Player? winner = _tournament.GetWinner();
                TournamentEndMessage = winner != null
                    ? _strings.FeedbackTournamentWinner(winner.Name)
                    : _strings.FeedbackTournamentUnexpectedEnd;
            }

            TournamentEndMessage += $"\n\n{_strings.FeedbackTournamentFinalWins}\n" +
                                    $"{_strings.FeedbackTournamentPlayerWins(Player1.Name, Player1.Wins)}\n" +
                                    $"{_strings.FeedbackTournamentPlayerWins(Player2.Name, Player2.Wins)}";
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            SecondsLeft--;
            int frame = SecondsLeft % AnimFrameCount;
            CreakAnimationText = _animFrames[frame];

            if (SecondsLeft <= 0)
            {
                _timer.Stop();
                CreakAnimationText = string.Empty;
                TournamentStatusMessage = _strings.RoundTimerExpired;
                _game.ForceLose();
            }
        }

        private bool CanGuess(object? parameter)
        {
            if (parameter is char letter)
            {
                return IsRoundInProgress && !_game.UsedLetters.Contains(letter);
            }
            return false;
        }

        private void MakeGuess(object? parameter)
        {
            if (parameter is char letter)
            {
                _game.Guess(letter);
                if (GuessCommand is RelayCommand rc) rc.RaiseCanExecuteChanged();
            }
        }

        private void OnGameUpdated(object? sender, char e)
        {
            UpdateUiProperties();
            if (GuessCommand is RelayCommand rc) rc.RaiseCanExecuteChanged();
        }

        private void OnRoundEnded(object? sender, GameStatus status)
        {
            _timer.Stop();
            IsRoundInProgress = false;
            CreakAnimationText = string.Empty;

            _tournament.HandleRoundEnd(status);

            if (status == GameStatus.Won)
            {
                TournamentStatusMessage = $"{CurrentGuesserName} {_strings.RoundWon} {_strings.EndScreenCorrectWord(_game.Secret)}";
            }
            else
            {
                if (string.IsNullOrEmpty(TournamentStatusMessage))
                    TournamentStatusMessage = $"{CurrentGuesserName} {_strings.RoundLost} {_strings.EndScreenCorrectWord(_game.Secret)}";
            }

            UpdateUiProperties();
            OnPropertyChanged(nameof(Player1));
            OnPropertyChanged(nameof(Player2));
        }

        private void UpdateUiProperties()
        {
            MaskedWord = string.Join(" ", _game.GetMaskedWord().ToCharArray());
            UsedLetters = $"{_strings.RoundGuessedLetters} {string.Join(", ", _game.UsedLetters.OrderBy(c => c))}";
            GallowsImageSource = Pack($"/Images/stage_{_game.Mistakes}.png");
        }

        private static string Pack(string relative) =>
            // Byt "Hangman.WPF" om ditt WPF-assembly heter något annat
            $"pack://application:,,,/Hangman.WPF;component{relative}";
    }
}