using Hangman.Core;
using Hangman.Core.Providers.Api;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Models;
using Hangman.Core.Providers.Local;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text; // <-- NYTT: Behövs för GetInputString

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
            // --- NYTT VÄLKOMST-BLOCK ---
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;

            // 1. AVANCERAD "HANGMAN"-titel
            Console.WriteLine(@"
     _   _    _    _   _  ____ __  __    _    _   _
    | | | |  / \  | \ | |/ ___|  \/  |  / \  | \ | |
    | |_| | / _ \ |  \| | |  _| |\/| | / _ \ |  \| |
    |  _  |/ ___ \| |\  | |_| | |  | |/ ___ \| |\  |
    |_| |_/_/   \_\_| \_|\____|_|  |_/_/   \_\_| \_|
");
            // 2. VÄLKOMST-TEXT
            Console.ResetColor();
            Console.WriteLine("\n\n          Välkommen till HÄNGA GUBBE!");
            Console.WriteLine("     Tryck valfri tangent för att starta...");

            // 3. VÄNTA PÅ INPUT
            Console.ReadKey(true);
            // --- SLUT PÅ VÄLKOMST-BLOCK ---


            // Huvudmenyn-loopen startar här (oförändrad)
            while (true)
            {
                var choice = ShowMainMenu(); // Visar den nya menyn
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
                    // NY ORDNING FÖR ATT MATCHA BILDEN
                    case '4':
                        await AddCustomWordAsync(); // Flyttad från undermenyn
                        break;
                    case '5':
                        ShowHelpScreen(); // Flyttad från undermenyn
                        break;
                    case '6': // Nytt val för att avsluta
                        Console.Clear();
                        Console.WriteLine("Tack för att du spelade!");
                        return;
                }
            }
        }

        // UPPDATERAD ShowMainMenu
        private char ShowMainMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;

            // NYTT: Använd "HANGMAN"-titeln här istället för galgen
            Console.WriteLine(@"
     _   _    _    _   _  ____ __  __    _    _   _
    | | | |  / \  | \ | |/ ___|  \/  |  / \  | \ | |
    | |_| | / _ \ |  \| | |  _| |\/| | / _ \ |  \| |
    |  _  |/ ___ \| |\  | |_| | |  | |/ ___ \| |\  |
    |_| |_/_/   \_\_| \_|\____|_|  |_/_/   \_\_| \_|
            ");

            // 2. Gubben/Galgen (som i bilden)
            // Vi använder steg 6 (med huvudet) från din befintliga array.
            Console.WriteLine(@"
        +-------+
        |       |
        |       e
        |      /|\
        |      / \
        |
    ------------
   /            \
  /              \
 =================
");
            Console.ResetColor();
            Console.WriteLine();

            // De nya menyvalen
            Console.WriteLine("--- HUVUDMENY ---");
            Console.WriteLine("1. Spela (Enspelare - Highscore)");
            Console.WriteLine("2. Spela (Tvåspelare, turnering)");
            Console.WriteLine("3. Visa Highscores");
            Console.WriteLine("4. Lägg till nytt ord");
            Console.WriteLine("5. Hjälp / Hur man spelar");
            Console.WriteLine("6. Avsluta");

            Console.Write("\nDitt val (1-6): ");

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                // Ändrad för att acceptera 1-6
                if (key.KeyChar >= '1' && key.KeyChar <= '6')
                {
                    Console.WriteLine(key.KeyChar);
                    return key.KeyChar;
                }
            }
        }

        // --- Huvudloop för enspelare med Highscore-logik ---
        private async Task PlaySinglePlayerHighscoreAsync()
        {
            // 1. Välj ordkälla och svårighetsgrad
            var (provider, currentDifficulty) = SelectWordSource();
            if (provider == null) return; // Backa till huvudmenyn

            // Hämta spelarnamn
            string? playerName = GetPlayerName("Ange ditt namn för highscore: ");
            if (playerName == null) return; // Backa till huvudmenyn

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
                    Console.Write("\nVill du fortsätta spela (J/N)? (Escape för att avsluta) ");
                    var key = Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Escape || char.ToUpperInvariant(key.KeyChar) != 'J')
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
                    if (consecutiveWins > 0) // Endast visa om de vann minst en
                    {
                        Console.WriteLine("\nTryck valfri tangent för att spara highscore...");
                        Console.ReadKey(true);
                    }
                }


            } while (roundResult == GameStatus.Won);

            // 2. Spara Highscore
            // FIX FÖR CS8629
            if (consecutiveWins > 0 && currentDifficulty.HasValue)
            {
                var newScore = new HighscoreEntry(playerName, consecutiveWins, currentDifficulty.Value)
                {
                    PlayerName = playerName,
                    ConsecutiveWins = consecutiveWins,
                    Difficulty = currentDifficulty.Value
                };
                await _statisticsService.SaveHighscoreAsync(newScore);

                Console.WriteLine($"\nHighscore ({consecutiveWins} vinster i rad) sparat för {currentDifficulty.Value}!");
            }

            Console.WriteLine("\nÅtergår till huvudmenyn...");
            Console.ReadKey(true);
        }

        // --- Visa Highscores (Top 10) ---
        private async Task ShowHighscoresAsync()
        {
            Console.Clear();
            Console.WriteLine("--- HIGHSCORES ---");
            Console.WriteLine("Hämtar globala topp 10-resultat...");

            try
            {
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

        // --- Lägg till anpassat ord ---
        private async Task AddCustomWordAsync()
        {
            Console.Clear();
            Console.WriteLine("--- LÄGG TILL NYTT ORD ---");
            Console.WriteLine("(Tryck Escape när som helst för att avbryta)");

            // 1. Få ordet
            string? word = string.Empty;
            while (string.IsNullOrWhiteSpace(word) || !word.All(char.IsLetter))
            {
                word = GetInputString("Ange ordet (A-Ö): ");

                if (word == null) return; // Användaren tryckte Escape

                if (string.IsNullOrWhiteSpace(word) || !word.All(char.IsLetter))
                {
                    Console.WriteLine("\nOgiltigt ord. Ange endast bokstäver.");
                }
            }

            word = word.ToUpperInvariant(); // Se till att det är versaler

            // 2. Välj svårighetsgrad
            WordDifficulty? difficulty = SelectDifficulty("Anpassat Ord");
            if (difficulty == null) return; // Användaren tryckte Escape

            // 3. Spara ordet
            try
            {
                var saver = new CustomWordProvider(difficulty.Value);
                await saver.AddWordAsync(word, difficulty.Value);

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
                Console.WriteLine($"\nEtt databasfel inträffade: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nTryck valfri tangent för att återgå till huvudmenyn...");
            Console.ReadKey(true);
        }

        // --- Hjälp/How to Play ---
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

            Console.WriteLine("\nTryck valfri tangent för att återgå till huvudmenyn...");
            Console.ReadKey(true);
        }

        // --- Huvudloop för 2-spelarturnering (MODIFIERAD) ---
        private async Task PlayTournamentAsync()
        {
            Console.Clear();
            Console.WriteLine("--- TVÅSPELARTURNERING ---");
            Console.WriteLine("(Tryck Escape när som helst för att avbryta)");

            // 1. Hämta spelarnamn och ordkälla
            string? p1Name = GetPlayerName("Ange namn för Spelare 1: ");
            if (p1Name == null) return; // Backa

            string? p2Name = GetPlayerName("Ange namn för Spelare 2: ");
            if (p2Name == null) return; // Backa

            var (provider, _) = SelectWordSource();
            if (provider == null) return; // Backa

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
                    // UPPDATERAD: StartNewRoundAsync kastar ett undantag
                    // om spelet är slut (Won, Lost eller Draw)
                    secret = await tournament.StartNewRoundAsync();
                }
                // NY CATCH
                catch (InvalidOperationException)
                {
                    // Turneringen är avslutad (Won, Lost, Draw).
                    // Bryt loopen för att visa slutresultatet.
                    break;
                }
                catch (Exception ex) // Hantera "riktiga" fel (t.ex. nätverksfel)
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

                // Om rundan avbröts (ForceLose), avbryt hela turneringen
                if (_game?.Status == GameStatus.Lost && _feedbackMessage == "Rundan avbröts av spelaren.")
                {
                    // Se till att liven uppdateras innan vi bryter
                    tournament.HandleRoundEnd(GameStatus.Lost);
                    Console.WriteLine("Turneringen avbröts.");
                    break;
                }

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

            // 4. Visa Vinnaren (UPPDATERAD MED DRAW-LOGIK)
            if (_feedbackMessage != "Rundan avbröts av spelaren.")
            {
                // NY KONTROLL FÖR OAVGJORT
                if (tournament.TournamentStatus == GameStatus.Draw)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n--- TURNERING AVSLUTAD ---");
                    Console.WriteLine("OAVGJORT! Båda spelarna förlorade alla sina liv.");
                    Console.ResetColor();
                }
                // GAMMAL LOGIK I EN 'ELSE'
                else
                {
                    Hangman.Core.Player? winner = tournament.GetWinner(); // Nullbar

                    if (winner != null)
                    {
                        Hangman.Core.Player loser = (winner == tournament.Player1) ? tournament.Player2 : tournament.Player1;

                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n--- TURNERING AVSLUTAD ---");
                        Console.WriteLine($"GRATTIS, {winner.Name} VANN TURNERINGEN!");
                        Console.ResetColor();
                        Console.WriteLine($"{loser.Name} förlorade alla sina liv. Vunna rundor:");
                    }
                    else
                    {
                        // Fallback om något gick fel (borde inte hända om Draw hanteras)
                        Console.WriteLine("\nTurneringen avslutad (Oväntat slut).");
                    }
                }

                // Visa alltid slutpoängen (Flyttad ut ur 'else')
                Console.WriteLine($"- {p1Name}: {tournament.Player1.Wins} rundor");
                Console.WriteLine($"- {p2Name}: {tournament.Player2.Wins} rundor");
            }

            Console.WriteLine("\nTryck valfri tangent för att återgå till menyn...");
            Console.ReadKey(true);
        }


        /// <summary>
        /// Låter användaren välja ordkälla (API, Lokal eller Anpassad).
        /// Returnerar både provider och vald svårighetsgrad (tuple).
        /// </summary>
        private (IAsyncWordProvider? Provider, WordDifficulty? Difficulty) SelectWordSource()
        {
            Console.Clear();
            Console.WriteLine("--- VÄLJ ORDLISTA ---");
            Console.WriteLine("1. Engelska (API - olika längd)");
            Console.WriteLine("2. Svenska (Lokal lista)");
            Console.WriteLine("3. Anpassad Ordlista (Dina sparade ord)");
            Console.WriteLine("(Tryck Escape för att backa)");
            Console.Write("\nVälj (1-3): ");

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                WordDifficulty? difficulty; // Nullbar

                switch (key.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        Console.WriteLine("Engelska (API)");
                        difficulty = SelectDifficulty("API");
                        if (difficulty == null) return (null, null); // Backa vidare
                        return (new ApiWordProvider(difficulty.Value), difficulty.Value);

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        Console.WriteLine("Svenska (Lokal)");
                        difficulty = SelectDifficulty("LOKAL");
                        if (difficulty == null) return (null, null); // Backa vidare
                        return (new WordProvider(difficulty.Value), difficulty.Value);

                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        Console.WriteLine("Anpassad Ordlista");
                        difficulty = SelectDifficulty("ANPASSAD");
                        if (difficulty == null) return (null, null); // Backa vidare
                        return (new CustomWordProvider(difficulty.Value), difficulty.Value);

                    case ConsoleKey.Escape:
                        Console.WriteLine("Avbryter...");
                        return (null, null);
                }
            }
        }

        /// <summary>
        /// Låter användaren välja svårighetsgrad.
        /// </summary>
        private WordDifficulty? SelectDifficulty(string source)
        {
            Console.Clear();
            Console.WriteLine($"--- VÄLJ SVÅRIGHETSGRAD ({source}) ---");
            Console.WriteLine("1. Lätt (3-4 bokstäver)");
            Console.WriteLine("2. Medium (5-7 bokstäver)");
            Console.WriteLine("3. Svår (8-11 bokstäver)");
            Console.WriteLine("(Tryck Escape för att backa)");
            Console.Write("\nVälj (1-3): ");

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        Console.WriteLine("Lätt");
                        return WordDifficulty.Easy;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        Console.WriteLine("Medium");
                        return WordDifficulty.Medium;

                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        Console.WriteLine("Svår");
                        return WordDifficulty.Hard;

                    case ConsoleKey.Escape:
                        Console.WriteLine("Avbryter...");
                        return null;
                }
            }
        }

        /// <summary>
        /// Kör en enda runda med ett givet hemligt ord.
        /// </summary>
        private GameStatus PlayRound(string playerGuessing, string secret, int currentLives)
        {
            int maxMistakes = 6;

            _game = new Game(maxMistakes);
            _game.StartNew(secret);
            _feedbackMessage = string.Empty; // Nollställ feedback för rundan

            Console.Clear();
            Console.WriteLine($"--- NY RUNDA STARTAD ---");

            while (_game.Status == GameStatus.InProgress)
            {
                DrawGameScreen(playerGuessing, currentLives);

                char guess = GetGuess();
                if (guess == '\0') // '\0' är vår signal för "Avbryt"
                {
                    _game.ForceLose(); // Använder den nya metoden i Game.cs
                    _feedbackMessage = "Rundan avbröts av spelaren.";
                    continue; // Gå till nästa loop-iteration (där Status nu är Lost)
                }

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

            ShowEndScreen(); // Visar slutskärmen (vinst/förlust/avbruten)

            if (_game.Status == GameStatus.Lost)
            {
                if (_feedbackMessage != "Rundan avbröts av spelaren.")
                {
                    Console.WriteLine($"\n{playerGuessing} förlorade rundan.");
                }
            }
            else
            {
                Console.WriteLine($"\n{playerGuessing} vann rundan!");
            }

            return _game.Status;
        }


        private void DrawGameScreen(string playerGuessing, int currentLives)
        {
            Console.Clear();
            Console.WriteLine("--- HÄNGA GUBBE ---");

            Console.ForegroundColor = ConsoleColor.Green;
            if (currentLives > 0)
            {
                Console.WriteLine($"Aktiv spelare: {playerGuessing} | Liv: {currentLives}");
            }
            else
            {
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
                    // Gult för "Fel" och "Avbröts"
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                Console.WriteLine(_feedbackMessage);
                Console.ResetColor();

                // Nollställ bara om det INTE är ett slutmeddelande
                if (_feedbackMessage != "Rundan avbröts av spelaren.")
                {
                    _feedbackMessage = string.Empty;
                }
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
                Console.Write("Gissa på en bokstav (eller Escape för att ge upp): ");

                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Avbryter...");
                    return '\0'; // Null-tecken som signal
                }

                char letter = key.KeyChar;

                if (!char.IsLetter(letter))
                {
                    Console.WriteLine($"\nOgiltig gissning '{letter}'. Endast bokstäver (A-Ö).");
                    continue;
                }

                char upperGuess = char.ToUpperInvariant(letter);

                if (_game!.UsedLetters.Contains(upperGuess))
                {
                    Console.WriteLine($"\nDu har redan gissat på '{upperGuess}'. Försök igen.");
                    continue;
                }

                Console.WriteLine(upperGuess); // Eka den giltiga gissningen
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
                if (_feedbackMessage == "Rundan avbröts av spelaren.")
                {
                    Console.WriteLine("\nDu avbröt rundan.");
                }
                else
                {
                    Console.WriteLine("\nDU FÖRLORADE...");
                }
            }

            Console.ResetColor();
            Console.WriteLine($"Det rätta ordet var: {_game!.Secret}");
        }

        // Hjälpmetod för att hämta spelarnamn
        private string? GetPlayerName(string prompt)
        {
            string? name;
            while (true)
            {
                name = GetInputString(prompt);

                if (name == null) return null; // Användaren tryckte Escape

                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name.Trim();
                }

                Console.WriteLine("\nNamnet kan inte vara tomt. Försök igen (eller Escape för att backa).");
            }
        }

        // NY HJÄLPMETOD: Ersätter Console.ReadLine() för att fånga Escape
        /// <summary>
        /// Läser en textrad från konsolen tangent för tangent.
        /// Returnerar null om användaren trycker Escape.
        /// </summary>
        private string? GetInputString(string prompt)
        {
            Console.Write(prompt);
            var sb = new StringBuilder();

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\nAvbryter...");
                    return null; // Signal för att backa
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return sb.ToString();
                }

                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Length--;
                    // Radera tecknet från konsolen
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    Console.Write(key.KeyChar);
                }
            }
        }
    }
}