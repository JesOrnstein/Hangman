using Hangman.Console.Localizations;
using Hangman.Core;
using Hangman.Core.Exceptions;
using Hangman.Core.Models;
using Hangman.Core.Providers.Api;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Providers.Local;

namespace Hangman.Console
{
    /// <summary>
    /// Hjärnan i applikationen. Hanterar applikationsflödet,
    /// spellogik och koordinerar mellan Input och Renderer.
    /// </summary>
    public class GameController
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IUiStrings _strings;
        private readonly ConsoleInput _input;
        private readonly ConsoleRenderer _renderer;

        public GameController(IStatisticsService statisticsService, IUiStrings strings, ConsoleInput input, ConsoleRenderer renderer)
        {
            _statisticsService = statisticsService;
            _strings = strings;
            _input = input;
            _renderer = renderer;
        }

        /// <summary>
        /// Startar applikationen, visar välkomstskärm och huvudmenyloop.
        /// </summary>
        public async Task RunAsync()
        {
            _renderer.ShowWelcomeScreen();

            while (true)
            {
                _renderer.ShowMainMenu();
                var choice = _input.GetMenuChoice("123456");

                switch (choice)
                {
                    case '1':
                        await PlaySinglePlayerAsync();
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
                        _renderer.ShowHelpScreen();
                        break;
                    case '6':
                        _renderer.Clear();
                        _renderer.ShowFeedback(_strings.FeedbackThanksForPlaying);
                        return;
                }
            }
        }

        /// <summary>
        /// Hanterar flödet för enspelarläget.
        /// </summary>
        private async Task PlaySinglePlayerAsync()
        {
            var (provider, currentDifficulty) = SelectWordSource();
            if (provider == null || !currentDifficulty.HasValue) return;

            string? playerName = _input.GetPlayerName(_strings.PromptPlayerName);
            if (playerName == null) return;

            int consecutiveWins = 0;
            GameStatus roundResult;

            do
            {
                string? secret = await GetWordFromProviderAsync(provider);
                if (secret == null)
                {
                    roundResult = GameStatus.Lost; // Fel vid hämtning
                    break;
                }

                roundResult = PlayRound(playerName, secret, 0);

                if (roundResult == GameStatus.Won)
                {
                    consecutiveWins++;
                    _renderer.ShowFeedback(_strings.FeedbackWonRound(consecutiveWins), ConsoleColor.Green);

                    if (!_input.GetYesNo(_strings.PromptContinuePlaying))
                    {
                        roundResult = GameStatus.Lost; // Spelaren valde att sluta
                    }
                }
                else if (consecutiveWins > 0) // Förlorade men hade vinster
                {
                    _renderer.WaitForKey(_strings.FeedbackPressAnyKeyToSave);
                }

            } while (roundResult == GameStatus.Won);

            // Spara highscore om spelaren vann minst en runda
            if (consecutiveWins > 0)
            {
                // --- HÄR ÄR FIXEN (baserad på din gamla kod) ---
                // Vi använder en objekt-initialiserare { ... } för att
                // explicit sätta de 'required' fälten.
                var newScore = new HighscoreEntry(playerName, consecutiveWins, currentDifficulty.Value)
                {
                    PlayerName = playerName,
                    ConsecutiveWins = consecutiveWins,
                    Difficulty = currentDifficulty.Value
                };
                await _statisticsService.SaveHighscoreAsync(newScore);
                // --- SLUT PÅ FIX ---

                _renderer.ShowFeedback(_strings.FeedbackHighscoreSaved(consecutiveWins, currentDifficulty.Value), ConsoleColor.Green);
            }

            _renderer.WaitForKey(_strings.FeedbackReturningToMenu);
        }

