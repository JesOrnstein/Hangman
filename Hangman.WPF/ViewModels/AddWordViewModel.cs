using Hangman.Core.Exceptions;
using Hangman.Core.Models;
using Hangman.Core.Providers.Local;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Hangman.Core.Localizations; // <-- NY USING

namespace Hangman.WPF.ViewModels
{
    public class AddWordViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly LocalizationProvider _strings; // <-- NYTT FÄLT

        // NY PROPERTY: Exponera strängarna för XAML
        public LocalizationProvider Strings { get; }

        private string _newWord = "";
        public string NewWord { get => _newWord; set { _newWord = value; OnPropertyChanged(); } }

        public IEnumerable<WordLanguage> Languages => System.Enum.GetValues<WordLanguage>();

        private WordLanguage _selectedLanguage = WordLanguage.Swedish;
        public WordLanguage SelectedLanguage { get => _selectedLanguage; set { _selectedLanguage = value; OnPropertyChanged(); } }

        private string _feedbackMessage = "";
        public string FeedbackMessage { get => _feedbackMessage; set { _feedbackMessage = value; OnPropertyChanged(); } }

        public ICommand AddWordCommand { get; }
        public ICommand BackToMenuCommand { get; }

        // UPPDATERAD KONSTRUKTOR
        public AddWordViewModel(MainViewModel mainViewModel, LocalizationProvider strings)
        {
            _mainViewModel = mainViewModel;
            _strings = strings; // Spara för C#-logik
            Strings = strings;  // Exponera för XAML

            AddWordCommand = new RelayCommand(async _ => await AddWord(), _ => CanAddWord());
            BackToMenuCommand = new RelayCommand(_ => _mainViewModel.NavigateToMenu());
        }

        private bool CanAddWord()
        {
            return !string.IsNullOrWhiteSpace(NewWord) && NewWord.All(char.IsLetter);
        }

        // UPPDATERAD LOGIK: Använder _strings för feedback
        private async Task AddWord()
        {
            string word = NewWord.ToUpperInvariant();
            var difficulty = GetDifficultyByLength(word);

            try
            {
                var provider = new CustomWordProvider(difficulty, SelectedLanguage);
                await provider.AddWordAsync(word, difficulty, SelectedLanguage);

                // ÄNDRAD
                FeedbackMessage = _strings.AddWordSuccess(word, difficulty, SelectedLanguage);
                NewWord = ""; // Rensa fältet
            }
            catch (WordAlreadyExistsException ex)
            {
                // ÄNDRAD
                FeedbackMessage = _strings.ErrorWordAlreadyExists(ex.Word, ex.Difficulty, ex.Language);
            }
            catch (System.Exception ex)
            {
                // ÄNDRAD
                FeedbackMessage = _strings.CommonErrorDatabaseError(ex.Message);
            }
        }

        private WordDifficulty GetDifficultyByLength(string word)
        {
            if (word.Length <= 4) return WordDifficulty.Easy;
            if (word.Length <= 7) return WordDifficulty.Medium;
            return WordDifficulty.Hard;
        }
    }
}