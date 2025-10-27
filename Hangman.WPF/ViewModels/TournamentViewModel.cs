using Hangman.Core;
using Hangman.Core.Exceptions;
using Hangman.Core.Models;
using Hangman.Core.Providers.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Hangman.Core.Localizations; // <-- NY USING

namespace Hangman.WPF.ViewModels
{
    // NY FIL: Kombinerar turneringslogik med spellogik (inspirerad av GameViewModel)
    public class TournamentViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAsyncWordProvider _wordProvider;
        private readonly Game _game;
        private readonly TwoPlayerGame _tournament;
        private readonly DispatcherTimer _timer;
        private readonly LocalizationProvider _strings; // <-- NYTT FÄLT

        // --- Animationsramar (från ConsoleRenderer) ---
        private static readonly string[] _animFrames =
        {
            "*creak...* ", "* *creak...* ", "  *creak...* ", "   *creak...* ", "    *creak...* ", "     *creak...*",
            "    *creak...* ", "   *creak...* ", "  *creak...* ", " *creak...* ", "*creak...* ", "               "
        };
        private const int AnimFrameCount = 12;

        // --- Bindningsbara Egenskaper (Turnering) ---

        // NY PROPERTY: Exponera strängarna för XAML
        public LocalizationProvider Strings { get; }

        public Player Player1 => _tournament.Player1;
        public Player Player2 => _tournament.Player2;

        private string _currentGuesserName = "";
        public string CurrentGuesserName { get => _currentGuesserName; set { _currentGuesserName = value; OnPropertyChanged(); } }

        // NY PROPERTY: Binder till den aktiva spelaren
        public string ActivePlayerText => _strings.RoundActivePlayerWithLives(CurrentGuesserName, 0).Split(':')[0]; // "Aktiv spelare:"

        private string _tournamentStatusMessage = "";
        public string TournamentStatusMessage { get => _tournamentStatusMessage; set { _tournamentStatusMessage = value; OnPropertyChanged(); } }

        // --- Bindningsbara Egenskaper (Spelomgång, från GameViewModel) ---
        private string _maskedWord = "Laddar...";
        public string MaskedWord { get => _maskedWord; set { _maskedWord = value; OnPropertyChanged(); } }

        private string _usedLetters = "";
        public string UsedLetters { get => _usedLetters; set { _usedLetters = value; OnPropertyChanged(); } }

        private string _gallowsImageSource = "/Images/stage_0.png";
        public string GallowsImageSource { get => _gallowsImageSource; set { _gallowsImageSource = value; OnPropertyChanged(); } }

        private int _secondsLeft = 60;
        public int SecondsLeft { get => _secondsLeft; set { _secondsLeft = value; OnPropertyChanged(); } }

        // NY PROPERTY: Formaterar timer-texten
        public string TimerText => _strings.RoundTimerDisplay(SecondsLeft);

        // NY PROPERTY FÖR ANIMATION
        private string _creakAnimationText = "";
        public string CreakAnimationText { get => _creakAnimationText; set { _creakAnimationText = value; OnPropertyChanged(); } }

        private bool _isRoundInProgress = false;
        public bool IsRoundInProgress { get => _isRoundInProgress; set { _isRoundInProgress = value; OnPropertyChanged(); } }

        private bool _isTournamentInProgress = true;
        public bool IsTournamentInProgress { get => _isTournamentInProgress; set { _isTournamentInProgress = value; OnPropertyChanged(); } }

        private string _tournamentEndMessage = "";
        public string TournamentEndMessage { get => _tournamentEndMessage; set { _tournamentEndMessage = value; OnPropertyChanged(); } }

        public ICommand GuessCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand NextRoundCommand { get; }

        public char[] KeyboardLetters { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ".ToCharArray();

        // UPPDATERAD KONSTRUKTOR
        public TournamentViewModel(MainViewModel mainViewModel, IAsyncWordProvider provider, GameSettings settings, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _wordProvider = provider;
            _strings = strings; // Spara för C#-logik
            Strings = strings;  // Exponera för XAML

            _tournament = new TwoPlayerGame(settings.PlayerName, settings.PlayerName2, _wordProvider);
            _game = new Game(6);

            // Prenumerera på events
            _game.LetterGuessed += OnGameUpdated;
            _game.WrongLetterGuessed += OnGameUpdated;
            _game.GameEnded += OnRoundEnded;

            // Kommandon
            GuessCommand = new RelayCommand(MakeGuess, CanGuess);
            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());
            NextRoundCommand = new RelayCommand(async _ => await StartNewRound());

            // Timer
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            Task.Run(StartNewRound);
        }

