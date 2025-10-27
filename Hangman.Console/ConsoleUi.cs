using Hangman.Core;
using Hangman.Core.Providers.Api;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Models;
using Hangman.Core.Providers.Local;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Hangman.Console.Localizations;

namespace Hangman.Console
{
    /// <summary>
    /// Hanterar all input och output i konsolen för spelet Hänga Gubbe.
    /// </summary>
    public class ConsoleUi
    {
        private Game? _game;

        private string _feedbackMessage = string.Empty;

        private readonly IStatisticsService _statisticsService;
        private readonly IUiStrings _strings; // <-- NYTT: Strategi-fält

        // Visualisering av galgen (ASCII-konst)
        // ... (HangmanStages-arrayen är oförändrad) ...
        private static readonly string[] HangmanStages =
        {
            // 0 fel
            @"
    +---+
    |   |
        |
        |
        |
        |
    =======",
            // 1 fel
            @"
    +---+
    |   |
    O   |
        |
        |
        |
    =======",
            // 2 fel
            @"
    +---+
    |   |
    O   |
    |   |
        |
        |
    =======",
            // 3 fel
            @"
    +---+
    |   |
    O   |
   /|   |
        |
        |
    =======",
            // 4 fel
            @"
    +---+
    |   |
    O   |
   /|\  |
        |
        |
    =======",
            // 5 fel
            @"
    +---+
    |   |
    O   |
   /|\  |
   /    |
        |
    =======",
            // 6 fel (Max)
            @"
    +---+
    |   |
    O   |
   /|\  |
   / \  |
        |
    ======="
        };

        // NY KONSTRUKTOR SOM ACCEPTERAR FLER BEROENDEN
        public ConsoleUi(IStatisticsService statsService, IUiStrings strings)
        {
            _statisticsService = statsService;
            _strings = strings; // <-- NYTT: Spara strategin
        }

        /// <summary>
        /// Huvudloopen för konsolgränssnittet.
        /// </summary>
        public async Task RunAsync()
        {
            // --- NYTT VÄLKOMST-BLOCK (Använder _strings) ---
            System.Console.Clear();
            System.Console.ForegroundColor = ConsoleColor.Cyan;

            // 1. AVANCERAD "HANGMAN"-titel
            System.Console.WriteLine(_strings.WelcomeTitleArt);

            // 2. VÄLKOMST-TEXT
            System.Console.ResetColor();
            System.Console.WriteLine(_strings.WelcomeMessage);
            System.Console.WriteLine(_strings.WelcomePressAnyKey);

            // 3. VÄNTA PÅ INPUT
            System.Console.ReadKey(true);
            // --- SLUT PÅ VÄLKOMST-BLOCK ---

            while (true)
            {
                var choice = ShowMainMenu();
                switch (choice)
                {
                    case '1':
                        await PlaySinglePlayerHighscoreAsync();
                        break;
                    case '2':
                        await PlayTournamentAsync();
                        break;
                    case '3':
                        await ShowHighscoresAsync();
                        break;
                    case '4':
                        await AddCustomWordAsync();
                        break;
                    case '5':
                        ShowHelpScreen();
                        break;
                    case '6':
                        System.Console.Clear();
                        System.Console.WriteLine(_strings.FeedbackThanksForPlaying);
                        return;
                }
            }
        }

        // UPPDATERAD ShowMainMenu
        private char ShowMainMenu()
        {
            System.Console.Clear();
            System.Console.ForegroundColor = ConsoleColor.Cyan;

            System.Console.WriteLine(_strings.MainMenuTitleArt);
            System.Console.WriteLine(_strings.MainMenuGallowsArt);

            System.Console.ResetColor();
            System.Console.WriteLine();

            System.Console.WriteLine(_strings.MainMenuTitle);
            System.Console.WriteLine(_strings.MenuPlaySingle);
            System.Console.WriteLine(_strings.MenuPlayTournament);
            System.Console.WriteLine(_strings.MenuShowHighscores);
            System.Console.WriteLine(_strings.MenuAddWord);
            System.Console.WriteLine(_strings.MenuHelp);
            System.Console.WriteLine(_strings.MenuQuit);

            System.Console.Write(_strings.MenuChoicePrompt);

            while (true)
            {
                var key = System.Console.ReadKey(intercept: true);
                if (key.KeyChar >= '1' && key.KeyChar <= '6')
                {
                    System.Console.WriteLine(key.KeyChar);
                    return key.KeyChar;
                }
            }
        }

