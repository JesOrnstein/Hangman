using Hangman.Core;
using Hangman.Core.Providers.Api;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.IO
{
    /// <summary>
    /// Hanterar all input och output i konsolen för spelet Hänga Gubbe.
    /// </summary>
    public class ConsoleUi
    {
        private Game? _game;

        private string _feedbackMessage = string.Empty;

        // Visualisering av galgen i ASCII-konst. Index = antal fel.
        private static readonly string[] HangmanStages =
    {
            // 0 fel
            @"
   +---+
   |   |
       |
       |
       |
       |
  =======",
            // 1 fel
            @"
   +---+
   |   |
   O   |
       |
       |
       |
  =======",
            // 2 fel
            @"
   +---+
   |   |
   O   |
   |   |
       |
       |
  =======",
            // 3 fel
            @"
   +---+
   |   |
   O   |
  /|   |
       |
       |
  =======",
            // 4 fel
            @"
   +---+
   |   |
   O   |
  /|\  |
       |
       |
  =======",
            // 5 fel
            @"
   +---+
   |   |
   O   |
  /|\  |
  /    |
       |
  =======",
            // 6 fel (Max)
            @"
   +---+
   |   |
   O   |
  /|\  |
  / \  |
       |
  ======="
    };


        public ConsoleUi()
        {
        }

        /// <summary>
        /// Huvudloopen för konsolgränssnittet.
        /// </summary>
        public async Task RunAsync()
        {
            Console.WriteLine("Välkommen till Hänga Gubbe!");

            while (true)
            {
                var choice = ShowMainMenu();
                switch (choice)
                {
                    case '1':
                        await PlaySinglePlayerAsync();
                        break;
                    case '2':
                        await PlayTournamentAsync(); // NY: Starta turneringsläge
                        break;
                    case '3':
                        Console.Clear();
                        Console.WriteLine("Statistik är inte implementerat än.");
                        Console.ReadKey();
                        break;
                    case '4': // Uppdaterat för Avsluta
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
            Console.WriteLine("1. Spela (Enspelare)");
            Console.WriteLine("2. Spela (Tvåspelare, turnering)"); // Nytt alternativ
            Console.WriteLine("3. Visa statistik");
            Console.WriteLine("4. Avsluta");
            Console.Write("\nVälj (1-4): ");

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.KeyChar == '1' || key.KeyChar == '2' || key.KeyChar == '3' || key.KeyChar == '4')
                {
                    Console.WriteLine(key.KeyChar);
                    return key.KeyChar;
                }
            }
        }

        // --- Huvudloop för enspelare ---
        private async Task PlaySinglePlayerAsync()
        {
            var provider = SelectWordSource();

            if (provider == null) return;

            // Hämtar ordet asynkront
            Console.Clear();
            Console.WriteLine($"Hämtar ord från: {provider.DifficultyName}...");
            string secret;
            try
            {
                secret = await provider.GetWordAsync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nKunde inte starta spelet (Ordlistefel):");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.WriteLine("Tryck valfri tangent för att återgå till menyn...");
                Console.ReadKey();
                return;
            }

            // Kör en runda med det hämtade ordet. Skickar 0 för liv eftersom det är enspelarläge.
            PlayRound("Spelare 1", secret, 0);
        }

        // --- Huvudloop för 2-spelarturnering ---
        private async Task PlayTournamentAsync()
        {
            Console.Clear();
            Console.WriteLine("--- TVÅSPELARTURNERING ---");

            // 1. Hämta spelarnamn och ordkälla (som gäller för alla rundor)
            string p1Name = GetPlayerName("Ange namn för Spelare 1: ");
            string p2Name = GetPlayerName("Ange namn för Spelare 2: ");
            IAsyncWordProvider? provider = SelectWordSource();

            if (provider == null) return;

            // 2. Skapa turneringsmotorn
            var tournament = new TwoPlayerGame(p1Name, p2Name, provider);

            Console.Clear();
            Console.WriteLine($"Turneringen startar! {p1Name} mot {p2Name}. Ordkälla: {provider.DifficultyName}");
            Console.WriteLine($"Första gissare: {tournament.CurrentGuesser!.Name}. Liv: {TwoPlayerGame.MaxLives} vardera.");
            Console.WriteLine("Tryck valfri tangent för att starta den första rundan...");
            Console.ReadKey(true);

            // 3. Huvudloop för turneringen
            while (tournament.TournamentStatus == GameStatus.InProgress)
            {
                Player currentGuesser = tournament.CurrentGuesser!;

                // Hämta ordet asynkront
                string secret;
                try
                {
                    secret = await tournament.StartNewRoundAsync();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nKunde inte hämta ord till turneringen:");
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                    Console.WriteLine("Turneringen avbryts. Tryck för att återgå...");
                    Console.ReadKey(true);
                    return;
                }

                // Kör rundan och få resultatet
                GameStatus roundResult = PlayRound(currentGuesser.Name, secret, currentGuesser.Lives);

                // Hantera resultat och byt gissare
                tournament.HandleRoundEnd(roundResult);

                if (tournament.TournamentStatus == GameStatus.InProgress)
                {
                    Console.WriteLine($"\n--- RUNDA AVSLUTAD ---");
                    Console.WriteLine($"{p1Name} Liv: {tournament.Player1.Lives} | {p2Name} Liv: {tournament.Player2.Lives}");
                    Console.WriteLine($"Nästa gissare: {tournament.CurrentGuesser!.Name}. Tryck för nästa runda...");
                    Console.ReadKey(true);
                }
            }

            // 4. Visa Vinnaren
            Player winner = tournament.GetWinner()!;
            Player loser = (winner == tournament.Player1) ? tournament.Player2 : tournament.Player1;

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n--- TURNERING AVSLUTAD ---");
            Console.WriteLine($"GRATTIS, {winner.Name} VANN TURNERINGEN!");
            Console.ResetColor();
            Console.WriteLine($"{loser.Name} förlorade alla sina liv. Vunna rundor:");
            Console.WriteLine($"- {p1Name}: {tournament.Player1.Wins} rundor");
            Console.WriteLine($"- {p2Name}: {tournament.Player2.Wins} rundor");

            Console.WriteLine("\nTryck valfri tangent för att återgå till menyn...");
            Console.ReadKey(true);
        }

        /// <summary>
        /// Låter användaren välja ordkälla (API eller Lokal lista).
        /// </summary>
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
                        Console.WriteLine("Engelska (API)");
                        WordDifficulty difficultyApi = SelectDifficulty("API");
                        return new ApiWordProvider(difficultyApi);

                    case '2':
                        Console.WriteLine("Svenska (Lokal)");
                        WordDifficulty difficultyLocal = SelectDifficulty("LOKAL");
                        return new WordProvider(difficultyLocal);
                }
            }
        }

        /// <summary>
        /// Låter användaren välja svårighetsgrad.
        /// </summary>
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

        /// <summary>
        /// NY GENERISK METOD: Kör en enda runda med ett givet hemligt ord.
        /// </summary>
        /// <param name="playerGuessing">Namnet på spelaren som gissar.</param>
        /// <param name="secret">Det hemliga ordet.</param>
        /// <param name="currentLives">Antal liv spelaren har kvar (0 om enspelare).</param>
        /// <returns>Slutstatusen för rundan (Won eller Lost).</returns>
        private GameStatus PlayRound(string playerGuessing, string secret, int currentLives)
        {
            // Hårkodad max mistakes som matchar Game.cs
            int maxMistakes = 6;

            _game = new Game(maxMistakes);
            _game.StartNew(secret);
            _feedbackMessage = string.Empty;

            Console.Clear();
            Console.WriteLine($"--- NY RUNDA STARTAD ---");

            while (_game.Status == GameStatus.InProgress)
            {
                // UPPDATERAT: Skickar med både namn och liv
                DrawGameScreen(playerGuessing, currentLives);

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

            // För SinglePlayer stannar loopen här. För Tournament Game återgår den till PlayTournamentAsync.
            if (_game.Status == GameStatus.Lost)
            {
                Console.WriteLine($"\n{playerGuessing} förlorade rundan.");
            }
            else
            {
                Console.WriteLine($"\n{playerGuessing} vann rundan!");
            }

            // Returnera resultatet av denna runda
            return _game.Status;
        }

        // KORRIGERAD METODSIGNATUR: Lagt till int currentLives
        private void DrawGameScreen(string playerGuessing, int currentLives)
        {
            Console.Clear();
            Console.WriteLine("--- HÄNGA GUBBE ---");

            // Visar namnet OCH liv
            Console.ForegroundColor = ConsoleColor.Green;
            if (currentLives > 0)
            {
                // Visar liv endast i turneringsläge (när currentLives > 0)
                Console.WriteLine($"Aktiv spelare: {playerGuessing} | Liv: {currentLives}/{TwoPlayerGame.MaxLives}");
            }
            else
            {
                // Enspelarläge
                Console.WriteLine($"Aktiv spelare: {playerGuessing}");
            }
            Console.ResetColor();

            DrawHangman(_game!.Mistakes);

            Console.WriteLine();

            string maskedWord = string.Join(" ", _game!.GetMaskedWord().ToCharArray());
            Console.WriteLine($"Ordet: {maskedWord}");

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Red;
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

        // Ritar gubben baserat på antal fel
        private void DrawHangman(int mistakes)
        {
            int stage = Math.Clamp(mistakes, 0, HangmanStages.Length - 1);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(HangmanStages[stage]);
            Console.ResetColor();
        }

        // Hämtar en giltig bokstavsgissning från användaren
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

        private void ShowEndScreen()
        {
            Console.Clear();

            DrawHangman(_game!.Mistakes);

            if (_game!.Status == GameStatus.Won)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nGRATTIS, DU VANN!");
            }
            else // GameStatus.Lost
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nDU FÖRLORADE...");
            }

            Console.ResetColor();
            Console.WriteLine($"Det rätta ordet var: {_game!.Secret}");
        }

        // Hjälpmetod för att hämta spelarnamn
        private string GetPlayerName(string prompt)
        {
            string? name = string.Empty;
            while (string.IsNullOrWhiteSpace(name))
            {
                Console.Write(prompt);
                name = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Namnet kan inte vara tomt.");
                }
            }
            return name;
        }
    }
}