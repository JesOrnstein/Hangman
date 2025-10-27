using Hangman.Core.Localizations;
using Hangman.Core.Models;

namespace Hangman.WPF.ViewModels
{
    /// <summary>
    /// Denna klass är vår "Context" i Strategy Pattern, anpassad för MVVM.
    /// Den ärver BaseViewModel för att kunna skicka INotifyPropertyChanged
    /// när språket byts.
    /// Den implementerar IUiStrings så att vi kan binda direkt till dess
    /// egenskaper från XAML.
    /// </summary>
    public class LocalizationProvider : BaseViewModel, IUiStrings
    {
        private IUiStrings _currentStrategy;

        public LocalizationProvider(IUiStrings defaultStrategy)
        {
            _currentStrategy = defaultStrategy;
        }

        /// <summary>
        /// Byter den aktiva språkstrategin och meddelar hela UI:t
        /// att alla text-bindningar måste uppdateras.
        /// </summary>
        public void SetStrategy(IUiStrings newStrategy)
        {
            _currentStrategy = newStrategy;

            // Anropar OnPropertyChanged med 'null' (eller string.Empty) 
            // för att signalera att ALLA properties på detta objekt 
            // har ändrats. Detta tvingar hela UI:t att ladda om alla strängar.
            OnPropertyChanged(null);
        }

        // --- IMPLEMENTATION AV IUiStrings ---
        // Alla egenskaper och metoder nedan delegerar helt enkelt
        // anropet till den _currentStrategy som är aktiv för tillfället.
        // När SetStrategy() anropas, kommer alla dessa att returnera
        // de nya strängarna automatiskt.

        #region Välkomstskärm
        public string WelcomeTitleArt => _currentStrategy.WelcomeTitleArt;
        public string WelcomeMessage => _currentStrategy.WelcomeMessage;
        public string WelcomePressAnyKey => _currentStrategy.WelcomePressAnyKey;
        #endregion

        #region Huvudmeny
        public string MainMenuTitleArt => _currentStrategy.MainMenuTitleArt;
        public string MainMenuGallowsArt => _currentStrategy.MainMenuGallowsArt;
        public string MainMenuTitle => _currentStrategy.MainMenuTitle;
        public string MenuPlaySingle => _currentStrategy.MenuPlaySingle;
        public string MenuPlayTournament => _currentStrategy.MenuPlayTournament;
        public string MenuShowHighscores => _currentStrategy.MenuShowHighscores;
        public string MenuAddWord => _currentStrategy.MenuAddWord;
        public string MenuHelp => _currentStrategy.MenuHelp;
        public string MenuQuit => _currentStrategy.MenuQuit;
        public string FeedbackThanksForPlaying => _currentStrategy.FeedbackThanksForPlaying;

        public string MenuChoicePrompt => _currentStrategy.MenuChoicePrompt;
        #endregion

        #region Val av ordkälla
        public string SelectWordSourceTitle => _currentStrategy.SelectWordSourceTitle;
        public string SelectWordSourceApi => _currentStrategy.SelectWordSourceApi;
        public string SelectWordSourceLocal => _currentStrategy.SelectWordSourceLocal;
        public string SelectWordSourceCustomSwedish => _currentStrategy.SelectWordSourceCustomSwedish;
        public string SelectWordSourceCustomEnglish => _currentStrategy.SelectWordSourceCustomEnglish;
        public string SelectWordSourcePrompt => _currentStrategy.SelectWordSourcePrompt;
        public string FeedbackWordSourceApi => _currentStrategy.FeedbackWordSourceApi;
        public string FeedbackWordSourceLocal => _currentStrategy.FeedbackWordSourceLocal;
        public string FeedbackWordSourceCustomSwedish => _currentStrategy.FeedbackWordSourceCustomSwedish;
        public string FeedbackWordSourceCustomEnglish => _currentStrategy.FeedbackWordSourceCustomEnglish;
        #endregion

