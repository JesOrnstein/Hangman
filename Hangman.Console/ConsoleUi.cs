using Hangman.Core;
using Hangman.Core.Providers.Api;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.IO
{
    public class ConsoleUi
    {
        private Game? _game;
        private IAsyncWordProvider? _provider;

        private string _feedbackMessage = string.Empty;

        // ... HangmanStages-arrayen är oförändrad ...
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


        public ConsoleUi()
        {
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Välkommen till Hänga Gubbe!");

            while (true)
            {
                var choice = ShowMainMenu();
                switch (choice)
                {
                    case '1':
                        await PlayNewGameAsync();
                        break;
                    case '2':
                        Console.Clear();
                        Console.WriteLine("Statistik är inte implementerat än.");
                        Console.WriteLine("Tryck valfri tangent för att återgå...");
                        Console.ReadKey();
                        break;
                    case '3':
                        Console.Clear();
                        Console.WriteLine("Tack för att du spelade!");
                        return;
                }
            }
        }

        private char ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("--- HUVUDMENY ---");
            Console.WriteLine("1. Spela");
            Console.WriteLine("2. Visa statistik");
            Console.WriteLine("3. Avsluta");
            Console.Write("\nVälj (1-3): ");

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.KeyChar == '1' || key.KeyChar == '2' || key.KeyChar == '3')
                {
                    Console.WriteLine(key.KeyChar);
                    return key.KeyChar;
                }
            }
        }

        private IAsyncWordProvider? SelectWordSource()
        {
            Console.Clear();
            Console.WriteLine("--- VÄLJ ORDLISTA ---");
            Console.WriteLine("1. Engelska (API - olika längd)");
            Console.WriteLine("2. Svenska (Lokal lista)");
            Console.Write("\nVälj (1-2): ");

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                switch (key.KeyChar)
                {
                    case '1':
                        // Om vi väljer API, visa svårighetsgrad
                        Console.WriteLine("Engelska (API)");
                        WordDifficulty difficultyApi = SelectDifficulty("API"); 
                        return new ApiWordProvider(difficultyApi);

                    case '2':
                        // Lokal lista med svårighetsgrad
                        Console.WriteLine("Svenska (Lokal)");
                        WordDifficulty difficultyLocal = SelectDifficulty("LOKAL"); 
                        return new WordProvider(difficultyLocal); 
                }
            }
        }

        // SelectDifficulty uppdaterad med WordDifficulty och för att ta emot en sträng
        private WordDifficulty SelectDifficulty(string source)
        {
            Console.Clear();
            Console.WriteLine($"--- VÄLJ SVÅRIGHETSGRAD ({source}) ---");
            Console.WriteLine("1. Lätt (3-4 bokstäver)");
            Console.WriteLine("2. Medium (5-7 bokstäver)");
            Console.WriteLine("3. Svår (8-11 bokstäver)");
            Console.Write("\nVälj (1-3): ");

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                switch (key.KeyChar)
                {
                    case '1':
                        Console.WriteLine("Lätt");
                        return WordDifficulty.Easy;
                    case '2':
                        Console.WriteLine("Medium");
                        return WordDifficulty.Medium;
                    case '3':
                        Console.WriteLine("Svår");
                        return WordDifficulty.Hard;
                }
            }
        }

       
        private async Task PlayNewGameAsync()
        {
            // 1. Välj provider. Denna kan nu returnera null
            _provider = SelectWordSource();

            // 2. Kontrollera om användaren valde ett icke-implementerat alternativ
            if (_provider == null)
            {
                return; // Gå tillbaka till huvudmenyn
            }

            // 3. Antal misstag är nu hårdkodat till 6. Denna borde flyttas till logic i Hangman.Core.Game för att dölja logic?
            int maxMistakes = 6;

            // 4. Skapa spelet
            _game = new Game(maxMistakes); //

            Console.Clear();
            Console.WriteLine($"Hämtar ord från: {_provider.DifficultyName}...");

            string secret;
            try
            {
                secret = await _provider.GetWordAsync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                // Mer generiskt meddelande för att hantera fel från både API och Lokal
                Console.WriteLine("\nKunde inte starta spelet (Ordlistefel):");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.WriteLine("Tryck valfri tangent för att återgå till menyn...");
                Console.ReadKey();
                return;
            }

            _game.StartNew(secret);
            _feedbackMessage = string.Empty;

            while (_game.Status == GameStatus.InProgress)
            {
                DrawGameScreen();
                char guess = GetGuess();
                bool wasCorrect = _game.Guess(guess);

                if (wasCorrect)
                {
                    _feedbackMessage = $"Korrekt! '{guess}' finns i ordet.";
                }
                else
                {
                    _feedbackMessage = $"Fel! '{guess}' finns inte i ordet.";
                }
            }

            ShowEndScreen();

            Console.WriteLine("\nTryck valfri tangent för att återgå till menyn...");
            Console.ReadKey();
        }

        private void DrawGameScreen()
        {
            Console.Clear();
            Console.WriteLine("--- HÄNGA GUBBE ---");

            DrawHangman(_game!.Mistakes);

            Console.WriteLine();

            string maskedWord = string.Join(" ", _game!.GetMaskedWord().ToCharArray());
            Console.WriteLine($"Ordet: {maskedWord}");

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Red;
            // Uppdaterad text för att visa det fasta värdet 6
            Console.WriteLine($"Felgissningar: {_game!.Mistakes} (av 6)");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            var sortedLetters = _game!.UsedLetters.OrderBy(c => c);
            Console.WriteLine($"Gissade: [ {string.Join(", ", sortedLetters)} ]");
            Console.ResetColor();

            Console.WriteLine("\n---------------------");

            if (!string.IsNullOrEmpty(_feedbackMessage))
            {
                if (_feedbackMessage.StartsWith("Korrekt"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                Console.WriteLine(_feedbackMessage);
                Console.ResetColor();
                _feedbackMessage = string.Empty;
            }
        }

        // Ritar gubben
        private void DrawHangman(int mistakes)
        {
            int stage = Math.Clamp(mistakes, 0, HangmanStages.Length - 1);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(HangmanStages[stage]);
            Console.ResetColor();
        }

        // Hämtar gissning
        private char GetGuess()
        {
            while (true)
            {
                Console.Write("Gissa på en bokstav: ");

                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input) || input.Length > 1)
                {
                    Console.WriteLine("Ogiltig gissning. Skriv EN bokstav.");
                    continue;
                }

                char letter = input[0];

                if (!char.IsLetter(letter))
                {
                    Console.WriteLine("Ogiltig gissning. Endast bokstäver (A-Ö) är tillåtna.");
                    continue;
                }

                char upperGuess = char.ToUpperInvariant(letter);

                if (_game!.UsedLetters.Contains(upperGuess))
                {
                    Console.WriteLine($"Du har redan gissat på '{upperGuess}'. Försök igen.");
                    continue;
                }

                return upperGuess;
            }
        }

        // Visar slutskärmen
        private void ShowEndScreen()
        {
            Console.Clear();

            DrawHangman(_game!.Mistakes);

            if (_game!.Status == GameStatus.Won)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nGRATTIS, DU VANN!");
            }
            else // Måste vara GameStatus.Lost
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nDU FÖRLORADE...");
            }

            Console.ResetColor();
            Console.WriteLine($"Det rätta ordet var: {_game!.Secret}");
        }
    }
}