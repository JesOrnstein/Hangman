using Hangman.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hangman.Core
{
    /// <summary>
    /// Ansvarar för all kärnlogik i spelet Hänga Gubbe.
    /// </summary>
    public sealed class Game
    {
        private string _secret = string.Empty;
        private readonly int _maxMistakes;
        private readonly HashSet<char> _used = new();

        public Game(int maxMistakes = 6)
        {
            if (maxMistakes <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxMistakes));

            _maxMistakes = maxMistakes;
        }

        public GameStatus Status { get; private set; } = GameStatus.InProgress;
        public int Mistakes { get; private set; } = 0;
        public int AttemptsLeft => _maxMistakes - Mistakes;
        public IReadOnlyCollection<char> UsedLetters => _used;
        public string Secret => _secret;

        // ───── EVENTS ──────────────────────────────────────────────
        public event EventHandler<char>? LetterGuessed;
        public event EventHandler<char>? WrongLetterGuessed;
        public event EventHandler<GameStatus>? GameEnded;
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Startar en ny spelrunda.
        /// Nollställer state, rensar använda bokstäver och ställer in nytt hemligt ord.
        /// </summary>
        public void StartNew(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentException("Secret word cannot be empty", nameof(word));

            _secret = word.ToUpperInvariant();
            Status = GameStatus.InProgress;
            Mistakes = 0;
            _used.Clear();
        }

        /// <summary>
        /// Hanterar en bokstavsgissning.
        /// Returnerar true om bokstaven finns i ordet, annars false.
        /// </summary>
        public bool Guess(char letter)
        {
            if (Status != GameStatus.InProgress) return false;

            var c = char.ToUpperInvariant(letter);

            // Ignorera dubblettgissningar
            if (_used.Contains(c))
                return _secret.Contains(c);

            _used.Add(c);

            // ───── Rätt gissning ───────────────────────────────
            if (_secret.Contains(c))
            {
                LetterGuessed?.Invoke(this, c);

                if (AllRevealed())
                {
                    Status = GameStatus.Won;
                    GameEnded?.Invoke(this, Status);
                }

                return true;
            }

            // ───── Fel gissning ────────────────────────────────
            Mistakes++;
            WrongLetterGuessed?.Invoke(this, c);

            if (Mistakes >= _maxMistakes)
            {
                Status = GameStatus.Lost;
                GameEnded?.Invoke(this, Status);
            }

            return false;
        }

        /// <summary>
        /// Returnerar ordet i maskerat format.
        /// </summary>
        public string GetMaskedWord()
        {
            if (string.IsNullOrEmpty(_secret))
                return string.Empty;

            var chars = new char[_secret.Length];
            for (int i = 0; i < _secret.Length; i++)
            {
                var ch = _secret[i];
                chars[i] = _used.Contains(ch) ? ch : '_';
            }
            return new string(chars);
        }

        private bool AllRevealed()
        {
            var needed = new HashSet<char>(_secret);
            // Ignorera icke-bokstäver
            needed.RemoveWhere(ch => !char.IsLetter(ch));
            return needed.IsSubsetOf(_used);
        }

        /// <summary>
        /// Tvingar spelet till förlust-läge.
        /// Används om spelaren avbryter en runda eller om tiden går ut.
        /// </summary>
        public void ForceLose()
        {
            if (Status == GameStatus.InProgress)
            {
                Status = GameStatus.Lost;
                GameEnded?.Invoke(this, Status);
            }
        }
    }
}