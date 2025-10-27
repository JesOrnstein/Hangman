using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Models;

namespace Hangman.Console.Localizations
{
    public class EnglishUiStrings : IUiStrings
    {
        // Välkomstskärm
        public string WelcomeTitleArt => @"                                                              ___________.._______
     _   _    _    _   _  ____ __  __    _    _   _          | .__________))______|
    | | | |  / \  | \ | |/ ___|  \/  |  / \  | \ | |         | | / /      ||
    | |_| | / _ \ |  \| | |  _| |\/| | / _ \ |  \| |         | |/ /       ||
    |  _  |/ ___ \| |\  | |_| | |  | |/ ___ \| |\  |         | | /        ||.-''.
    |_| |_/_/   \_\_| \_|\____|_|  |_/_/   \_\_| \_|         | |/         |/  _  \
                                                             | |          ||  `/,|
                                                             | |          (\\`_.'
                                                             | |         .-`--'.
                                                             | |        /Y . . Y\
                                                             | |       // |   | \\
                                                             | |      //  | . |  \\
                                                             | |     ')   |   |   (`
                                                             | |          ||'||
                                                             | |          || ||
                                                             | |          || ||                                                       
                                                             | |          || ||
                                                             | |         / | | \
                                                   """"""""""""""""""""|_`-' `-' |""""""|
                                                   |""|""""""""""""""\ \         |""|
                                                             | |      \ \        | |
                                                             | |       \ \       | |
                                                             : :        \ \      : :
                                                             . .         `'      . .
";
        public string WelcomeMessage => "\n\n          Welcome to HANGMAN!";
        public string WelcomePressAnyKey => "     Press any key to start...";

        // Huvudmeny
        public string MainMenuTitleArt => @"
     _   _    _    _   _  ____ __  __    _    _   _
    | | | |  / \  | \ | |/ ___|  \/  |  / \  | \ | |
    | |_| | / _ \ |  \| | |  _| |\/| | / _ \ |  \| |
    |  _  |/ ___ \| |\  | |_| | |  | |/ ___ \| |\  |
    |_| |_/_/   \_\_| \_|\____|_|  |_/_/   \_\_| \_|
            ";
        public string MainMenuGallowsArt => @"
        +-------+
        |       |
        |       e
        |      /|\
        |      / \
        |
    ------------
   /            \
  /              \
 =================
";
        public string MainMenuTitle => "--- MAIN MENU ---";
        public string MenuPlaySingle => "1. Play (Single Player - Highscore)";
        public string MenuPlayTournament => "2. Play (Two Player, Tournament)";
        public string MenuShowHighscores => "3. View Highscores";
        public string MenuAddWord => "4. Add new word";
        public string MenuHelp => "5. Help / How to play";
        public string MenuQuit => "6. Quit";
        public string MenuChoicePrompt => "\nYour choice (1-6): ";
        public string FeedbackThanksForPlaying => "Thanks for playing!";

        // Val av ordkälla
        public string SelectWordSourceTitle => "--- CHOOSE WORD LIST ---";
        public string SelectWordSourceApi => "1. English (API - varied length)";
        public string SelectWordSourceLocal => "2. Swedish (Local list)";
        public string SelectWordSourceCustomSwedish => "3. Custom Word List (Swedish)"; // ÄNDRAD
        public string SelectWordSourceCustomEnglish => "4. Custom Word List (English)"; // NY
        public string SelectWordSourcePrompt => "\nChoose (1-4): "; // ÄNDRAD
        public string FeedbackWordSourceApi => "English (API)";
        public string FeedbackWordSourceLocal => "Swedish (Local)";
        public string FeedbackWordSourceCustomSwedish => "Custom Word List (Swedish)"; // ÄNDRAD
        public string FeedbackWordSourceCustomEnglish => "Custom Word List (English)"; // NY

        // Val av svårighetsgrad
        public string SelectDifficultyTitle(string source) => $"--- CHOOSE DIFFICULTY ({source}) ---";
        public string SelectDifficultyEasy => "1. Easy (3-4 letters)";
        public string SelectDifficultyMedium => "2. Medium (5-7 letters)";
        public string SelectDifficultyHard => "3. Hard (8-11 letters)";
        public string SelectDifficultyPrompt => "\nChoose (1-3): ";
        public string FeedbackDifficultyEasy => "Easy";
        public string FeedbackDifficultyMedium => "Medium";
        public string FeedbackDifficultyHard => "Hard";

        // Gemensamt
        public string CommonPressEscapeToCancel => "(Press Escape at any time to cancel)";
        public string CommonPressAnyKeyToContinue => "\nPress any key to return to the menu...";
        public string CommonFeedbackCancelling => "Cancelling...";
        public string CommonErrorDatabaseError(string message) => $"\nA database error occurred: {message}";

        // Spela (Enspelare)
        public string PromptPlayerName => "Enter your name for the highscore: ";
        public string FeedbackFetchingWord(string source) => $"Fetching word from: {source}...";
        public string ErrorCouldNotStartGame(string message) => $"\nCould not start game (Word list error):\n{message}";
        public string FeedbackWonRound(int wins) => $"\n** You've won {wins} round(s) in a row! **";
        public string PromptContinuePlaying => "\nDo you want to keep playing (Y/N)? (Escape to quit) ";
        public string FeedbackEndingLoop => "\nEnding game loop...";
        public string FeedbackContinuing => "\nContinuing!";
        public string FeedbackPressAnyKeyToSave => "\nPress any key to save highscore...";
        public string FeedbackHighscoreSaved(int wins, WordDifficulty difficulty) => $"\nHighscore ({wins} wins in a row) saved for {difficulty}!";
        public string FeedbackReturningToMenu => "\nReturning to main menu...";

        // Spela (Turnering)
        public string TournamentTitle => "--- TWO PLAYER TOURNAMENT ---";
        public string PromptPlayer1Name => "Enter name for Player 1: ";
        public string PromptPlayer2Name => "Enter name for Player 2: ";
        public string FeedbackTournamentStarting(string p1, string p2, string source, string firstGuesser, int lives) =>
            $"The tournament begins! {p1} vs {p2}. Word source: {source}\nFirst guesser: {firstGuesser}. Lives: {lives} each.";
        public string FeedbackPressToStartRound => "Press any key to start the first round...";
        public string ErrorCouldNotFetchTournamentWord(string message) => $"\nCould not fetch word for the tournament:\n{message}";
        public string FeedbackTournamentRoundEnded => "\n--- ROUND ENDED ---";
        public string FeedbackTournamentLives(string p1Name, int p1Lives, string p2Name, int p2Lives) =>
            $"{p1Name} Lives: {p1Lives} | {p2Name} Lives: {p2Lives}";
        public string FeedbackTournamentNextGuesser(string guesserName) => $"Next guesser: {guesserName}. Press for next round...";
        public string FeedbackTournamentCancelled => "Tournament cancelled.";
        public string FeedbackTournamentEnded => "\n--- TOURNAMENT ENDED ---";
        public string FeedbackTournamentDraw => "IT'S A DRAW! Both players lost all their lives.";
        public string FeedbackTournamentWinner(string winnerName) => $"CONGRATULATIONS, {winnerName} WON THE TOURNAMENT!";
        public string FeedbackTournamentLoser(string loserName) => $"{loserName} lost all their lives. Rounds won:";
        public string FeedbackTournamentFinalWins => "Rounds won:";
        public string FeedbackTournamentPlayerWins(string playerName, int wins) => $"- {playerName}: {wins} rounds";
        public string FeedbackTournamentUnexpectedEnd => "\nTournament finished (Unexpected end).";

        // Highscores
        public string HighscoreTitle => "--- HIGHSCORES ---";
        public string HighscoreFetching => "Fetching global top 10 results...";
        public string HighscoreNoneFound => "\nNo highscores saved yet. Play a round!";
        public string HighscoreDifficultyHeader(WordDifficulty difficulty) => $"\n--- DIFFICULTY: {difficulty} ---";
        public string HighscoreEntry(int rank, string name, int wins) => $"{rank}. {name} - {wins} wins in a row.";

        // Lägg till ord
        public string AddWordTitle => "--- ADD NEW WORD ---";
        public string AddWordPrompt => "Enter the word (A-Z): ";
        public string AddWordInvalid => "\nInvalid word. Please use letters only.";
        public string AddWordSuccess(string word, WordDifficulty difficulty, WordLanguage language) => $"\nThe word '{word}' has been added to the custom list ({language} - {difficulty})!"; // ÄNDRAD
        public string AddWordErrorExists(string message) => $"\nCould not add word: {message}";
        public string AddWordSelectLanguageTitle => "--- SELECT WORD LANGUAGE ---"; // NY
        public string AddWordSelectLanguagePrompt => "\nSelect language (1-2): "; // NY
        public string AddWordLanguageSwedish => "1. Swedish"; // NY
        public string AddWordLanguageEnglish => "2. English"; // NY

        // Hjälpskärm
        public string HelpTitle => "--- HELP / HOW TO PLAY ---";
        public string HelpLine1 => "The game is about guessing the secret word, one letter at a time.";
        public string HelpLine2 => "You have 6 guesses before the man is hung (MAX 6 mistakes).";
        public string HelpModesTitle => "\nModes:";
        public string HelpModesLine1 => "  1. Single Player (Highscore): The game continues until you lose.";
        public string HelpModesLine2 => "  2. Tournament (Two Player): You each have 3 lives. Lives are reset on a win.";
        public string HelpSourcesTitle => "\nWord Sources:";
        public string HelpSourcesLine1 => "  API: English words with length based on difficulty.";
        public string HelpSourcesLine2 => "  Local: Swedish words from a built-in list.";
        public string HelpSourcesLine3 => "  Custom: Words that you have added yourself (both Swedish and English)."; // ÄNDRAD

        // Spelrunda
        public string RoundTitleNewRound => "--- NEW ROUND STARTED ---";
        public string RoundTitleGame => "--- HANGMAN ---";
        public string RoundActivePlayer => "Active player:";
        public string RoundActivePlayerWithLives(string playerName, int lives) => $"Active player: {playerName} | Lives: {lives}";
        public string RoundWord => "The word:";
        public string RoundMistakes(int mistakes, int max) => $"Mistakes: {mistakes} (of {max})";
        public string RoundGuessedLetters => "Guessed:";
        public string RoundFeedbackCorrectGuess(char guess) => $"Correct! '{guess}' is in the word.";
        public string RoundFeedbackWrongGuess(char guess) => $"Wrong! '{guess}' is not in the word.";
        public string RoundFeedbackCancelled => "The round was cancelled by the player.";
        public string RoundLost => "lost the round.";
        public string RoundWon => "won the round!";

        // Slutskärm
        public string EndScreenCongrats => "\nCONGRATULATIONS, YOU WON!";
        public string EndScreenCancelled => "\nYou cancelled the round.";
        public string EndScreenLost => "\nYOU LOST...";
        public string EndScreenCorrectWord(string secret) => $"The correct word was: {secret}";

        // Input-hantering
        public string GetGuessPrompt => "Guess a letter (or Escape to give up): ";
        public string GetGuessInvalid(char letter) => $"\nInvalid guess '{letter}'. Only letters (A-Z).";
        public string GetGuessAlreadyGuessed(char letter) => $"\nYou have already guessed '{letter}'. Try again.";
        public string GetPlayerNameEmpty => "\nName cannot be empty. Try again (or Escape to go back).";

        // --- NEW IMPLEMENTATIONS FOR EXCEPTIONS ---

        public string ErrorWordAlreadyExists(string word, WordDifficulty difficulty, WordLanguage language)
            => $"\nCould not add word: '{word}' already exists in the list for {difficulty} ({language}).";

        public string ErrorNoCustomWordsFound(WordDifficulty difficulty, WordLanguage language)
            => $"\nCould not start game: No custom words found in the list for {difficulty} ({language}). Please add words via the menu.";

        // --- NYTT FÖR TIMER ---
        public string RoundTimerDisplay(int seconds) => $"Time left: {seconds:D2}s";
        public string RoundTimerExpired => "Time's up!";
    }
}