        #region Val av svårighetsgrad
        public string SelectDifficultyTitle(string source) => _currentStrategy.SelectDifficultyTitle(source);
        public string SelectDifficultyEasy => _currentStrategy.SelectDifficultyEasy;
        public string SelectDifficultyMedium => _currentStrategy.SelectDifficultyMedium;
        public string SelectDifficultyHard => _currentStrategy.SelectDifficultyHard;
        public string SelectDifficultyPrompt => _currentStrategy.SelectDifficultyPrompt;
        public string FeedbackDifficultyEasy => _currentStrategy.FeedbackDifficultyEasy;
        public string FeedbackDifficultyMedium => _currentStrategy.FeedbackDifficultyMedium;
        public string FeedbackDifficultyHard => _currentStrategy.FeedbackDifficultyHard;
        #endregion

        #region Gemensamt
        public string CommonPressEscapeToCancel => _currentStrategy.CommonPressEscapeToCancel;
        public string CommonPressAnyKeyToContinue => _currentStrategy.CommonPressAnyKeyToContinue;
        public string CommonFeedbackCancelling => _currentStrategy.CommonFeedbackCancelling;
        public string CommonErrorDatabaseError(string message) => _currentStrategy.CommonErrorDatabaseError(message);
        #endregion

        #region Spela (Enspelare)
        public string PromptPlayerName => _currentStrategy.PromptPlayerName;
        public string FeedbackFetchingWord(string source) => _currentStrategy.FeedbackFetchingWord(source);
        public string ErrorCouldNotStartGame(string message) => _currentStrategy.ErrorCouldNotStartGame(message);
        public string FeedbackWonRound(int wins) => _currentStrategy.FeedbackWonRound(wins);
        public string PromptContinuePlaying => _currentStrategy.PromptContinuePlaying;
        public string FeedbackEndingLoop => _currentStrategy.FeedbackEndingLoop;
        public string FeedbackContinuing => _currentStrategy.FeedbackContinuing;
        public string FeedbackPressAnyKeyToSave => _currentStrategy.FeedbackPressAnyKeyToSave;
        public string FeedbackHighscoreSaved(int wins, WordDifficulty difficulty) => _currentStrategy.FeedbackHighscoreSaved(wins, difficulty);
        public string FeedbackReturningToMenu => _currentStrategy.FeedbackReturningToMenu;
        public string ErrorNoCustomWordsFound(WordDifficulty difficulty, WordLanguage language) => _currentStrategy.ErrorNoCustomWordsFound(difficulty, language);
        #endregion

        #region Spela (Turnering)
        public string TournamentTitle => _currentStrategy.TournamentTitle;
        public string PromptPlayer1Name => _currentStrategy.PromptPlayer1Name;
        public string PromptPlayer2Name => _currentStrategy.PromptPlayer2Name;
        public string FeedbackTournamentStarting(string p1, string p2, string source, string firstGuesser, int lives) => _currentStrategy.FeedbackTournamentStarting(p1, p2, source, firstGuesser, lives);
        public string FeedbackPressToStartRound => _currentStrategy.FeedbackPressToStartRound;
        public string ErrorCouldNotFetchTournamentWord(string message) => _currentStrategy.ErrorCouldNotFetchTournamentWord(message);
        public string FeedbackTournamentRoundEnded => _currentStrategy.FeedbackTournamentRoundEnded;
        public string FeedbackTournamentLives(string p1Name, int p1Lives, string p2Name, int p2Lives) => _currentStrategy.FeedbackTournamentLives(p1Name, p1Lives, p2Name, p2Lives);
        public string FeedbackTournamentNextGuesser(string guesserName) => _currentStrategy.FeedbackTournamentNextGuesser(guesserName);
        public string FeedbackTournamentCancelled => _currentStrategy.FeedbackTournamentCancelled;
        public string FeedbackTournamentEnded => _currentStrategy.FeedbackTournamentEnded;
        public string FeedbackTournamentDraw => _currentStrategy.FeedbackTournamentDraw;
        public string FeedbackTournamentWinner(string winnerName) => _currentStrategy.FeedbackTournamentWinner(winnerName);
        public string FeedbackTournamentLoser(string loserName) => _currentStrategy.FeedbackTournamentLoser(loserName);
        public string FeedbackTournamentFinalWins => _currentStrategy.FeedbackTournamentFinalWins;
        public string FeedbackTournamentPlayerWins(string playerName, int wins) => _currentStrategy.FeedbackTournamentPlayerWins(playerName, wins);
        public string FeedbackTournamentUnexpectedEnd => _currentStrategy.FeedbackTournamentUnexpectedEnd;
        #endregion

