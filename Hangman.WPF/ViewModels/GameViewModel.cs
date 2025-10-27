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

namespace Hangman.WPF.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IAsyncWordProvider _wordProvider;
        private readonly IStatisticsService _statisticsService;
        private readonly Game _game;
        private readonly DispatcherTimer _timer;

        private string _playerName;
        private int _consecutiveWins = 0;

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

        // --- Bindningsbara Egenskaper ---
        private string _maskedWord = "Laddar ord...";
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

        private bool _isGameInProgress = false;
        public bool IsGameInProgress { get => _isGameInProgress; set { _isGameInProgress = value; OnPropertyChanged(); } }

        private string _gameEndMessage = "";
        public string GameEndMessage { get => _gameEndMessage; set { _gameEndMessage = value; OnPropertyChanged(); } }

        public ICommand GuessCommand { get; }
        public ICommand PlayAgainCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand BackToMenuFinalCommand { get; }

        // En lista med bokstäver för tangentbordet
        public char[] KeyboardLetters { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ".ToCharArray();

        public GameViewModel(MainViewModel mainViewModel, IAsyncWordProvider wordProvider, IStatisticsService statisticsService, string playerName)
        {
            _mainViewModel = mainViewModel;
            _wordProvider = wordProvider;
            _statisticsService = statisticsService;
            _playerName = playerName;
            _game = new Game(6);

            // Prenumerera på events från din modell!
            _game.LetterGuessed += OnGameUpdated;
            _game.WrongLetterGuessed += OnGameUpdated;
            _game.GameEnded += OnGameEnded;

            // Kommandon
            GuessCommand = new RelayCommand(MakeGuess, CanGuess);
            PlayAgainCommand = new RelayCommand(async _ => await StartNewRound());
            BackToMenuCommand = new RelayCommand(_ => ExitGame(saveScore: false));
            BackToMenuFinalCommand = new RelayCommand(_ => ExitGame(saveScore: true));

            // Timer (baserad på ConsoleRenderer)
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;

            // Starta första rundan
            Task.Run(StartNewRound);
        }

        private async Task StartNewRound()
        {
            IsGameInProgress = true;
            GameEndMessage = string.Empty;
            MaskedWord = "Hämtar ord...";
            UsedLetters = "";
            GallowsImageSource = "/Images/stage_0.png";
            CreakAnimationText = string.Empty; // Återställ animation

            try
            {
                string word = await _wordProvider.GetWordAsync();
                _game.StartNew(word);
            }
            catch (NoCustomWordsFoundException ex)
            {
                //
                MessageBox.Show($"Kunde inte starta spelet: Inga anpassade ord hittades för {ex.Difficulty} ({ex.Language}).", "Ordlistefel");
                _mainViewModel.NavigateToMenu();
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
                GameEndMessage = "Tiden är ute!"; //
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
            CreakAnimationText = string.Empty; // Rensa

            if (status == GameStatus.Won)
            {
                _consecutiveWins++;
                GameEndMessage = $"GRATTIS, DU VANN! Ordet var: {_game.Secret}\nVinster i rad: {_consecutiveWins}"; //
            }
            else // Lost
            {
                GameEndMessage = $"DU FÖRLORADE... Ordet var: {_game.Secret}"; //

                // Om spelaren förlorar, spara highscore och återställ
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
            // Lägger till mellanrum i strängen, precis som i ConsoleRenderer
            MaskedWord = string.Join(" ", _game.GetMaskedWord().ToCharArray());

            UsedLetters = $"Gissade: {string.Join(", ", _game.UsedLetters.OrderBy(c => c))}";
            GallowsImageSource = $"/Images/stage_{_game.Mistakes}.png";
        }

        private async void ExitGame(bool saveScore)
        {
            _timer.Stop();
            CreakAnimationText = string.Empty; // Rensa
            if (saveScore && _consecutiveWins > 0)
            {
                await SaveHighscoreAsync();
            }
            _mainViewModel.NavigateToMenu();
        }

        // --- KORRIGERAD METOD ---
        private async Task SaveHighscoreAsync()
        {
            // Försöker ta reda på svårighetsgraden från providern.
            // Detta är lite osäkert (använder reflektion), men fungerar för de kända typerna.
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
                return; // Kan inte avgöra svårighetsgrad för okänd provider
            }

            // KORRIGERING: Använd object initializer för att uppfylla 'required'
            var newScore = new HighscoreEntry
            {
                PlayerName = _playerName,
                ConsecutiveWins = _consecutiveWins,
                Difficulty = difficulty
            };

            await _statisticsService.SaveHighscoreAsync(newScore); //
        }
    }
}