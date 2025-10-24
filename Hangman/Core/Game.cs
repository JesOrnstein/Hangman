using System;
using System.Collections.Generic;

/*
  Innehåller all spel-logik:
  - Startar nytt spel med StartNew()
  - Hanterar gissningar via Guess()
  - Håller reda på rätt/fel gissningar och status (vinst/förlust)
  - Returnerar det maskerade ordet (_ _ _ _)
  - Använder events (LetterGuessed, WrongLetterGuessed, GameEnded)
    för att meddela användargränssnittet när något händer.
*/

namespace Hangman.Core
{
    public sealed class Game
    {
        private string _secret = string.Empty;
        private readonly int _maxMistakes;

        // NYTT: intern samling för använda bokstäver
        private readonly HashSet<char> _used = new();

        public Game(int maxMistakes = 6)
        {
            if (maxMistakes <= 0) throw new ArgumentOutOfRangeException(nameof(maxMistakes));
            _maxMistakes = maxMistakes;
        }

        public GameStatus Status { get; private set; } = GameStatus.InProgress;
        public int Mistakes { get; private set; } = 0;
        public int AttemptsLeft => _maxMistakes - Mistakes;

        // ÄNDRAT: expose den interna hashen som read-only collection
        public IReadOnlyCollection<char> UsedLetters => _used;

        public string Secret => _secret;

        public event EventHandler<char>? LetterGuessed;
        public event EventHandler<char>? WrongLetterGuessed;
        public event EventHandler<GameStatus>? GameEnded;

        public void StartNew(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentException("Secret word cannot be empty", nameof(word));

            _secret = word.ToUpperInvariant();   // "Test" -> "TEST"
            Status = GameStatus.InProgress;      // Startläge
            Mistakes = 0;                        // Nollställ fel
            _used.Clear();                       // NYTT: nollställ använda bokstäver
        }

        // MINSTA MÖJLIGA för att klara testet:
        public bool Guess(char letter)
        {
            if (Status != GameStatus.InProgress) return false;

            var c = char.ToUpperInvariant(letter);

            // om vi redan gissat den här bokstaven – räkna det inte som fel
            if (_used.Contains(c)) return _secret.Contains(c);

            _used.Add(c);

            if (_secret.Contains(c))
            {
                // event (frivilligt för testet, men bra att ha)
                LetterGuessed?.Invoke(this, c);
                return true;
            }

            Mistakes++;
            WrongLetterGuessed?.Invoke(this, c);

            if (Mistakes >= _maxMistakes)
            {
                Status = GameStatus.Lost;
                GameEnded?.Invoke(this, Status);
            }

            return false;
        }

        // Lämnas till kommande test
        public string GetMaskedWord() => throw new NotImplementedException();
    }
}



