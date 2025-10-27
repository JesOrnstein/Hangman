using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Models;

namespace Hangman.Core
{
    /// <summary>
    /// Representerar en spelare i 2-spelarläget.
    /// </summary>
    public class Player
    {
        public string Name { get; }
        public int Lives { get; set; }
        public int Wins { get; set; }

        public Player(string name, int initialLives)
        {
            Name = name;
            Lives = initialLives;
            Wins = 0;
        }
    }

    /// <summary>
    /// Hanterar turneringen mellan två spelare med liv, runda-logik och vinstvillkor.
    /// </summary>
    public class TwoPlayerGame
    {
        public const int MaxLives = 3;
        private readonly IAsyncWordProvider _wordProvider;
        private readonly Random _rng = new Random();

        public Player Player1 { get; }
        public Player Player2 { get; }
        public Player? CurrentGuesser { get; private set; }
        public Game? CurrentRound { get; private set; }

        /// <summary>
        /// Namnet på spelaren vars tur det är att gissa.
        /// </summary>
        public string CurrentPlayerName => CurrentGuesser?.Name ?? "Inget";

        /// <summary>
        /// Status på turneringen.
        /// </summary>
        public GameStatus TournamentStatus { get; private set; } = GameStatus.InProgress;

        /// <summary>
        /// Initierar en ny 2-spelarturnering.
        /// </summary>
        public TwoPlayerGame(string p1Name, string p2Name, IAsyncWordProvider wordProvider)
        {
            Player1 = new Player(p1Name, MaxLives);
            Player2 = new Player(p2Name, MaxLives);
            _wordProvider = wordProvider;

            // Bestäm slumpmässigt vem som börjar gissa
            CurrentGuesser = (_rng.Next(2) == 0) ? Player1 : Player2;
        }

        /// <summary>
        /// Startar en ny spelrunda. Ordet hämtas från den konfigurerade ordkällan.
        /// (UPPDATERAD MED NY VINST-LOGIK)
        /// </summary>
        /// <returns>Det hemliga ordet.</returns>
        public async Task<string> StartNewRoundAsync()
        {
            // --- NY LOGIK: Kontrollera turneringsstatus FÖRE RUNDAN STARTAR ---
            // "CurrentGuesser" är den som *ska* spela nu.
            // "opponent" är den som spelade *förra* rundan.
            Player opponent = (CurrentGuesser == Player1) ? Player2 : Player1;

            if (CurrentGuesser!.Lives <= 0)
            {
                // Spelaren vars tur det ÄR har 0 liv.
                if (opponent.Lives <= 0)
                {
                    // Motståndaren har OCKSÅ 0 liv (båda förlorade sin sista runda).
                    TournamentStatus = GameStatus.Draw;
                }
                else
                {
                    // CurrentGuesser har 0 liv, men motståndaren har > 0.
                    // CurrentGuesser förlorar, motståndaren vinner.
                    TournamentStatus = GameStatus.Lost; // CurrentGuesser förlorade
                }
            }
            // OBS: Vi kollar INTE "else if (opponent.Lives <= 0)"
            // Om motståndaren har 0 liv, men CurrentGuesser har liv,
            // MÅSTE CurrentGuesser fortfarande spela sin runda (för att ev. vinna och återställa liv).
            // Om de misslyckas, får de 0 liv, och nästa runda blir Draw.
            // --- SLUT PÅ NY LOGIK ---


            if (TournamentStatus != GameStatus.InProgress)
            {
                // Signalera till UI att spelet är slut och inget ord kan hämtas.
                throw new InvalidOperationException("Turneringen är avslutad.");
            }

            // Hämta ordet asynkront (Gammal logik)
            string secret = await _wordProvider.GetWordAsync();

            // Skapa en ny Game-instans med 6 maxfel
            CurrentRound = new Game(6);
            CurrentRound.StartNew(secret);

            return secret;
        }

        /// <summary>
        /// Hanterar resultatet av den nyss avslutade rundan och uppdaterar spelarnas liv och turordning.
        /// (FÖRENKLAD)
        /// </summary>
        /// <param name="roundResult">Resultatet av rundan (Won eller Lost).</param>
        public void HandleRoundEnd(GameStatus roundResult)
        {
            if (CurrentGuesser == null)
            {
                throw new InvalidOperationException("Kan inte hantera runda. Ingen aktiv gissare.");
            }

            // Spelaren som gissade
            Player guessingPlayer = CurrentGuesser;

            // 1. Hantera Liv och Wins
            if (roundResult == GameStatus.Won)
            {
                // Vinnaren får en vinst och återställer liv till max.
                guessingPlayer.Wins++;
                guessingPlayer.Lives = MaxLives;
            }
            else // Lost
            {
                // Förloraren (den som gissade) förlorar ett liv.
                guessingPlayer.Lives--;
            }

            // 2. Kontrollera Vinstvillkor (Turneringen Slut)
            // === DENNA LOGIK ÄR BORTTAGEN HÄRIFRÅN ===
            // (Den ligger nu i StartNewRoundAsync)


            // 3. Byt Gissare (alltid, oavsett status)
            CurrentGuesser = (CurrentGuesser == Player1) ? Player2 : Player1;
        }

        /// <summary>
        /// Returnerar den spelare som vann turneringen, eller null om den fortfarande pågår eller blev oavgjord.
        /// (UPPDATERAD LOGIK)
        /// </summary>
        public Player? GetWinner()
        {
            // Denna anropas bara när TournamentStatus INTE är InProgress eller Draw.
            // Vinnaren är den som har liv kvar.

            if (Player1.Lives > 0 && Player2.Lives <= 0)
            {
                return Player1;
            }

            if (Player2.Lives > 0 && Player1.Lives <= 0)
            {
                return Player2;
            }

            // Om TournamentStatus är Draw (båda har 0 liv) -> return null.
            // Om TournamentStatus är InProgress -> return null.

            return null;
        }
    }
}