        #region Highscores
        public string HighscoreTitle => _currentStrategy.HighscoreTitle;
        public string HighscoreFetching => _currentStrategy.HighscoreFetching;
        public string HighscoreNoneFound => _currentStrategy.HighscoreNoneFound;
        public string HighscoreDifficultyHeader(WordDifficulty difficulty) => _currentStrategy.HighscoreDifficultyHeader(difficulty);
        public string HighscoreEntry(int rank, string name, int wins) => _currentStrategy.HighscoreEntry(rank, name, wins);
        #endregion

        #region Lägg till ord
        public string AddWordTitle => _currentStrategy.AddWordTitle;
        public string AddWordPrompt => _currentStrategy.AddWordPrompt;
        public string AddWordInvalid => _currentStrategy.AddWordInvalid;
        public string AddWordSuccess(string word, WordDifficulty difficulty, WordLanguage language) => _currentStrategy.AddWordSuccess(word, difficulty, language);
        public string AddWordErrorExists(string message) => _currentStrategy.AddWordErrorExists(message);
        public string AddWordSelectLanguageTitle => _currentStrategy.AddWordSelectLanguageTitle;
        public string AddWordSelectLanguagePrompt => _currentStrategy.AddWordSelectLanguagePrompt;
        public string AddWordLanguageSwedish => _currentStrategy.AddWordLanguageSwedish;
        public string AddWordLanguageEnglish => _currentStrategy.AddWordLanguageEnglish;
        public string ErrorWordAlreadyExists(string word, WordDifficulty difficulty, WordLanguage language) => _currentStrategy.ErrorWordAlreadyExists(word, difficulty, language);
        #endregion

        #region Hjälpskärm
        public string HelpTitle => _currentStrategy.HelpTitle;
        public string HelpLine1 => _currentStrategy.HelpLine1;
        public string HelpLine2 => _currentStrategy.HelpLine2;
        public string HelpModesTitle => _currentStrategy.HelpModesTitle;
        public string HelpModesLine1 => _currentStrategy.HelpModesLine1;
        public string HelpModesLine2 => _currentStrategy.HelpModesLine2;
        public string HelpSourcesTitle => _currentStrategy.HelpSourcesTitle;
        public string HelpSourcesLine1 => _currentStrategy.HelpSourcesLine1;
        public string HelpSourcesLine2 => _currentStrategy.HelpSourcesLine2;
        public string HelpSourcesLine3 => _currentStrategy.HelpSourcesLine3;
        #endregion

        #region Spelrunda
        public string RoundTitleNewRound => _currentStrategy.RoundTitleNewRound;
        public string RoundTitleGame => _currentStrategy.RoundTitleGame;
        public string RoundActivePlayer => _currentStrategy.RoundActivePlayer;
        public string RoundActivePlayerWithLives(string playerName, int lives) => _currentStrategy.RoundActivePlayerWithLives(playerName, lives);
        public string RoundWord => _currentStrategy.RoundWord;
        public string RoundMistakes(int mistakes, int max) => _currentStrategy.RoundMistakes(mistakes, max);
        public string RoundGuessedLetters => _currentStrategy.RoundGuessedLetters;
        public string RoundFeedbackCorrectGuess(char guess) => _currentStrategy.RoundFeedbackCorrectGuess(guess);
        public string RoundFeedbackWrongGuess(char guess) => _currentStrategy.RoundFeedbackWrongGuess(guess);
        public string RoundFeedbackCancelled => _currentStrategy.RoundFeedbackCancelled;
        public string RoundLost => _currentStrategy.RoundLost;
        public string RoundWon => _currentStrategy.RoundWon;
        #endregion

