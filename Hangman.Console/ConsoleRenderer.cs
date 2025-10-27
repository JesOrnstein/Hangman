using Hangman.Console.Localizations;
using Hangman.Core;
using Hangman.Core.Models;

namespace Hangman.Console
{
    /// <summary>
    /// Hanterar all rendering till konsolen.
    /// Ansvarar för att rita menyer, spelskärmar, ASCII-konst och feedback.
    /// </summary>
    public class ConsoleRenderer
    {
        private readonly IUiStrings _strings;

        // Visualisering av galgen (ASCII-konst)
        private static readonly string[] HangmanStages =
        {
            // 0-6 fel
            "\n    +---+\n    |   |\n        |\n        |\n        |\n        |\n    =======",
            "\n    +---+\n    |   |\n    O   |\n        |\n        |\n        |\n    =======",
            "\n    +---+\n    |   |\n    O   |\n    |   |\n        |\n        |\n    =======",
            "\n    +---+\n    |   |\n    O   |\n   /|   |\n        |\n        |\n    =======",
            "\n    +---+\n    |   |\n    O   |\n   /|\\  |\n        |\n        |\n    =======",
            "\n    +---+\n    |   |\n    O   |\n   /|\\  |\n   /    |\n        |\n    =======",
            "\n    +---+\n    |   |\n    O   |\n   /|\\  |\n   / \\  |\n        |\n    ======="
        };

        // NYTT: Fast position för timern (kolumn 50, rad 1)
        private const int TimerLeftPos = 50;
        private const int TimerTopPos = 1;

        // NYTT: Fast position för animation (under galgen)
        private const int AnimLeftPos = 4;
        private const int AnimTopPos = 10; // Galgen är ~7 rader hög, så 10 är under

        // NYTT: Definierar animationsramarna
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
        private const int AnimFrameCount = 12; // 0-11

        public ConsoleRenderer(IUiStrings strings)
        {
            _strings = strings;
        }

        public void Clear()
        {
            System.Console.Clear();
        }

        public void ShowWelcomeScreen()
        {
            Clear();
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine(_strings.WelcomeTitleArt);
            System.Console.ResetColor();
            System.Console.WriteLine(_strings.WelcomeMessage);
            System.Console.WriteLine(_strings.WelcomePressAnyKey);
            System.Console.ReadKey(true);
        }

        public void ShowMainMenu()
        {
            Clear();
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
        }

        /// <summary>
        /// Ritar spelskärmen baserat på spelets nuvarande tillstånd.
        /// </summary>
        public void DrawGameScreen(Game game, string playerGuessing, int currentLives, string feedbackMessage)
        {
            Clear();
            System.Console.WriteLine(_strings.RoundTitleGame);

            System.Console.ForegroundColor = ConsoleColor.Green;
            if (currentLives > 0)
                System.Console.WriteLine(_strings.RoundActivePlayerWithLives(playerGuessing, currentLives));
            else
                System.Console.WriteLine($"{_strings.RoundActivePlayer} {playerGuessing}");
            System.Console.ResetColor();

            DrawHangman(game.Mistakes);
            System.Console.WriteLine();

            string maskedWord = string.Join(" ", game.GetMaskedWord().ToCharArray());
            System.Console.WriteLine($"{_strings.RoundWord} {maskedWord}");
            System.Console.WriteLine();

            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine(_strings.RoundMistakes(game.Mistakes, game.AttemptsLeft + game.Mistakes));
            System.Console.ResetColor();

            System.Console.ForegroundColor = ConsoleColor.Yellow;
            var sortedLetters = game.UsedLetters.OrderBy(c => c);
            System.Console.WriteLine($"{_strings.RoundGuessedLetters} [ {string.Join(", ", sortedLetters)} ]");
            System.Console.ResetColor();

            System.Console.WriteLine("\n---------------------");

            if (!string.IsNullOrEmpty(feedbackMessage))
            {
                if (feedbackMessage == _strings.RoundFeedbackCancelled)
                    System.Console.ForegroundColor = ConsoleColor.DarkGray;
                else if (feedbackMessage.StartsWith(_strings.RoundFeedbackCorrectGuess('X').Substring(0, 5)))
                    System.Console.ForegroundColor = ConsoleColor.Green;
                else
                    System.Console.ForegroundColor = ConsoleColor.Yellow;

                System.Console.WriteLine(feedbackMessage);
                System.Console.ResetColor();
            }

            // NYTT: Skriv ut gissningsprompten här istället för i ConsoleInput
            // Detta säkerställer att markören återställs korrekt efter timer-uppdateringar
            System.Console.Write(_strings.GetGuessPrompt);
        }

