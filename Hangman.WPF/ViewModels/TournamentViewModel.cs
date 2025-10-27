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

        // --- Animationsramar (från ConsoleRenderer) ---
        private static readonly string[] _animFrames =
        {
            "*creak...* ", // Frame 0
            " *creak...* ", // Frame 1
            "  *creak...* ", // Frame 2
            "   *creak...* ", // Frame 3
            "    *creak...* ", // Frame 4
            "     *creak...*", // Frame 5
            "    *creak...* ", // Frame 6
            "   *creak...* ", // Frame 7
            "  *creak...* ", // Frame 8
            " *creak...* ", // Frame 9
            "*creak...* ", // Frame 10
            "               "  // Frame 11 (paus)
        };
        private const int AnimFrameCount = 12;

        // --- Bindningsbara Egenskaper (Turnering) ---
        public Player Player1 => _tournament.Player1;
        public Player Player2 => _tournament.Player2;

        private string _currentGuesserName = "";
        public string CurrentGuesserName { get => _currentGuesserName; set { _currentGuesserName = value; OnPropertyChanged(); } }

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

        // NY PROPERTY FÖR ANIMATION
        private string _creakAnimationText = "";
        public string CreakAnimationText { get => _creakAnimationText; set { _creakAnimationText = value; OnPropertyChanged(); } }

        private bool _isRoundInProgress = false;
        public bool IsRoundInProgress { get => _isRoundInProgress; set { _isRoundInProgress = value; OnPropertyChanged(); } }

        // Döljer spelet/visar slutresultat
        private bool _isTournamentInProgress = true;
        public bool IsTournamentInProgress { get => _isTournamentInProgress; set { _isTournamentInProgress = value; OnPropertyChanged(); } }

        private string _tournamentEndMessage = "";
        public string TournamentEndMessage { get => _tournamentEndMessage; set { _tournamentEndMessage = value; OnPropertyChanged(); } }

        public ICommand GuessCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand NextRoundCommand { get; }

        public char[] KeyboardLetters { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ".ToCharArray();

        public TournamentViewModel(MainViewModel mainViewModel, IAsyncWordProvider provider, GameSettings settings)
        {
            _mainViewModel = mainViewModel;
            _wordProvider = provider;

            _tournament = new TwoPlayerGame(settings.PlayerName, settings.PlayerName2, _wordProvider);
            _game = new Game(6);

            // Prenumerera på events från spelomgången
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

            // Starta första rundan
            Task.Run(StartNewRound);
        }

        private async Task StartNewRound()
        {
            IsRoundInProgress = true;
            TournamentStatusMessage = string.Empty;
            MaskedWord = "Hämtar ord...";
            UsedLetters = "";
            GallowsImageSource = "/Images/stage_0.png";
            CreakAnimationText = string.Empty; // Rensa
            CurrentGuesserName = _tournament.CurrentPlayerName;

            // Uppdatera liv (viktigt för UI-bindning)
            OnPropertyChanged(nameof(Player1));
            OnPropertyChanged(nameof(Player2));

            try
            {
                // Använd turneringslogiken för att starta rundan
                string word = await _tournament.StartNewRoundAsync();
                _game.StartNew(word);
            }
            catch (NoCustomWordsFoundException ex)
            {
                MessageBox.Show($"Kunde inte starta spelet: Inga anpassade ord hittades för {ex.Difficulty} ({ex.Language}).", "Ordlistefel");
                _mainViewModel.NavigateToMenu();
                return;
            }
            catch (InvalidOperationException) // Kastas när turneringen är slut
            {
                EndTournament();
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kunde inte hämta ord: {ex.Message}", "API-fel");
                _game.StartNew("APIERROR"); // Fallback
            }

            UpdateUiProperties();
            SecondsLeft = 60;
            _timer.Start();
        }

        private void EndTournament()
        {
            IsTournamentInProgress = false;
            IsRoundInProgress = false;
            _timer.Stop();
            CreakAnimationText = string.Empty; // Rensa

            if (_tournament.TournamentStatus == GameStatus.Draw)
            {
                TournamentEndMessage = "OAVGJORT! Båda spelarna förlorade.";
            }
            else
            {
                Player? winner = _tournament.GetWinner();
                if (winner != null)
                {
                    TournamentEndMessage = $"GRATTIS, {winner.Name} VANN TURNERINGEN!";
                }
                else
                {
                    TournamentEndMessage = "Turneringen avslutad.";
                }
            }

            // Lägg till slutställning
            TournamentEndMessage += $"\n\nSlutställning:\n{Player1.Name}: {Player1.Wins} vinster\n{Player2.Name}: {Player2.Wins} vinster";
        }


        private void Timer_Tick(object? sender, EventArgs e)
        {
            SecondsLeft--;

            // NYTT: Uppdatera animationen
            int frame = SecondsLeft % AnimFrameCount;
            CreakAnimationText = _animFrames[frame];
            // ---

            if (SecondsLeft <= 0)
            {
                _timer.Stop();
                CreakAnimationText = string.Empty; // Rensa
                TournamentStatusMessage = "Tiden är ute!";
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
            }
        }

        // Kallas när en gissning görs
        private void OnGameUpdated(object? sender, char e)
        {
            UpdateUiProperties();
        }

        // Kallas när rundan är vunnen eller förlorad
        private void OnRoundEnded(object? sender, GameStatus status)
        {
            _timer.Stop();
            IsRoundInProgress = false;
            CreakAnimationText = string.Empty; // Rensa

            // Hantera resultatet i turneringen
            _tournament.HandleRoundEnd(status);

            if (status == GameStatus.Won)
            {
                TournamentStatusMessage = $"{CurrentGuesserName} VANN rundan! Ordet var: {_game.Secret}";
            }
            else // Lost
            {
                TournamentStatusMessage = $"{CurrentGuesserName} FÖRLORADE rundan... Ordet var: {_game.Secret}";
            }

            UpdateUiProperties();

            // Uppdatera liv i UI
            OnPropertyChanged(nameof(Player1));
            OnPropertyChanged(nameof(Player2));
        }

        private void UpdateUiProperties()
        {
            MaskedWord = string.Join(" ", _game.GetMaskedWord().ToCharArray());
            UsedLetters = $"Gissade: {string.Join(", ", _game.UsedLetters.OrderBy(c => c))}";
            GallowsImageSource = $"/Images/stage_{_game.Mistakes}.png";
        }
    }
}