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

            CurrentGuesser = (_rng.Next(2) == 0) ? Player1 : Player2;
        }

        /// <summary>
        /// Startar en ny spelrunda. Ordet hämtas från den konfigurerade ordkällan.
        /// Returnerar det hemliga ordet, eller null om turneringen är avslutad.
        /// </summary>
        /// <returns>Det hemliga ordet.</returns>
        public async Task<string?> StartNewRoundAsync()
        {
            Player opponent = (CurrentGuesser == Player1) ? Player2 : Player1;

            if (opponent.Lives <= 0)
            {
                // Motståndaren har 0 liv. Använd Wins som tie-breaker.
                if (Player1.Wins != Player2.Wins)
                {
                    TournamentStatus = GameStatus.Lost;
                }
                else
                {
                    TournamentStatus = GameStatus.Draw;
                }
            }

            if (TournamentStatus != GameStatus.InProgress)
            {
                return null; // NYTT: Returnerar null istället för att kasta exception
            }

            string secret = await _wordProvider.GetWordAsync();

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

            Player guessingPlayer = CurrentGuesser;

            // 1. Hantera Liv och Wins
            if (roundResult == GameStatus.Won)
            {
                guessingPlayer.Wins++;
                guessingPlayer.Lives = MaxLives;
            }
            else // Lost
            {
                guessingPlayer.Lives--;
            }

            // 2. Byt Gissare
            CurrentGuesser = (CurrentGuesser == Player1) ? Player2 : Player1;
        }

        /// <summary>
        /// Returnerar den spelare som vann turneringen, eller null om den fortfarande pågår eller blev oavgjord.
        /// </summary>
        public Player? GetWinner()
        {
            if (Player1.Lives > 0 && Player2.Lives <= 0)
            {
                return Player1;
            }

            if (Player2.Lives > 0 && Player1.Lives <= 0)
            {
                return Player2;
            }

            // Tie-breaker när båda har 0 liv (TournamentStatus = Draw)
            if (Player1.Lives <= 0 && Player2.Lives <= 0)
            {
                if (Player1.Wins > Player2.Wins)
                {
                    return Player1;
                }
                if (Player2.Wins > Player1.Wins)
                {
                    return Player2;
                }
            }

            return null;
        }
    }
}