        #region Slutskärm
        public string EndScreenCongrats => _currentStrategy.EndScreenCongrats;
        public string EndScreenCancelled => _currentStrategy.EndScreenCancelled;
        public string EndScreenLost => _currentStrategy.EndScreenLost;
        public string EndScreenCorrectWord(string secret) => _currentStrategy.EndScreenCorrectWord(secret);
        #endregion

        #region Input-hantering
        public string GetGuessPrompt => _currentStrategy.GetGuessPrompt;
        public string GetGuessInvalid(char letter) => _currentStrategy.GetGuessInvalid(letter);
        public string GetGuessAlreadyGuessed(char letter) => _currentStrategy.GetGuessAlreadyGuessed(letter);
        public string GetPlayerNameEmpty => _currentStrategy.GetPlayerNameEmpty;
        #endregion

        #region Timer
        public string RoundTimerDisplay(int seconds) => _currentStrategy.RoundTimerDisplay(seconds);
        public string RoundTimerExpired => _currentStrategy.RoundTimerExpired;
        #endregion

        // ==================================================================
        // === ALLA SAKNADE PROPERTIES SOM ORSAKADE FELEN ÄR TILLAGDA HÄR ===
        // ==================================================================

        #region WPF-Specifika Strängar
        public string ButtonBackToMenu => _currentStrategy.ButtonBackToMenu;
        public string MainMenuWpfTitle => _currentStrategy.MainMenuWpfTitle;
        public string LanguageSelectorTitle => _currentStrategy.LanguageSelectorTitle;
        public string LanguageButtonSwedish => _currentStrategy.LanguageButtonSwedish;
        public string LanguageButtonEnglish => _currentStrategy.LanguageButtonEnglish;
        public string SettingsTitle => _currentStrategy.SettingsTitle;
        public string SettingsLabelDifficulty => _currentStrategy.SettingsLabelDifficulty;
        public string SettingsLabelWordList => _currentStrategy.SettingsLabelWordList;
        public string SettingsButtonStart => _currentStrategy.SettingsButtonStart;
        public string AddWordLabelLanguage => _currentStrategy.AddWordLabelLanguage;
        public string AddWordButtonSave => _currentStrategy.AddWordButtonSave;
        public string HighscoreHeaderName => _currentStrategy.HighscoreHeaderName;
        public string HighscoreHeaderWins => _currentStrategy.HighscoreHeaderWins;
        public string HighscoreHeaderDifficulty => _currentStrategy.HighscoreHeaderDifficulty;
        public string GameButtonGiveUp => _currentStrategy.GameButtonGiveUp;
        public string GameButtonPlayAgain => _currentStrategy.GameButtonPlayAgain;
        public string GameButtonSaveAndExit => _currentStrategy.GameButtonSaveAndExit;
        public string TournamentButtonNextRound => _currentStrategy.TournamentButtonNextRound;
        public string TournamentButtonCancel => _currentStrategy.TournamentButtonCancel;
        public string ErrorApiGeneric => _currentStrategy.ErrorApiGeneric;
        public string HighscoreStatusLoading => _currentStrategy.HighscoreStatusLoading;
        public string HighscoreStatusError(string message) => _currentStrategy.HighscoreStatusError(message);
        public string HighscoreStatusNoneFoundWpf => _currentStrategy.HighscoreStatusNoneFoundWpf;
        public string TournamentHeaderLives => _currentStrategy.TournamentHeaderLives;
        public string TournamentHeaderWins => _currentStrategy.TournamentHeaderWins;
        #endregion
    }
}