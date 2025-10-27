using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Models;

namespace Hangman.Core.Localizations
{
    /// <summary>
    /// Definierar kontraktet för alla UI-strängar (Strategy Pattern).
    /// ConsoleUi kommer att använda detta interface istället för hårdkodade strängar.
    /// </summary>
    public interface IUiStrings
    {
        // Välkomstskärm
        string WelcomeTitleArt { get; }
        string WelcomeMessage { get; }
        string WelcomePressAnyKey { get; }

        // Huvudmeny
        string MainMenuTitleArt { get; }
        string MainMenuGallowsArt { get; }
        string MainMenuTitle { get; }
        string MenuPlaySingle { get; }
        string MenuPlayTournament { get; }
        string MenuShowHighscores { get; }
        string MenuAddWord { get; }
        string MenuHelp { get; }
        string MenuQuit { get; }
        string MenuChoicePrompt { get; }
        string FeedbackThanksForPlaying { get; }

        // Val av ordkälla
        string SelectWordSourceTitle { get; }
        string SelectWordSourceApi { get; }
        string SelectWordSourceLocal { get; }
        string SelectWordSourceCustomSwedish { get; }
        string SelectWordSourceCustomEnglish { get; }
        string SelectWordSourcePrompt { get; }
        string FeedbackWordSourceApi { get; }
        string FeedbackWordSourceLocal { get; }
        string FeedbackWordSourceCustomSwedish { get; }
        string FeedbackWordSourceCustomEnglish { get; }

        // Val av svårighetsgrad
        string SelectDifficultyTitle(string source);
        string SelectDifficultyEasy { get; }
        string SelectDifficultyMedium { get; }
        string SelectDifficultyHard { get; }
        string SelectDifficultyPrompt { get; }
        string FeedbackDifficultyEasy { get; }
        string FeedbackDifficultyMedium { get; }
        string FeedbackDifficultyHard { get; }

        // Gemensamt
        string CommonPressEscapeToCancel { get; }
        string CommonPressAnyKeyToContinue { get; }
        string CommonFeedbackCancelling { get; }
        string CommonErrorDatabaseError(string message);

        // Spela (Enspelare)
        string PromptPlayerName { get; }
        string FeedbackFetchingWord(string source);
        string ErrorCouldNotStartGame(string message);
        string FeedbackWonRound(int wins);
        string PromptContinuePlaying { get; }
        string FeedbackEndingLoop { get; }
        string FeedbackContinuing { get; }
        string FeedbackPressAnyKeyToSave { get; }
        string FeedbackHighscoreSaved(int wins, WordDifficulty difficulty);
        string FeedbackReturningToMenu { get; }
        string ErrorNoCustomWordsFound(WordDifficulty difficulty, WordLanguage language); // <-- NY METOD

        // Spela (Turnering)
        string TournamentTitle { get; }
        string PromptPlayer1Name { get; }
        string PromptPlayer2Name { get; }
        string FeedbackTournamentStarting(string p1, string p2, string source, string firstGuesser, int lives);
        string FeedbackPressToStartRound { get; }
        string ErrorCouldNotFetchTournamentWord(string message);
        string FeedbackTournamentRoundEnded { get; }
        string FeedbackTournamentLives(string p1Name, int p1Lives, string p2Name, int p2Lives);
        string FeedbackTournamentNextGuesser(string guesserName);
        string FeedbackTournamentCancelled { get; }
        string FeedbackTournamentEnded { get; }
        string FeedbackTournamentDraw { get; }
        string FeedbackTournamentWinner(string winnerName);
        string FeedbackTournamentLoser(string loserName);
        string FeedbackTournamentFinalWins { get; }
        string FeedbackTournamentPlayerWins(string playerName, int wins);
        string FeedbackTournamentUnexpectedEnd { get; }

        // Highscores
        string HighscoreTitle { get; }
        string HighscoreFetching { get; }
        string HighscoreNoneFound { get; }
        string HighscoreDifficultyHeader(WordDifficulty difficulty);
        string HighscoreEntry(int rank, string name, int wins);

        // Lägg till ord
        string AddWordTitle { get; }
        string AddWordPrompt { get; }
        string AddWordInvalid { get; }
        string AddWordSuccess(string word, WordDifficulty difficulty, WordLanguage language);
        string AddWordErrorExists(string message);
        string AddWordSelectLanguageTitle { get; }
        string AddWordSelectLanguagePrompt { get; }
        string AddWordLanguageSwedish { get; }
        string AddWordLanguageEnglish { get; }
        string ErrorWordAlreadyExists(string word, WordDifficulty difficulty, WordLanguage language); // <-- NY METOD

        // Hjälpskärm
        string HelpTitle { get; }
        string HelpLine1 { get; }
        string HelpLine2 { get; }
        string HelpModesTitle { get; }
        string HelpModesLine1 { get; }
        string HelpModesLine2 { get; }
        string HelpSourcesTitle { get; }
        string HelpSourcesLine1 { get; }
        string HelpSourcesLine2 { get; }
        string HelpSourcesLine3 { get; }

        // Spelrunda
        string RoundTitleNewRound { get; }
        string RoundTitleGame { get; }
        string RoundActivePlayer { get; }
        string RoundActivePlayerWithLives(string playerName, int lives);
        string RoundWord { get; }
        string RoundMistakes(int mistakes, int max);
        string RoundGuessedLetters { get; }
        string RoundFeedbackCorrectGuess(char guess);
        string RoundFeedbackWrongGuess(char guess);
        string RoundFeedbackCancelled { get; }
        string RoundLost { get; }
        string RoundWon { get; }

        // Slutskärm
        string EndScreenCongrats { get; }
        string EndScreenCancelled { get; }
        string EndScreenLost { get; }
        string EndScreenCorrectWord(string secret);

        // Input-hantering
        string GetGuessPrompt { get; }
        string GetGuessInvalid(char letter);
        string GetGuessAlreadyGuessed(char letter);
        string GetPlayerNameEmpty { get; }

        // --- NYTT FÖR TIMER ---
        string RoundTimerDisplay(int seconds);
        string RoundTimerExpired { get; }
    }
}