        // --- Huvudloop för enspelare med Highscore-logik ---
        private async Task PlaySinglePlayerHighscoreAsync()
        {
            var (provider, currentDifficulty) = SelectWordSource();
            if (provider == null) return;

            string? playerName = GetPlayerName(_strings.PromptPlayerName);
            if (playerName == null) return;

            int consecutiveWins = 0;
            GameStatus roundResult;

            do
            {
                System.Console.Clear();
                System.Console.WriteLine(_strings.FeedbackFetchingWord(provider.DifficultyName));

                string secret;
                try
                {
                    secret = await provider.GetWordAsync();
                }
                catch (Exception ex)
                {
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine(_strings.ErrorCouldNotStartGame(ex.Message));
                    System.Console.ResetColor();
                    System.Console.WriteLine(_strings.CommonPressAnyKeyToContinue);
                    System.Console.ReadKey();
                    roundResult = GameStatus.Lost;
                    break;
                }

                roundResult = PlayRound(playerName, secret, currentLives: 0);

                if (roundResult == GameStatus.Won)
                {
                    consecutiveWins++;
                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.WriteLine(_strings.FeedbackWonRound(consecutiveWins));
                    System.Console.ResetColor();

                    System.Console.Write(_strings.PromptContinuePlaying);
                    var key = System.Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Escape || char.ToUpperInvariant(key.KeyChar) != 'J')
                    {
                        roundResult = GameStatus.Lost;
                        System.Console.WriteLine(_strings.FeedbackEndingLoop);
                    }
                    else
                    {
                        System.Console.WriteLine(_strings.FeedbackContinuing);
                    }
                }
                else
                {
                    if (consecutiveWins > 0)
                    {
                        System.Console.WriteLine(_strings.FeedbackPressAnyKeyToSave);
                        System.Console.ReadKey(true);
                    }
                }

            } while (roundResult == GameStatus.Won);

            if (consecutiveWins > 0 && currentDifficulty.HasValue)
            {
                var newScore = new HighscoreEntry(playerName, consecutiveWins, currentDifficulty.Value)
                {
                    PlayerName = playerName,
                    ConsecutiveWins = consecutiveWins,
                    Difficulty = currentDifficulty.Value
                };
                await _statisticsService.SaveHighscoreAsync(newScore);

                System.Console.WriteLine(_strings.FeedbackHighscoreSaved(consecutiveWins, currentDifficulty.Value));
            }

            System.Console.WriteLine(_strings.FeedbackReturningToMenu);
            System.Console.ReadKey(true);
        }