        /// <summary>
        /// Ritar galgen baserat på antal fel.
        /// </summary>
        public void DrawHangman(int mistakes)
        {
            int stage = Math.Clamp(mistakes, 0, HangmanStages.Length - 1);
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine(HangmanStages[stage]);
            System.Console.ResetColor();
        }

        /// <summary>
        /// Visar slutskärmen för en avslutad runda.
        /// </summary>
        public void ShowEndScreen(Game game, string feedbackMessage)
        {
            Clear();
            DrawHangman(game.Mistakes);
            System.Console.WriteLine();

            if (game.Status == GameStatus.Won)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine(_strings.EndScreenCongrats);
            }
            else // Lost
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                if (feedbackMessage == _strings.RoundFeedbackCancelled)
                    System.Console.WriteLine(_strings.EndScreenCancelled);
                else
                    System.Console.WriteLine(_strings.EndScreenLost);
            }
            System.Console.ResetColor();
            System.Console.WriteLine(_strings.EndScreenCorrectWord(game.Secret));
            System.Console.WriteLine();
        }

        /// <summary>
        /// Ritar hjälpskärmen.
        /// </summary>
        public void ShowHelpScreen()
        {
            Clear();
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
            WaitForKey(_strings.CommonPressAnyKeyToContinue);
        }

        /// <summary>
        /// Visar Highscore-listan.
        /// </summary>
        public void ShowHighscores(List<HighscoreEntry> topScores)
        {
            Clear();
            System.Console.WriteLine(_strings.HighscoreTitle);

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
            WaitForKey(_strings.CommonPressAnyKeyToContinue);
        }

        /// <summary>
        /// Visar ett färglagt feedback-meddelande.
        /// </summary>
        public void ShowFeedback(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(message);
            System.Console.ResetColor();
        }

        /// <summary>
        /// Visar ett standardiserat felmeddelande.
        /// </summary>
        public void ShowError(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine(message);
            System.Console.ResetColor();
        }

        /// <summary>
        /// Visar en prompt och väntar på en tangenttryckning.
        /// </summary>
        public void WaitForKey(string prompt)
        {
            System.Console.WriteLine(prompt);
            System.Console.ReadKey(true);
        }

        // --- NYA METODER FÖR TIMER & ANIMATION ---

        /// <summary>
        /// Ritar timer-displayen på en fast position.
        /// Används av den parallella timer-tasken.
        /// </summary>
        public void DrawTimer(int secondsLeft)
        {
            // Spara nuvarande markörposition
            int originalLeft = System.Console.CursorLeft;
            int originalTop = System.Console.CursorTop;

            System.Console.SetCursorPosition(TimerLeftPos, TimerTopPos);
            System.Console.ForegroundColor = ConsoleColor.Magenta;
            System.Console.Write(_strings.RoundTimerDisplay(secondsLeft));
            System.Console.ResetColor();

            // Återställ markören
            System.Console.SetCursorPosition(originalLeft, originalTop);
        }

        /// <summary>
        /// Rensar timer-displayen när rundan är över.
        /// </summary>
        public void ClearTimerArea()
        {
            int originalLeft = System.Console.CursorLeft;
            int originalTop = System.Console.CursorTop;

            System.Console.SetCursorPosition(TimerLeftPos, TimerTopPos);
            // Skriv över med blanksteg
            System.Console.Write(new string(' ', _strings.RoundTimerDisplay(99).Length));

            System.Console.SetCursorPosition(originalLeft, originalTop);
        }

        /// <summary>
        /// NY METOD: Ritar den parallella "hänggubbe-animationen" (knarrande).
        /// Används av den parallella timer-tasken.
        /// </summary>
        public void DrawAnimation(int secondsLeft)
        {
            // Välj en frame baserat på sekunden (modulo AnimFrameCount)
            int frame = secondsLeft % AnimFrameCount;
            string text = _animFrames[frame];

            // Spara nuvarande markörposition
            int originalLeft = System.Console.CursorLeft;
            int originalTop = System.Console.CursorTop;

            System.Console.SetCursorPosition(AnimLeftPos, AnimTopPos);
            System.Console.ForegroundColor = ConsoleColor.DarkGray; // Mörkgrå färg
            System.Console.Write(text);
            System.Console.ResetColor();

            // Återställ markören
            System.Console.SetCursorPosition(originalLeft, originalTop);
        }

        /// <summary>
        /// NY METOD: Rensar animations-displayen när rundan är över.
        /// </summary>
        public void ClearAnimationArea()
        {
            int originalLeft = System.Console.CursorLeft;
            int originalTop = System.Console.CursorTop;

            System.Console.SetCursorPosition(AnimLeftPos, AnimTopPos);
            // Skriv över med blanksteg (baserat på längsta frame)
            System.Console.Write(new string(' ', _animFrames[0].Length));

            System.Console.SetCursorPosition(originalLeft, originalTop);
        }
    }
}