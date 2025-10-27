using Hangman.Core;
using Hangman.Core.Providers.Api;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Models;
using Hangman.Core.Providers.Local;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace Hangman.IO
{
    /// <summary>
    /// Hanterar all input och output i konsolen för spelet Hänga Gubbe.
    /// </summary>
    public class ConsoleUi
    {
        private Game? _game;

        private string _feedbackMessage = string.Empty;

        // NY: Statistik-tjänsten injiceras
        private readonly IStatisticsService _statisticsService;

        // Visualisering av galgen i ASCII-konst. Index = antal fel.
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

        // NY KONSTRUKTOR SOM ACCEPTERAR BEROENDEN (Dependency Injection)
        public ConsoleUi(IStatisticsService statsService)
        {
            _statisticsService = statsService;
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
                        await PlaySinglePlayerHighscoreAsync(); // NYTT HIGHSCRE-FLÖDE
                        break;
                    case '2':
                        await PlayTournamentAsync();
                        break;
                    case '3':
                        await ShowHighscoresAsync(); // NYTT
                        break;
                    case '4':
                        await ShowSettingsMenu(); // NYTT
                        break;
                    case '5':
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
            Console.WriteLine("1. Spela (Enspelare - Highscoreläge)");
            Console.WriteLine("2. Spela (Tvåspelare, turnering)");
            Console.WriteLine("3. Highscores");
            Console.WriteLine("4. Inställningar/Verktyg");
            Console.WriteLine("5. Avsluta");
            Console.Write("\nVälj (1-5): ");

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.KeyChar >= '1' && key.KeyChar <= '5')
                {
                    Console.WriteLine(key.KeyChar);
                    return key.KeyChar;
                }
            }
        }

        // --- NY Huvudloop för enspelare med Highscore-logik ---
        private async Task PlaySinglePlayerHighscoreAsync()
        {
            // 1. Välj ordkälla och svårighetsgrad
            var (provider, currentDifficulty) = SelectWordSource();

            if (provider == null) return;

            // Hämta spelarnamn
            string playerName = GetPlayerName("Ange ditt namn för highscore: ");
            int consecutiveWins = 0;
            GameStatus roundResult;

            do
            {
                // Förbered ordhämtning
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
                    roundResult = GameStatus.Lost;
                    break;
                }

                // Kör runda (0 för liv = enspelarläge)
                roundResult = PlayRound(playerName, secret, currentLives: 0);

                if (roundResult == GameStatus.Won)
                {
                    consecutiveWins++;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n** Du vann runda {consecutiveWins} i rad! **");
                    Console.ResetColor();

                    // Fråga om spelaren vill fortsätta
                    Console.Write("\nVill du fortsätta spela och försöka förbättra ditt highscore (J/N)? ");
                    var key = Console.ReadKey(intercept: true);
                    if (char.ToUpperInvariant(key.KeyChar) != 'J')
                    {
                        roundResult = GameStatus.Lost;
                        Console.WriteLine("\nAvslutar spelloop...");
                    }
                    else
                    {
                        Console.WriteLine("\nFortsätter!");
                    }
                }
                else // roundResult == GameStatus.Lost
                {
                    Console.WriteLine("\nTryck valfri tangent för att spara highscore...");
                    Console.ReadKey(true);
                }


            } while (roundResult == GameStatus.Won);

            // 2. Spara Highscore
            if (consecutiveWins > 0)
            {
                // Fix for CS9035: ensure required members are set.
                // Use an object initializer (combined with ctor for safety) to set all required properties.
                var newScore = new HighscoreEntry(playerName, consecutiveWins, currentDifficulty)
                {
                    PlayerName = playerName,
                    ConsecutiveWins = consecutiveWins,
                    Difficulty = currentDifficulty
                };
                await _statisticsService.SaveHighscoreAsync(newScore);

                Console.WriteLine($"\nHighscore ({consecutiveWins} vinster i rad) sparat för {currentDifficulty}!");
            }

            Console.WriteLine("\nÅtergår till huvudmenyn...");
            Console.ReadKey(true);
        }

        // --- NY METOD: Visa Highscores ---
        private async Task ShowHighscoresAsync()
        {
            Console.Clear();
            Console.WriteLine("--- HIGHSCORES ---");
            // ÄNDRAD TEXT: 5 -> 10
            Console.WriteLine("Hämtar globala topp 10-resultat...");

            try
            {
                // ÄNDRAD KOD: 5 -> 10
                var topScores = await _statisticsService.GetGlobalTopScoresAsync(10);

                if (!topScores.Any())
                {
                    Console.WriteLine("\nInga highscores sparade än. Spela en runda!");
                }
                else
                {
                    foreach (var diffGroup in topScores.GroupBy(s => s.Difficulty))
                    {
                        Console.WriteLine($"\n--- SVÅRIGHETSGRAD: {diffGroup.Key} ---");
                        int rank = 1;
                        foreach (var score in diffGroup.OrderByDescending(s => s.ConsecutiveWins))
                        {
                            Console.WriteLine($"{rank++}. {score.PlayerName} - {score.ConsecutiveWins} vinster i rad.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nKunde inte hämta highscores (Databaseror):");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            Console.WriteLine("\nTryck valfri tangent för att återgå till menyn...");
            Console.ReadKey(true);
        }

        // --- NY METOD: Inställningar/Verktyg ---
        private async Task ShowSettingsMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- INSTÄLLNINGAR/VERKTYG ---");
                Console.WriteLine("1. Lägg till nytt ord till anpassad lista");
                Console.WriteLine("2. Hjälp / Hur man spelar");
                Console.WriteLine("3. Tillbaka till Huvudmenyn");
                Console.Write("\nVälj (1-3): ");

                var key = Console.ReadKey(intercept: true);
                Console.WriteLine(key.KeyChar);

                switch (key.KeyChar)
                {
                    case '1':
                        await AddCustomWordAsync();
                        break;
                    case '2':
                        ShowHelpScreen();
                        break;
                    case '3':
                        return; // Återgå till RunAsync-loopen
                }
            }
        }

        // --- NY METOD: Lägg till anpassat ord ---
        private async Task AddCustomWordAsync()
        {
            Console.Clear();
            Console.WriteLine("--- LÄGG TILL NYTT ORD ---");

            // 1. Få ordet
            string? word = string.Empty;
            while (string.IsNullOrWhiteSpace(word) || !word.All(char.IsLetter))
            {
                Console.Write("Ange ordet du vill lägga till (endast bokstäver, A-Ö): ");
                word = Console.ReadLine()?.Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(word) || !word.All(char.IsLetter))
                {
                    Console.WriteLine("Ogiltigt ord. Ange endast bokstäver.");
                }
            }

            // 2. Välj svårighetsgrad (återanvänd befintlig metod)
            WordDifficulty difficulty = SelectDifficulty("Anpassat Ord");

            // 3. Spara ordet
            try
            {
                var saver = new CustomWordProvider(difficulty);
                await saver.AddWordAsync(word, difficulty);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nOrdet '{word}' har lagts till i den anpassade listan ({difficulty})!");
                Console.ResetColor();
            }
            catch (InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nKunde inte lägga till ordet: {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nEtt fel inträffade vid fil-IO: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nTryck valfri tangent för att återgå till Inställningar...");
            Console.ReadKey(true);
        }

        // --- NY METOD: Hjälp/How to Play ---
        private void ShowHelpScreen()
        {
            Console.Clear();
            Console.WriteLine("--- HJÄLP / HUR MAN SPELAR ---");
            Console.WriteLine("Spelet går ut på att gissa det hemliga ordet, bokstav för bokstav.");
            Console.WriteLine("Du har 6 gissningar på dig innan gubben hängs (MAX 6 fel).");
            Console.WriteLine("\nLägen:");
            Console.WriteLine("  1. Enspelare (Highscore): Spelet fortsätter tills du förlorar.");
            Console.WriteLine("  2. Turnering (Tvåspelare): Ni har 3 liv var. Liv återställs vid vinst.");
            Console.WriteLine("\nOrdkällor:");
            Console.WriteLine("  API: Engelska ord med längd baserat på svårighetsgrad.");
            Console.WriteLine("  Lokal: Svenska ord från en inbyggd lista.");
            Console.WriteLine("  Anpassad: Ord som du själv har lagt till.");

            Console.WriteLine("\nTryck valfri tangent för att återgå till Inställningar...");
            Console.ReadKey(true);
        }

        // --- Huvudloop för 2-spelarturnering ---
        private async Task PlayTournamentAsync()
        {
            Console.Clear();
            Console.WriteLine("--- TVÅSPELARTURNERING ---");

            // 1. Hämta spelarnamn och ordkälla (som gäller för alla rundor)
            string p1Name = GetPlayerName("Ange namn för Spelare 1: ");
            string p2Name = GetPlayerName("Ange namn för Spelare 2: ");

            // ANVÄND NY SelectWordSource, ignorera svårighetsgraden
            var (provider, _) = SelectWordSource();

            if (provider == null) return;

            // Skapa turneringsmotorn
            var tournament = new Hangman.Core.TwoPlayerGame(p1Name, p2Name, provider);

            Console.Clear();
            Console.WriteLine($"Turneringen startar! {p1Name} mot {p2Name}. Ordkälla: {provider.DifficultyName}");
            Console.WriteLine($"Första gissare: {tournament.CurrentGuesser!.Name}. Liv: {Hangman.Core.TwoPlayerGame.MaxLives} vardera.");
            Console.WriteLine("Tryck valfri tangent för att starta den första rundan...");
            Console.ReadKey(true);

            // 3. Huvudloop för turneringen
            while (tournament.TournamentStatus == GameStatus.InProgress)
            {
                Hangman.Core.Player currentGuesser = tournament.CurrentGuesser!;

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
            Hangman.Core.Player winner = tournament.GetWinner()!;
            Hangman.Core.Player loser = (winner == tournament.Player1) ? tournament.Player2 : tournament.Player1;

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
        /// Låter användaren välja ordkälla (API, Lokal eller Anpassad).
        /// Returnerar både provider och vald svårighetsgrad (tuple).
        /// </summary>
        private (IAsyncWordProvider? Provider, WordDifficulty Difficulty) SelectWordSource()
        {
            Console.Clear();
            Console.WriteLine("--- VÄLJ ORDLISTA ---");
            Console.WriteLine("1. Engelska (API - olika längd)");
            Console.WriteLine("2. Svenska (Lokal lista)");
            Console.WriteLine("3. Anpassad Ordlista (Dina sparade ord)");
            Console.Write("\nVälj (1-3): ");

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                WordDifficulty difficulty;
                switch (key.KeyChar)
                {
                    case '1':
                        Console.WriteLine("Engelska (API)");
                        difficulty = SelectDifficulty("API");
                        return (new ApiWordProvider(difficulty), difficulty);

                    case '2':
                        Console.WriteLine("Svenska (Lokal)");
                        difficulty = SelectDifficulty("LOKAL");
                        return (new WordProvider(difficulty), difficulty);

                    case '3':
                        Console.WriteLine("Anpassad Ordlista");
                        difficulty = SelectDifficulty("ANPASSAD");
                        return (new CustomWordProvider(difficulty), difficulty);

                    default:
                        return (null, WordDifficulty.Medium);
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
                // Hela namnet behövs här om Hangman.Core.TwoPlayerGame inte har using
                Console.WriteLine($"Aktiv spelare: {playerGuessing} | Liv: {Hangman.Core.TwoPlayerGame.MaxLives}");
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