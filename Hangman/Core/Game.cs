using System;
using System.Collections.Generic;

/*
  ───────────────────────────────────────────────────────────────
   CLASS: Game
   Ansvarar för all kärnlogik i spelet Hänga Gubbe.

   Huvuduppgifter:
   - Starta nytt spel (StartNew)
   - Hantera gissningar (Guess)
   - Hålla koll på fel, status och använda bokstäver
   - Ge maskerat ord (GetMaskedWord)
   - Skicka ut events till UI (LetterGuessed, WrongLetterGuessed, GameEnded)
  ───────────────────────────────────────────────────────────────
*/

namespace Hangman.Core
{
    public sealed class Game
    {
        // Hemliga ordet som spelaren försöker lista ut
        private string _secret = string.Empty;

        // Max antal tillåtna fel innan förlust
        private readonly int _maxMistakes;

        // Bokstäver som spelaren redan gissat
        private readonly HashSet<char> _used = new();

        public Game(int maxMistakes = 6)
        {
            if (maxMistakes <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxMistakes));

            _maxMistakes = maxMistakes;
        }

        // Spelets nuvarande status: pågående, vunnet eller förlorat
        public GameStatus Status { get; private set; } = GameStatus.InProgress;

        // Antal fel som gjorts hittills
        public int Mistakes { get; private set; } = 0;

        // Hur många försök som återstår
        public int AttemptsLeft => _maxMistakes - Mistakes;

        // Publik read-only vy av använda bokstäver
        public IReadOnlyCollection<char> UsedLetters => _used;

        // Visar vilket ord som används i aktuell runda
        public string Secret => _secret;

        // ───── EVENTS ──────────────────────────────────────────────
        // UI eller testkod kan prenumerera på dessa:
        public event EventHandler<char>? LetterGuessed;      // När en bokstav gissas rätt
        public event EventHandler<char>? WrongLetterGuessed; // När en bokstav gissas fel
        public event EventHandler<GameStatus>? GameEnded;    // När spelet tar slut (vinst/förlust)
        // ──────────────────────────────────────────────────────────

        /*
          Startar en ny spelrunda.
          Nollställer state, rensar använda bokstäver och ställer in nytt hemligt ord.
        */
        public void StartNew(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentException("Secret word cannot be empty", nameof(word));

            _secret = word.ToUpperInvariant();   // Normalisera till versaler
            Status = GameStatus.InProgress;      // Startläge
            Mistakes = 0;                        // Nollställ felräknaren
            _used.Clear();                       // Rensa tidigare gissningar
        }

        /*
          Hanterar en bokstavsgissning.
          Returnerar true om bokstaven finns i ordet, annars false.

          - Ignorerar dubblettgissningar
          - Uppdaterar Mistakes vid fel gissning
          - Avslutar spelet vid vinst/förlust
        */
        public bool Guess(char letter)
        {
            if (Status != GameStatus.InProgress) return false; // Ignorera om spelet redan är slut

            var c = char.ToUpperInvariant(letter);

            // Om bokstaven redan gissats: returnera bara om den finns i ordet
            if (_used.Contains(c))
                return _secret.Contains(c);

            _used.Add(c);

            // ───── Rätt gissning ───────────────────────────────
            if (_secret.Contains(c))
            {
                LetterGuessed?.Invoke(this, c);

                // Kontrollera om alla bokstäver i ordet är gissade ⇒ vinst
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

            // Förlust om gränsen nås
            if (Mistakes >= _maxMistakes)
            {
                Status = GameStatus.Lost;
                GameEnded?.Invoke(this, Status);
            }

            return false;
        }

        /*
          Returnerar ordet i maskerat format.
          - Rätt gissade bokstäver visas
          - Ej gissade bokstäver ersätts med '_'
        */
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

        /*
          Hjälpmetod: kontrollerar om alla bokstäver i det hemliga ordet är avslöjade.
          Används för att avgöra vinstvillkoret.
        */
        private bool AllRevealed()
        {
            var needed = new HashSet<char>(_secret);
            needed.RemoveWhere(ch => !char.IsLetter(ch)); // ignorera t.ex. mellanslag eller bindestreck
            return needed.IsSubsetOf(_used);
        }
    }
}