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

        public GameStatus Status { get; private set; } = GameStatus.InProgress;
        public int Mistakes { get; private set; } = 0;
        public int AttemptsLeft => _maxMistakes - Mistakes;
        public IReadOnlyCollection<char> UsedLetters { get; private set; } = Array.Empty<char>();
        public string Secret => _secret;

        private readonly int _maxMistakes;

        public Game(int maxMistakes = 6)
        {
            if (maxMistakes <= 0) throw new ArgumentOutOfRangeException(nameof(maxMistakes));
            _maxMistakes = maxMistakes;
        }

        // Events finns redan här så att du kan skriva tester mot dem senare om du vill
        public event EventHandler<char>? LetterGuessed;
        public event EventHandler<char>? WrongLetterGuessed;
        public event EventHandler<GameStatus>? GameEnded;

        public void StartNew(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentException("Secret word cannot be empty", nameof(word));

            _secret = word.ToUpperInvariant();   // Räcker för första testerna ("Test" -> "TEST")
            Status = GameStatus.InProgress;      // Startläge
            Mistakes = 0;                        // Nollställ fel
            UsedLetters = Array.Empty<char>();   // Inga gissningar ännu
        }

        // IMPLEMENTERAS SEN när testerna skrivs
        public bool Guess(char letter) => throw new NotImplementedException();

        // IMPLEMENTERAS SEN när testerna skrivs
        public string GetMaskedWord() => throw new NotImplementedException();
    }
}


