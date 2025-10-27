using Hangman.Core;
using Hangman.Core.Models;
using Hangman.Core.Providers.Api; // <-- NY: För att hämta ord
using System.Linq;
using System.Threading.Tasks;     // <-- NY: För async
using System.Windows;

namespace Hangman.WPF.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        // 1. Modellen (M)
        private readonly Game _game;

        // 2. Egenskaper som UI:t (V) kan binda till
        private string _maskedWord = "Laddar ord...";
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

        // 3. Konstruktor (Modifierad)
        public GameViewModel()
        {
            // Skapa en instans av din modell
            _game = new Game(6);

            // Prenumerera på events från din modell!
            _game.LetterGuessed += OnGameUpdated;
            _game.WrongLetterGuessed += OnGameUpdated;
            _game.GameEnded += OnGameEnded;

            // Vi startar inte spelet här längre, det gör LoadNewGameAsync
        }

        // 4. NY Metod för att ladda spel
        public async Task LoadNewGameAsync()
        {
            // Välj vilken ordhämtare du vill testa med:
            var wordProvider = new ApiWordProvider(WordDifficulty.Medium);
            // eller:
            // var wordProvider = new Hangman.Core.WordProvider(WordDifficulty.Easy);

            try
            {
                // Hämta ordet asynkront
                string word = await wordProvider.GetWordAsync();
                _game.StartNew(word);
            }
            catch (System.Exception)
            {
                // Hantera fel, t.ex. om API:et är nere
                // (Du kan logga ex.Message här)
                _game.StartNew("APIERROR");
            }
            finally
            {
                // Uppdatera alltid UI:t när laddningen är klar
                UpdateUiProperties();
            }
        }


        // 5. Metod som UI:t kan anropa
        public void MakeGuess(char letter)
        {
            _game.Guess(letter);
        }

        // 6. Event-handlers (Modellen pratar med oss)
        private void OnGameUpdated(object? sender, char e)
        {
            UpdateUiProperties();
        }

        private void OnGameEnded(object? sender, GameStatus status)
        {
            UpdateUiProperties();
            // Här kan du visa en MessageBox
            MessageBox.Show(
                status == GameStatus.Won ? "Grattis, du vann!" : $"Du förlorade! Ordet var: {_game.Secret}",
                "Spelet är slut");
        }

        // 7. Hjälpmetod för att uppdatera alla UI-fält
        private void UpdateUiProperties()
        {
            MaskedWord = _game.GetMaskedWord();
            UsedLetters = $"Gissade: {string.Join(", ", _game.UsedLetters.OrderBy(c => c))}";
        }
    }
}