        // UPPDATERAD LOGIK: Använder _strings
        private async Task StartNewRound()
        {
            IsRoundInProgress = true;
            TournamentStatusMessage = string.Empty;
            MaskedWord = _strings.FeedbackFetchingWord("..."); // "Hämtar ord..."
            UsedLetters = "";
            GallowsImageSource = "/Images/stage_0.png";
            CreakAnimationText = string.Empty;
            CurrentGuesserName = _tournament.CurrentPlayerName;
            OnPropertyChanged(nameof(ActivePlayerText)); // Uppdatera "Aktiv spelare:"-texten

            OnPropertyChanged(nameof(Player1));
            OnPropertyChanged(nameof(Player2));

            try
            {
                string word = await _tournament.StartNewRoundAsync();
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
            catch (InvalidOperationException) // Turneringen är slut
            {
                EndTournament();
                return;
            }
            catch (Exception ex)
            {
                // ÄNDRAD
                MessageBox.Show(
                    _strings.ErrorCouldNotFetchTournamentWord(ex.Message),
                    "API-fel" // Kan läggas i IUiStrings
                );
                _game.StartNew("APIERROR"); // Fallback
            }

            UpdateUiProperties();
            SecondsLeft = 60;
            _timer.Start();
        }

        // UPPDATERAD LOGIK: Använder _strings
        private void EndTournament()
        {
            IsTournamentInProgress = false;
            IsRoundInProgress = false;
            _timer.Stop();
            CreakAnimationText = string.Empty;

            if (_tournament.TournamentStatus == GameStatus.Draw)
            {
                TournamentEndMessage = _strings.FeedbackTournamentDraw; // ÄNDRAD
            }
            else
            {
                Player? winner = _tournament.GetWinner();
                if (winner != null)
                {
                    TournamentEndMessage = _strings.FeedbackTournamentWinner(winner.Name); // ÄNDRAD
                }
                else
                {
                    TournamentEndMessage = _strings.FeedbackTournamentUnexpectedEnd; // ÄNDRAD
                }
            }

            // ÄNDRAD (hämtar strängar)
            TournamentEndMessage += $"\n\n{_strings.FeedbackTournamentFinalWins}\n" +
                                    $"{_strings.FeedbackTournamentPlayerWins(Player1.Name, Player1.Wins)}\n" +
                                    $"{_strings.FeedbackTournamentPlayerWins(Player2.Name, Player2.Wins)}";
        }


        private void Timer_Tick(object? sender, EventArgs e)
        {
            SecondsLeft--;
            OnPropertyChanged(nameof(TimerText)); // Meddela UI om textuppdatering

            int frame = SecondsLeft % AnimFrameCount;
            CreakAnimationText = _animFrames[frame];

            if (SecondsLeft <= 0)
            {
                _timer.Stop();
                CreakAnimationText = string.Empty;
                TournamentStatusMessage = _strings.RoundTimerExpired; // ÄNDRAD
                _game.ForceLose();
            }
        }

        private bool CanGuess(object? parameter)
        {
            // ... (logik oförändrad) ...
            if (parameter is char letter)
            {
                return IsRoundInProgress && !_game.UsedLetters.Contains(letter);
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

        // UPPDATERAD LOGIK: Använder _strings
        private void OnRoundEnded(object? sender, GameStatus status)
        {
            _timer.Stop();
            IsRoundInProgress = false;
            CreakAnimationText = string.Empty;

            _tournament.HandleRoundEnd(status);

            if (status == GameStatus.Won)
            {
                // ÄNDRAD
                TournamentStatusMessage = $"{CurrentGuesserName} {_strings.RoundWon} {_strings.EndScreenCorrectWord(_game.Secret)}";
            }
            else // Lost
            {
                // ÄNDRAD (visar bara tiden om det var anledningen)
                if (string.IsNullOrEmpty(TournamentStatusMessage))
                {
                    TournamentStatusMessage = $"{CurrentGuesserName} {_strings.RoundLost} {_strings.EndScreenCorrectWord(_game.Secret)}";
                }
            }

            UpdateUiProperties();

            OnPropertyChanged(nameof(Player1));
            OnPropertyChanged(nameof(Player2));
        }

        private void UpdateUiProperties()
        {
            MaskedWord = string.Join(" ", _game.GetMaskedWord().ToCharArray());
            UsedLetters = $"{_strings.RoundGuessedLetters} {string.Join(", ", _game.UsedLetters.OrderBy(c => c))}"; // ÄNDRAD
            GallowsImageSource = $"/Images/stage_{_game.Mistakes}.png";
        }
    }
}