        /// <summary>
        /// Hanterar flödet för tvåspelarläget.
        /// </summary>
        private async Task PlayTournamentAsync()
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.TournamentTitle, ConsoleColor.Cyan);

            string? p1Name = _input.GetPlayerName(_strings.PromptPlayer1Name);
            if (p1Name == null) return;

            string? p2Name = _input.GetPlayerName(_strings.PromptPlayer2Name);
            if (p2Name == null) return;

            var (provider, _) = SelectWordSource();
            if (provider == null) return;

            var tournament = new TwoPlayerGame(p1Name, p2Name, provider);

            _renderer.Clear();
            _renderer.ShowFeedback(_strings.FeedbackTournamentStarting(
                p1Name, p2Name, provider.DifficultyName,
                tournament.CurrentGuesser!.Name, TwoPlayerGame.MaxLives
            ), ConsoleColor.Green);
            _renderer.WaitForKey(_strings.FeedbackPressToStartRound);

            string roundFeedback = string.Empty;

            while (tournament.TournamentStatus == GameStatus.InProgress)
            {
                Player currentGuesser = tournament.CurrentGuesser!;
                string? secret;
                try
                {
                    secret = await tournament.StartNewRoundAsync();
                }
                catch (NoCustomWordsFoundException ex)
                {
                    _renderer.ShowError(_strings.ErrorNoCustomWordsFound(ex.Difficulty, ex.Language));
                    _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
                    return;
                }
                catch (InvalidOperationException) // Turneringen slut (förväntat)
                {
                    break;
                }
                catch (Exception ex) // Övriga fel
                {
                    _renderer.ShowError(_strings.ErrorCouldNotFetchTournamentWord(ex.Message));
                    _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
                    return;
                }

                GameStatus roundResult = PlayRound(currentGuesser.Name, secret, currentGuesser.Lives, out roundFeedback);

                // Hantera avbruten runda
                if (roundResult == GameStatus.Lost && roundFeedback == _strings.RoundFeedbackCancelled)
                {
                    tournament.HandleRoundEnd(GameStatus.Lost);
                    _renderer.ShowFeedback(_strings.FeedbackTournamentCancelled, ConsoleColor.DarkGray);
                    break;
                }

                tournament.HandleRoundEnd(roundResult);

                if (tournament.TournamentStatus == GameStatus.InProgress)
                {
                    _renderer.ShowFeedback(_strings.FeedbackTournamentRoundEnded);
                    _renderer.ShowFeedback(_strings.FeedbackTournamentLives(
                        tournament.Player1.Name, tournament.Player1.Lives,
                        tournament.Player2.Name, tournament.Player2.Lives
                    ), ConsoleColor.Yellow);
                    _renderer.WaitForKey(_strings.FeedbackTournamentNextGuesser(tournament.CurrentGuesser!.Name));
                }
            }

            // Visa slutresultat (om turneringen inte avbröts)
            if (roundFeedback != _strings.RoundFeedbackCancelled)
            {
                ShowTournamentResult(tournament);
            }

            _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
        }

        /// <summary>
        /// Spelar en enskild runda av Hänga Gubbe.
        /// </summary>
        private GameStatus PlayRound(string playerGuessing, string secret, int currentLives)
        {
            string feedback; // dummy out
            return PlayRound(playerGuessing, secret, currentLives, out feedback);
        }

        /// <summary>
        /// Spelar en enskild runda av Hänga Gubbe (överlagrad version).
        /// </summary>
        private GameStatus PlayRound(string playerGuessing, string secret, int currentLives, out string feedbackMessage)
        {
            var game = new Game(6);
            game.StartNew(secret);
            feedbackMessage = string.Empty;

            _renderer.Clear();
            _renderer.ShowFeedback(_strings.RoundTitleNewRound, ConsoleColor.Cyan);

            while (game.Status == GameStatus.InProgress)
            {
                _renderer.DrawGameScreen(game, playerGuessing, currentLives, feedbackMessage);
                feedbackMessage = string.Empty; // Nollställ efter ritning

                char guess = _input.GetGuess(game.UsedLetters);
                if (guess == '\0') // Escape
                {
                    game.ForceLose();
                    feedbackMessage = _strings.RoundFeedbackCancelled;
                    continue;
                }

                bool wasCorrect = game.Guess(guess);
                feedbackMessage = wasCorrect ?
                    _strings.RoundFeedbackCorrectGuess(guess) :
                    _strings.RoundFeedbackWrongGuess(guess);
            }

            _renderer.ShowEndScreen(game, feedbackMessage);

            // Skriv ut vem som vann/förlorade
            if (game.Status == GameStatus.Lost && feedbackMessage != _strings.RoundFeedbackCancelled)
                _renderer.ShowFeedback($"{playerGuessing} {_strings.RoundLost}", ConsoleColor.Red);
            else if (game.Status == GameStatus.Won)
                _renderer.ShowFeedback($"{playerGuessing} {_strings.RoundWon}", ConsoleColor.Green);

            return game.Status;
        }

        /// <summary>
        /// Hanterar logiken för att hämta highscores och visa dem.
        /// </summary>
        private async Task ShowHighscoresAsync()
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.HighscoreTitle, ConsoleColor.Cyan);
            _renderer.ShowFeedback(_strings.HighscoreFetching);
            try
            {
                var topScores = await _statisticsService.GetGlobalTopScoresAsync(10);
                _renderer.ShowHighscores(topScores);
            }
            catch (Exception ex)
            {
                _renderer.ShowError(_strings.CommonErrorDatabaseError(ex.Message));
                _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
            }
        }

        /// <summary>
        /// Hanterar logiken för att lägga till ett nytt anpassat ord.
        /// </summary>
        private async Task AddCustomWordAsync()
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.AddWordTitle, ConsoleColor.Cyan);
            _renderer.ShowFeedback(_strings.CommonPressEscapeToCancel);

            string? word = string.Empty;
            while (string.IsNullOrWhiteSpace(word) || !word.All(char.IsLetter))
            {
                word = _input.GetInputString(_strings.AddWordPrompt);
                if (word == null) return; // Avbröt

                if (string.IsNullOrWhiteSpace(word) || !word.All(char.IsLetter))
                    _renderer.ShowFeedback(_strings.AddWordInvalid, ConsoleColor.Yellow);
            }

            word = word.ToUpperInvariant();
            WordDifficulty difficulty = GetDifficultyByLength(word);

            WordLanguage? language = _input.SelectLanguage();
            if (language == null) return; // Avbröt

            try
            {
                var saver = new CustomWordProvider(difficulty, language.Value);
                await saver.AddWordAsync(word, difficulty, language.Value);
                _renderer.ShowFeedback(_strings.AddWordSuccess(word, difficulty, language.Value), ConsoleColor.Green);
            }
            catch (WordAlreadyExistsException ex)
            {
                _renderer.ShowError(_strings.ErrorWordAlreadyExists(ex.Word, ex.Difficulty, ex.Language));
            }
            catch (Exception ex)
            {
                _renderer.ShowError(_strings.CommonErrorDatabaseError(ex.Message));
            }

            _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
        }

        // --- Privata hjälpmetoder för flödeskontroll ---

        private WordDifficulty GetDifficultyByLength(string word)
        {
            if (word.Length <= 4) return WordDifficulty.Easy;
            if (word.Length <= 7) return WordDifficulty.Medium;
            return WordDifficulty.Hard;
        }

        private void ShowTournamentResult(TwoPlayerGame tournament)
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.FeedbackTournamentEnded, ConsoleColor.Cyan);

            if (tournament.TournamentStatus == GameStatus.Draw)
            {
                _renderer.ShowFeedback(_strings.FeedbackTournamentDraw, ConsoleColor.Yellow);
            }
            else
            {
                Player? winner = tournament.GetWinner();
                if (winner != null)
                {
                    Player loser = (winner == tournament.Player1) ? tournament.Player2 : tournament.Player1;
                    _renderer.ShowFeedback(_strings.FeedbackTournamentWinner(winner.Name), ConsoleColor.Green);
                    _renderer.ShowFeedback(_strings.FeedbackTournamentLoser(loser.Name));
                }
                else
                {
                    _renderer.ShowFeedback(_strings.FeedbackTournamentUnexpectedEnd);
                }
            }

            _renderer.ShowFeedback(_strings.FeedbackTournamentFinalWins);
            _renderer.ShowFeedback(_strings.FeedbackTournamentPlayerWins(tournament.Player1.Name, tournament.Player1.Wins));
            _renderer.ShowFeedback(_strings.FeedbackTournamentPlayerWins(tournament.Player2.Name, tournament.Player2.Wins));
        }

        private async Task<string?> GetWordFromProviderAsync(IAsyncWordProvider provider)
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.FeedbackFetchingWord(provider.DifficultyName));
            try
            {
                return await provider.GetWordAsync();
            }
            catch (NoCustomWordsFoundException ex)
            {
                _renderer.ShowError(_strings.ErrorNoCustomWordsFound(ex.Difficulty, ex.Language));
                _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
                return null;
            }
            catch (Exception ex)
            {
                _renderer.ShowError(_strings.ErrorCouldNotStartGame(ex.Message));
                _renderer.WaitForKey(_strings.CommonPressAnyKeyToContinue);
                return null;
            }
        }

        private WordDifficulty? SelectDifficulty(string source)
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.SelectDifficultyTitle(source), ConsoleColor.Cyan);
            _renderer.ShowFeedback(_strings.SelectDifficultyEasy);
            _renderer.ShowFeedback(_strings.SelectDifficultyMedium);
            _renderer.ShowFeedback(_strings.SelectDifficultyHard);
            _renderer.ShowFeedback(_strings.CommonPressEscapeToCancel);
            System.Console.Write(_strings.SelectDifficultyPrompt);

            var choice = _input.GetMenuChoice("123", allowEscape: true);
            switch (choice)
            {
                case '1': return WordDifficulty.Easy;
                case '2': return WordDifficulty.Medium;
                case '3': return WordDifficulty.Hard;
                default: return null;
            }
        }

        private (IAsyncWordProvider? Provider, WordDifficulty? Difficulty) SelectWordSource()
        {
            _renderer.Clear();
            _renderer.ShowFeedback(_strings.SelectWordSourceTitle, ConsoleColor.Cyan);
            _renderer.ShowFeedback(_strings.SelectWordSourceApi);
            _renderer.ShowFeedback(_strings.SelectWordSourceLocal);
            _renderer.ShowFeedback(_strings.SelectWordSourceCustomSwedish);
            _renderer.ShowFeedback(_strings.SelectWordSourceCustomEnglish);
            _renderer.ShowFeedback(_strings.CommonPressEscapeToCancel);
            System.Console.Write(_strings.SelectWordSourcePrompt);

            var choice = _input.GetMenuChoice("1234", allowEscape: true);
            WordDifficulty? difficulty;

            switch (choice)
            {
                case '1':
                    difficulty = SelectDifficulty(_strings.FeedbackWordSourceApi);
                    return difficulty.HasValue ? (new ApiWordProvider(difficulty.Value), difficulty) : (null, null);
                case '2':
                    difficulty = SelectDifficulty(_strings.FeedbackWordSourceLocal);
                    return difficulty.HasValue ? (new WordProvider(difficulty.Value), difficulty) : (null, null);
                case '3':
                    difficulty = SelectDifficulty(_strings.FeedbackWordSourceCustomSwedish);
                    return difficulty.HasValue ? (new CustomWordProvider(difficulty.Value, WordLanguage.Swedish), difficulty) : (null, null);
                case '4':
                    difficulty = SelectDifficulty(_strings.FeedbackWordSourceCustomEnglish);
                    return difficulty.HasValue ? (new CustomWordProvider(difficulty.Value, WordLanguage.English), difficulty) : (null, null);
                default:
                    return (null, null);
            }
        }
    }
}