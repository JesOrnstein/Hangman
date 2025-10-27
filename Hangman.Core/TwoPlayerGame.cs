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
        /// <param name="p1Name">Namnet på spelare 1.</param>
        /// <param name="p2Name">Namnet på spelare 2.</param>
        /// <param name="wordProvider">Den ordkälla som ska användas för varje runda (t.ex. en lokal lista).</param>
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
        /// </summary>
        /// <returns>Det hemliga ordet.</returns>
        public async Task<string> StartNewRoundAsync()
        {
            if (TournamentStatus != GameStatus.InProgress)
            {
                throw new InvalidOperationException("Turneringen är avslutad.");
            }

            // Hämta ordet asynkront
            string secret = await _wordProvider.GetWordAsync();

            // Skapa en ny Game-instans med 6 maxfel
            CurrentRound = new Game(6);
            CurrentRound.StartNew(secret);

            return secret;
        }

        /// <summary>
        /// Hanterar resultatet av den nyss avslutade rundan och uppdaterar spelarnas liv och turordning.
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
            if (guessingPlayer.Lives <= 0)
            {
                TournamentStatus = GameStatus.Lost; // Turneringen avslutas
                // Den andra spelaren (som inte gissade) är vinnaren
            }

            // 3. Byt Gissare (om turneringen fortsätter)
            if (TournamentStatus == GameStatus.InProgress)
            {
                // Turordningen byts.
                CurrentGuesser = (CurrentGuesser == Player1) ? Player2 : Player1;
            }
        }

        /// <summary>
        /// Returnerar den spelare som vann turneringen, eller null om den fortfarande pågår.
        /// </summary>
        public Player? GetWinner()
        {
            if (TournamentStatus != GameStatus.Lost) return null;

            // Vinnaren är den spelare som INTE fick 0 liv.
            if (Player1.Lives <= 0) return Player2;
            if (Player2.Lives <= 0) return Player1;

            return null; // Skulle inte hända om TournamentStatus är Lost
        }
    }
}