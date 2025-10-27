using Hangman.Core; // <--- Tillgång till din spellogik!
using Hangman.Core.Models;
using System.Linq;

namespace Hangman.WPF.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        // 1. Modellen (M)
        private readonly Game _game;

        // 2. Egenskaper som UI:t (V) kan binda till
        private string _maskedWord = "";
        public string MaskedWord
        {
            get { return _maskedWord; }
            set
            {
                _maskedWord = value;
                OnPropertyChanged(); // Meddela UI:t!
            }
        }

        private string _usedLetters = "";
        public string UsedLetters
        {
            get { return _usedLetters; }
            set
            {
                _usedLetters = value;
                OnPropertyChanged(); // Meddela UI:t!
            }
        }

        // ... (lägg till fler för AttemptsLeft etc.)

        // 3. Konstruktor
        public GameViewModel()
        {
            // Skapa en instans av din modell
            _game = new Game(6);

            // Prenumerera på events från din modell!
            _game.LetterGuessed += OnGameUpdated;
            _game.WrongLetterGuessed += OnGameUpdated;
            _game.GameEnded += OnGameEnded;

            // Starta ett spel (hårdkodat ord för test)
            _game.StartNew("WPFTEST");
            UpdateUiProperties();
        }

        // 4. Metod som UI:t kan anropa
        public void MakeGuess(char letter)
        {
            _game.Guess(letter);
            // Vi behöver inte kalla OnPropertyChanged här,
            // för _game.Guess() kommer trigga våra event-handlers!
        }

        // 5. Event-handlers (Modellen pratar med oss)
        private void OnGameUpdated(object? sender, char e)
        {
            UpdateUiProperties();
        }

        private void OnGameEnded(object? sender, GameStatus status)
        {
            UpdateUiProperties();
            // Visa en popup?
        }

        // 6. Hjälpmetod för att uppdatera alla UI-fält
        private void UpdateUiProperties()
        {
            MaskedWord = _game.GetMaskedWord();
            UsedLetters = string.Join(", ", _game.UsedLetters.OrderBy(c => c));
        }
    }
}