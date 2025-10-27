using Hangman.Console.Localizations;
using Hangman.Core.Models;
using System.Text;

namespace Hangman.Console
{
    /// <summary>
    /// Hanterar all inmatning från konsolen.
    /// Ansvarar för att hämta tangenter, strängar och validera inmatning.
    /// </summary>
    public class ConsoleInput
    {
        private readonly IUiStrings _strings;

        public ConsoleInput(IUiStrings strings)
        {
            _strings = strings;
        }

        /// <summary>
        /// Hämtar en enskild bokstavsgissning.
        /// Validerar att det är en bokstav och att den inte redan gissats.
        /// Returnerar '\0' om användaren trycker Escape.
        /// </summary>
        public char GetGuess(IEnumerable<char> usedLetters)
        {
            while (true)
            {
                System.Console.Write(_strings.GetGuessPrompt);
                var key = System.Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape)
                {
                    System.Console.WriteLine();
                    return '\0'; // Signalera Escape
                }

                char letter = key.KeyChar;

                if (!char.IsLetter(letter))
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine(_strings.GetGuessInvalid(letter));
                    System.Console.ResetColor();
                    continue;
                }

                char upperGuess = char.ToUpperInvariant(letter);

                if (usedLetters.Contains(upperGuess))
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine(_strings.GetGuessAlreadyGuessed(upperGuess));
                    System.Console.ResetColor();
                    continue;
                }

                System.Console.WriteLine(upperGuess); // Eka den giltiga gissningen
                return upperGuess;
            }
        }

        /// <summary>
        /// Hämtar ett spelarnamn. Säkerställer att det inte är tomt.
        /// Returnerar null om användaren trycker Escape.
        /// </summary>
        public string? GetPlayerName(string prompt)
        {
            string? name;
            while (true)
            {
                name = GetInputString(prompt); // Använder GetInputString

                if (name == null) return null; // Användaren tryckte Escape

                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name.Trim(); // Returnera trimmade namnet
                }

                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine(_strings.GetPlayerNameEmpty);
                System.Console.ResetColor();
            }
        }

        /// <summary>
        /// Generell metod för att hämta en sträng från användaren.
        /// Hanterar Backspace och Escape.
        /// </summary>
        public string? GetInputString(string prompt)
        {
            System.Console.Write(prompt);
            var sb = new StringBuilder();

            while (true)
            {
                var key = System.Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape)
                {
                    System.Console.WriteLine($"\n{_strings.CommonFeedbackCancelling}");
                    return null; // Signalera Escape
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

        /// <summary>
        /// Väntar på ett menyval (t.ex. '1'-'6').
        /// Returnerar '\0' vid Escape.
        /// </summary>
        public char GetMenuChoice(string validChars, bool allowEscape = false)
        {
            while (true)
            {
                var key = System.Console.ReadKey(intercept: true);

                if (allowEscape && key.Key == ConsoleKey.Escape)
                {
                    System.Console.WriteLine(_strings.CommonFeedbackCancelling);
                    return '\0';
                }

                if (validChars.Contains(key.KeyChar))
                {
                    System.Console.WriteLine(key.KeyChar);
                    return key.KeyChar;
                }
            }
        }

        /// <summary>
        /// Hämtar ett Ja/Nej-svar.
        /// Returnerar true för 'J' (eller 'Y'), false för 'N' eller Escape.
        /// </summary>
        public bool GetYesNo(string prompt)
        {
            System.Console.Write(prompt);
            while (true)
            {
                var key = System.Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape ||
                    key.KeyChar == 'n' || key.KeyChar == 'N')
                {
                    System.Console.WriteLine(_strings.FeedbackEndingLoop);
                    return false;
                }

                if (key.KeyChar == 'j' || key.KeyChar == 'J' ||
                    key.KeyChar == 'y' || key.KeyChar == 'Y')
                {
                    System.Console.WriteLine(_strings.FeedbackContinuing);
                    return true;
                }
            }
        }

        /// <summary>
        /// Visar menyn för att välja språk (för anpassade ord).
        /// Returnerar null vid Escape.
        /// </summary>
        public WordLanguage? SelectLanguage()
        {
            System.Console.Clear();
            System.Console.WriteLine(_strings.AddWordSelectLanguageTitle);
            System.Console.WriteLine(_strings.AddWordLanguageSwedish);
            System.Console.WriteLine(_strings.AddWordLanguageEnglish);
            System.Console.WriteLine(_strings.CommonPressEscapeToCancel);
            System.Console.Write(_strings.AddWordSelectLanguagePrompt);

            var choice = GetMenuChoice("12", allowEscape: true);
            switch (choice)
            {
                case '1':
                    return WordLanguage.Swedish;
                case '2':
                    return WordLanguage.English;
                default:
                    return null;
            }
        }
    }
}