        // --- Visa Highscores (Top 10) ---
        private async Task ShowHighscoresAsync()
        {
            System.Console.Clear();
            System.Console.WriteLine(_strings.HighscoreTitle);
            System.Console.WriteLine(_strings.HighscoreFetching);

            try
            {
                var topScores = await _statisticsService.GetGlobalTopScoresAsync(10);

                if (!topScores.Any())
                {
                    System.Console.WriteLine(_strings.HighscoreNoneFound);
                }
                else
                {
                    foreach (var diffGroup in topScores.GroupBy(s => s.Difficulty))
                    {
                        System.Console.WriteLine(_strings.HighscoreDifficultyHeader(diffGroup.Key));
                        int rank = 1;
                        foreach (var score in diffGroup.OrderByDescending(s => s.ConsecutiveWins))
                        {
                            System.Console.WriteLine(_strings.HighscoreEntry(rank++, score.PlayerName, score.ConsecutiveWins));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(_strings.CommonErrorDatabaseError(ex.Message));
                System.Console.ResetColor();
            }

            System.Console.WriteLine(_strings.CommonPressAnyKeyToContinue);
            System.Console.ReadKey(true);
        }

        // --- Lägg till anpassat ord ---
        private async Task AddCustomWordAsync()
        {
            System.Console.Clear();
            System.Console.WriteLine(_strings.AddWordTitle);
            System.Console.WriteLine(_strings.CommonPressEscapeToCancel);

            string? word = string.Empty;
            while (string.IsNullOrWhiteSpace(word) || !word.All(char.IsLetter))
            {
                word = GetInputString(_strings.AddWordPrompt);

                if (word == null) return;

                if (string.IsNullOrWhiteSpace(word) || !word.All(char.IsLetter))
                {
                    System.Console.WriteLine(_strings.AddWordInvalid);
                }
            }

            word = word.ToUpperInvariant();

            WordDifficulty? difficulty = SelectDifficulty("Anpassat Ord"); // TODO: Denna text bör också in i IUiStrings
            if (difficulty == null) return;

            try
            {
                var saver = new CustomWordProvider(difficulty.Value);
                await saver.AddWordAsync(word, difficulty.Value);

                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine(_strings.AddWordSuccess(word, difficulty.Value));
                System.Console.ResetColor();
            }
            catch (InvalidOperationException ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine(_strings.AddWordErrorExists(ex.Message));
                System.Console.ResetColor();
            }
            catch (Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(_strings.CommonErrorDatabaseError(ex.Message));
                System.Console.ResetColor();
            }

            System.Console.WriteLine(_strings.CommonPressAnyKeyToContinue);
            System.Console.ReadKey(true);
        }

        // --- Hjälp/How to Play ---
        private void ShowHelpScreen()
        {
            System.Console.Clear();
            System.Console.WriteLine(_strings.HelpTitle);
            System.Console.WriteLine(_strings.HelpLine1);
            System.Console.WriteLine(_strings.HelpLine2);
            System.Console.WriteLine(_strings.HelpModesTitle);
            System.Console.WriteLine(_strings.HelpModesLine1);
            System.Console.WriteLine(_strings.HelpModesLine2);
            System.Console.WriteLine(_strings.HelpSourcesTitle);
            System.Console.WriteLine(_strings.HelpSourcesLine1);
            System.Console.WriteLine(_strings.HelpSourcesLine2);
            System.Console.WriteLine(_strings.HelpSourcesLine3);

            System.Console.WriteLine(_strings.CommonPressAnyKeyToContinue);
            System.Console.ReadKey(true);
        }

        // --- Huvudloop för 2-spelarturnering (MODIFIERAD) ---
        private async Task PlayTournamentAsync()
        {
            System.Console.Clear();
            System.Console.WriteLine(_strings.TournamentTitle);
            System.Console.WriteLine(_strings.CommonPressEscapeToCancel);

            string? p1Name = GetPlayerName(_strings.PromptPlayer1Name);
            if (p1Name == null) return;

            string? p2Name = GetPlayerName(_strings.PromptPlayer2Name);
            if (p2Name == null) return;

            var (provider, _) = SelectWordSource();
            if (provider == null) return;

            var tournament = new Hangman.Core.TwoPlayerGame(p1Name, p2Name, provider);

            System.Console.Clear();
            System.Console.WriteLine(_strings.FeedbackTournamentStarting(
                p1Name, p2Name, provider.DifficultyName,
                tournament.CurrentGuesser!.Name, Hangman.Core.TwoPlayerGame.MaxLives
            ));
            System.Console.WriteLine(_strings.FeedbackPressToStartRound);
            System.Console.ReadKey(true);

            while (tournament.TournamentStatus == GameStatus.InProgress)
            {
                Hangman.Core.Player currentGuesser = tournament.CurrentGuesser!;
                string secret;
                try
                {
                    secret = await tournament.StartNewRoundAsync();
                }
                catch (InvalidOperationException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine(_strings.ErrorCouldNotFetchTournamentWord(ex.Message));
                    System.Console.ResetColor();
                    System.Console.WriteLine(_strings.CommonPressAnyKeyToContinue);
                    System.Console.ReadKey(true);
                    return;
                }

                GameStatus roundResult = PlayRound(currentGuesser.Name, secret, currentGuesser.Lives);

                if (_game?.Status == GameStatus.Lost && _feedbackMessage == _strings.RoundFeedbackCancelled)
                {
                    tournament.HandleRoundEnd(GameStatus.Lost);
                    System.Console.WriteLine(_strings.FeedbackTournamentCancelled);
                    break;
                }

                tournament.HandleRoundEnd(roundResult);

                if (tournament.TournamentStatus == GameStatus.InProgress)
                {
                    System.Console.WriteLine(_strings.FeedbackTournamentRoundEnded);
                    System.Console.WriteLine(_strings.FeedbackTournamentLives(
                        tournament.Player1.Name, tournament.Player1.Lives,
                        tournament.Player2.Name, tournament.Player2.Lives
                    ));
                    System.Console.WriteLine(_strings.FeedbackTournamentNextGuesser(tournament.CurrentGuesser!.Name));
                    System.Console.ReadKey(true);
                }
            }

            if (_feedbackMessage != _strings.RoundFeedbackCancelled)
            {
                if (tournament.TournamentStatus == GameStatus.Draw)
                {
                    System.Console.Clear();
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine(_strings.FeedbackTournamentEnded);
                    System.Console.WriteLine(_strings.FeedbackTournamentDraw);
                    System.Console.ResetColor();
                }
                else
                {
                    Hangman.Core.Player? winner = tournament.GetWinner();

                    if (winner != null)
                    {
                        Hangman.Core.Player loser = (winner == tournament.Player1) ? tournament.Player2 : tournament.Player1;

                        System.Console.Clear();
                        System.Console.ForegroundColor = ConsoleColor.Green;
                        System.Console.WriteLine(_strings.FeedbackTournamentEnded);
                        System.Console.WriteLine(_strings.FeedbackTournamentWinner(winner.Name));
                        System.Console.ResetColor();
                        System.Console.WriteLine(_strings.FeedbackTournamentLoser(loser.Name));
                    }
                    else
                    {
                        System.Console.WriteLine(_strings.FeedbackTournamentUnexpectedEnd);
                    }
                }

                System.Console.WriteLine(_strings.FeedbackTournamentPlayerWins(p1Name, tournament.Player1.Wins));
                System.Console.WriteLine(_strings.FeedbackTournamentPlayerWins(p2Name, tournament.Player2.Wins));
            }

            System.Console.WriteLine(_strings.CommonPressAnyKeyToContinue);
            System.Console.ReadKey(true);
        }

        private (IAsyncWordProvider? Provider, WordDifficulty? Difficulty) SelectWordSource()
        {
            System.Console.Clear();
            System.Console.WriteLine(_strings.SelectWordSourceTitle);
            System.Console.WriteLine(_strings.SelectWordSourceApi);
            System.Console.WriteLine(_strings.SelectWordSourceLocal);
            System.Console.WriteLine(_strings.SelectWordSourceCustom);
            System.Console.WriteLine(_strings.CommonPressEscapeToCancel);
            System.Console.Write(_strings.SelectWordSourcePrompt);

            while (true)
            {
                var key = System.Console.ReadKey(intercept: true);
                WordDifficulty? difficulty;

                switch (key.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        System.Console.WriteLine(_strings.FeedbackWordSourceApi);
                        difficulty = SelectDifficulty(_strings.FeedbackWordSourceApi);
                        if (difficulty == null) return (null, null);
                        return (new ApiWordProvider(difficulty.Value), difficulty.Value);

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        System.Console.WriteLine(_strings.FeedbackWordSourceLocal);
                        difficulty = SelectDifficulty(_strings.FeedbackWordSourceLocal);
                        if (difficulty == null) return (null, null);
                        return (new WordProvider(difficulty.Value), difficulty.Value);

                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        System.Console.WriteLine(_strings.FeedbackWordSourceCustom);
                        difficulty = SelectDifficulty(_strings.FeedbackWordSourceCustom);
                        if (difficulty == null) return (null, null);
                        return (new CustomWordProvider(difficulty.Value), difficulty.Value);

                    case ConsoleKey.Escape:
                        System.Console.WriteLine(_strings.CommonFeedbackCancelling);
                        return (null, null);
                }
            }
        }

        private WordDifficulty? SelectDifficulty(string source)
        {
            System.Console.Clear();
            System.Console.WriteLine(_strings.SelectDifficultyTitle(source));
            System.Console.WriteLine(_strings.SelectDifficultyEasy);
            System.Console.WriteLine(_strings.SelectDifficultyMedium);
            System.Console.WriteLine(_strings.SelectDifficultyHard);
            System.Console.WriteLine(_strings.CommonPressEscapeToCancel);
            System.Console.Write(_strings.SelectDifficultyPrompt);

            while (true)
            {
                var key = System.Console.ReadKey(intercept: true);
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        System.Console.WriteLine(_strings.FeedbackDifficultyEasy);
                        return WordDifficulty.Easy;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        System.Console.WriteLine(_strings.FeedbackDifficultyMedium);
                        return WordDifficulty.Medium;

                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        System.Console.WriteLine(_strings.FeedbackDifficultyHard);
                        return WordDifficulty.Hard;

                    case ConsoleKey.Escape:
                        System.Console.WriteLine(_strings.CommonFeedbackCancelling);
                        return null;
                }
            }
        }

        private GameStatus PlayRound(string playerGuessing, string secret, int currentLives)
        {
            int maxMistakes = 6;

            _game = new Game(maxMistakes);
            _game.StartNew(secret);
            _feedbackMessage = string.Empty;

            System.Console.Clear();
            System.Console.WriteLine(_strings.RoundTitleNewRound);

            while (_game.Status == GameStatus.InProgress)
            {
                DrawGameScreen(playerGuessing, currentLives);

                char guess = GetGuess();
                if (guess == '\0')
                {
                    _game.ForceLose();
                    _feedbackMessage = _strings.RoundFeedbackCancelled;
                    continue;
                }

                bool wasCorrect = _game.Guess(guess);

                if (wasCorrect)
                {
                    _feedbackMessage = _strings.RoundFeedbackCorrectGuess(guess);
                }
                else
                {
                    _feedbackMessage = _strings.RoundFeedbackWrongGuess(guess);
                }
            }

            ShowEndScreen();

            if (_game.Status == GameStatus.Lost)
            {
                if (_feedbackMessage != _strings.RoundFeedbackCancelled)
                {
                    System.Console.WriteLine($"\n{playerGuessing} {_strings.RoundLost}");
                }
            }
            else
            {
                System.Console.WriteLine($"\n{playerGuessing} {_strings.RoundWon}");
            }

            return _game.Status;
        }

        private void DrawGameScreen(string playerGuessing, int currentLives)
        {
            System.Console.Clear();
            System.Console.WriteLine(_strings.RoundTitleGame);

            System.Console.ForegroundColor = ConsoleColor.Green;
            if (currentLives > 0)
            {
                System.Console.WriteLine(_strings.RoundActivePlayerWithLives(playerGuessing, currentLives));
            }
            else
            {
                System.Console.WriteLine($"{_strings.RoundActivePlayer} {playerGuessing}");
            }
            System.Console.ResetColor();

            DrawHangman(_game!.Mistakes);

            System.Console.WriteLine();

            string maskedWord = string.Join(" ", _game!.GetMaskedWord().ToCharArray());
            System.Console.WriteLine($"{_strings.RoundWord} {maskedWord}");

            System.Console.WriteLine();

            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine(_strings.RoundMistakes(_game!.Mistakes, 6));
            System.Console.ResetColor();

            System.Console.ForegroundColor = ConsoleColor.Yellow;
            var sortedLetters = _game!.UsedLetters.OrderBy(c => c);
            System.Console.WriteLine($"{_strings.RoundGuessedLetters} [ {string.Join(", ", sortedLetters)} ]");
            System.Console.ResetColor();

            System.Console.WriteLine("\n---------------------");

            if (!string.IsNullOrEmpty(_feedbackMessage))
            {
                if (_feedbackMessage.StartsWith("Korrekt") || _feedbackMessage.StartsWith("Correct"))
                {
                    System.Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                }
                System.Console.WriteLine(_feedbackMessage);
                System.Console.ResetColor();

                if (_feedbackMessage != _strings.RoundFeedbackCancelled)
                {
                    _feedbackMessage = string.Empty;
                }
            }
        }

        // Ritar gubben baserat på antal fel
        private void DrawHangman(int mistakes)
        {
            // ... (Oförändrad) ...
            int stage = Math.Clamp(mistakes, 0, HangmanStages.Length - 1);
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine(HangmanStages[stage]);
            System.Console.ResetColor();
        }

        // Hämtar en giltig bokstavsgissning från användaren
        private char GetGuess()
        {
            while (true)
            {
                System.Console.Write(_strings.GetGuessPrompt);

                var key = System.Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape)
                {
                    System.Console.WriteLine(_strings.CommonFeedbackCancelling);
                    return '\0';
                }

                char letter = key.KeyChar;

                if (!char.IsLetter(letter))
                {
                    System.Console.WriteLine(_strings.GetGuessInvalid(letter));
                    continue;
                }

                char upperGuess = char.ToUpperInvariant(letter);

                if (_game!.UsedLetters.Contains(upperGuess))
                {
                    System.Console.WriteLine(_strings.GetGuessAlreadyGuessed(upperGuess));
                    continue;
                }

                System.Console.WriteLine(upperGuess);
                return upperGuess;
            }
        }

        private void ShowEndScreen()
        {
            System.Console.Clear();

            DrawHangman(_game!.Mistakes);

            if (_game!.Status == GameStatus.Won)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine(_strings.EndScreenCongrats);
            }
            else // GameStatus.Lost
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                if (_feedbackMessage == _strings.RoundFeedbackCancelled)
                {
                    System.Console.WriteLine(_strings.EndScreenCancelled);
                }
                else
                {
                    System.Console.WriteLine(_strings.EndScreenLost);
                }
            }

            System.Console.ResetColor();
            System.Console.WriteLine(_strings.EndScreenCorrectWord(_game!.Secret));
        }

        // Hjälpmetod för att hämta spelarnamn
        private string? GetPlayerName(string prompt)
        {
            string? name;
            while (true)
            {
                name = GetInputString(prompt);

                if (name == null) return null;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name.Trim();
                }

                System.Console.WriteLine(_strings.GetPlayerNameEmpty);
            }
        }

        // NY HJÄLPMETOD: Ersätter Console.ReadLine() för att fånga Escape
        private string? GetInputString(string prompt)
        {
            System.Console.Write(prompt);
            var sb = new StringBuilder();

            while (true)
            {
                var key = System.Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape)
                {
                    System.Console.WriteLine($"\n{_strings.CommonFeedbackCancelling}");
                    return null;
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    System.Console.WriteLine();
                    return sb.ToString();
                }

                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Length--;
                    System.Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    System.Console.Write(key.KeyChar);
                }
            }
        }
    }
}