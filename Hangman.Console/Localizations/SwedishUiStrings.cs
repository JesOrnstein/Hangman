using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Models;

namespace Hangman.Console.Localizations
{
    public class SwedishUiStrings : IUiStrings
    {
        // Välkomstskärm
        public string WelcomeTitleArt => @"´      
      _   _   _ _ _   _    _   ____     _            ____   _   _   ____   ____   ____   ___________.._______
     | | | |   / \    | \ | | / ___|   / \          / ___| | | | | | __ \ | __ \ | ___| | .__________))______|
     | |_| |  / _ \   |  \| | | |  _  / _ \        | |  _  | | | | | |_) || |_) || |__  | | / /      || 
     |  _  | / ___ \  | |\  | | |_| |/ ___ \       | |_| | | |_| | | |_)  | |_) || ___| | |/ /       ||  
     |_| |_|/_/   \_\ |_| \_| \____|/_/   \_\       \____| \_____/ |____/ |____/ |____| | | /        ||.-''.
                                                                                        | |/         |/  _  \
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
                                                                                        : :        \ \      : :                                                                                       . .         `'      . .
";
        public string WelcomeMessage => "\n\n          Välkommen till HÄNGA GUBBE!";
        public string WelcomePressAnyKey => "     Tryck valfri tangent för att starta...";

        // Huvudmeny
        public string MainMenuTitleArt => @"
      _   _   _ _ _   _    _   ____     _            ____   _   _   ____   ____   ____ 
     | | | |   / \    | \ | | / ___|   / \          / ___| | | | | | __ \ | __ \ | ___|
     | |_| |  / _ \   |  \| | | |  _  / _ \        | |  _  | | | | | |_) || |_) || |__  
     |  _  | / ___ \  | |\  | | |_| |/ ___ \       | |_| | | |_| | | |_)  | |_) || ___| 
     |_| |_|/_/   \_\ |_| \_| \____|/_/   \_\       \____| \_____/ |____/ |____/ |____|
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
        public string MainMenuTitle => "--- HUVUDMENY ---";
        public string MenuPlaySingle => "1. Spela (Enspelare - Highscore)";
        public string MenuPlayTournament => "2. Spela (Tvåspelare, turnering)";
        public string MenuShowHighscores => "3. Visa Highscores";
        public string MenuAddWord => "4. Lägg till nytt ord";
        public string MenuHelp => "5. Hjälp / Hur man spelar";
        public string MenuQuit => "6. Avsluta";
        public string MenuChoicePrompt => "\nDitt val (1-6): ";
        public string FeedbackThanksForPlaying => "Tack för att du spelade!";

        // Val av ordkälla
        public string SelectWordSourceTitle => "--- VÄLJ ORDLISTA ---";
        public string SelectWordSourceApi => "1. Engelska (API - olika längd)";
        public string SelectWordSourceLocal => "2. Svenska (Lokal lista)";
        public string SelectWordSourceCustomSwedish => "3. Anpassad Ordlista (Svenska)"; // ÄNDRAD
        public string SelectWordSourceCustomEnglish => "4. Anpassad Ordlista (Engelska)"; // NY
        public string SelectWordSourcePrompt => "\nVälj (1-4): "; // ÄNDRAD
        public string FeedbackWordSourceApi => "Engelska (API)";
        public string FeedbackWordSourceLocal => "Svenska (Lokal)";
        public string FeedbackWordSourceCustomSwedish => "Anpassad Ordlista (Svenska)"; // ÄNDRAD
        public string FeedbackWordSourceCustomEnglish => "Anpassad Ordlista (Engelska)"; // NY

        // Val av svårighetsgrad
        public string SelectDifficultyTitle(string source) => $"--- VÄLJ SVÅRIGHETSGRAD ({source}) ---";
        public string SelectDifficultyEasy => "1. Lätt (3-4 bokstäver)";
        public string SelectDifficultyMedium => "2. Medium (5-7 bokstäver)";
        public string SelectDifficultyHard => "3. Svår (8-11 bokstäver)";
        public string SelectDifficultyPrompt => "\nVälj (1-3): ";
        public string FeedbackDifficultyEasy => "Lätt";
        public string FeedbackDifficultyMedium => "Medium";
        public string FeedbackDifficultyHard => "Svår";

        // Gemensamt
        public string CommonPressEscapeToCancel => "(Tryck Escape när som helst för att avbryta)";
        public string CommonPressAnyKeyToContinue => "\nTryck valfri tangent för att återgå till menyn...";
        public string CommonFeedbackCancelling => "Avbryter...";
        public string CommonErrorDatabaseError(string message) => $"\nEtt databasfel inträffade: {message}";

        // Spela (Enspelare)
        public string PromptPlayerName => "Ange ditt namn för highscore: ";
        public string FeedbackFetchingWord(string source) => $"Hämtar ord från: {source}...";
        public string ErrorCouldNotStartGame(string message) => $"\nKunde inte starta spelet (Ordlistefel):\n{message}";
        public string FeedbackWonRound(int wins) => $"\n** Du vann runda {wins} i rad! **";
        public string PromptContinuePlaying => "\nVill du fortsätta spela (J/N)? (Escape för att avsluta) ";
        public string FeedbackEndingLoop => "\nAvslutar spelloop...";
        public string FeedbackContinuing => "\nFortsätter!";
        public string FeedbackPressAnyKeyToSave => "\nTryck valfri tangent för att spara highscore...";
        public string FeedbackHighscoreSaved(int wins, WordDifficulty difficulty) => $"\nHighscore ({wins} vinster i rad) sparat för {difficulty}!";
        public string FeedbackReturningToMenu => "\nÅtergår till huvudmenyn...";

        // Spela (Turnering)
        public string TournamentTitle => "--- TVÅSPELARTURNERING ---";
        public string PromptPlayer1Name => "Ange namn för Spelare 1: ";
        public string PromptPlayer2Name => "Ange namn för Spelare 2: ";
        public string FeedbackTournamentStarting(string p1, string p2, string source, string firstGuesser, int lives) =>
            $"Turneringen startar! {p1} mot {p2}. Ordkälla: {source}\nFörsta gissare: {firstGuesser}. Liv: {lives} vardera.";
        public string FeedbackPressToStartRound => "Tryck valfri tangent för att starta den första rundan...";
        public string ErrorCouldNotFetchTournamentWord(string message) => $"\nKunde inte hämta ord till turneringen:\n{message}";
        public string FeedbackTournamentRoundEnded => "\n--- RUNDA AVSLUTAD ---";
        public string FeedbackTournamentLives(string p1Name, int p1Lives, string p2Name, int p2Lives) =>
            $"{p1Name} Liv: {p1Lives} | {p2Name} Liv: {p2Lives}";
        public string FeedbackTournamentNextGuesser(string guesserName) => $"Nästa gissare: {guesserName}. Tryck för nästa runda...";
        public string FeedbackTournamentCancelled => "Turneringen avbröts.";
        public string FeedbackTournamentEnded => "\n--- TURNERING AVSLUTAD ---";
        public string FeedbackTournamentDraw => "OAVGJORT! Båda spelarna förlorade alla sina liv.";
        public string FeedbackTournamentWinner(string winnerName) => $"GRATTIS, {winnerName} VANN TURNERINGEN!";
        public string FeedbackTournamentLoser(string loserName) => $"{loserName} förlorade alla sina liv. Vunna rundor:";
        public string FeedbackTournamentFinalWins => "Vunna rundor:";
        public string FeedbackTournamentPlayerWins(string playerName, int wins) => $"- {playerName}: {wins} rundor";
        public string FeedbackTournamentUnexpectedEnd => "\nTurneringen avslutad (Oväntat slut).";

        // Highscores
        public string HighscoreTitle => "--- HIGHSCORES ---";
        public string HighscoreFetching => "Hämtar globala topp 10-resultat...";
        public string HighscoreNoneFound => "\nInga highscores sparade än. Spela en runda!";
        public string HighscoreDifficultyHeader(WordDifficulty difficulty) => $"\n--- SVÅRIGHETSGRAD: {difficulty} ---";
        public string HighscoreEntry(int rank, string name, int wins) => $"{rank}. {name} - {wins} vinster i rad.";

        // Lägg till ord
        public string AddWordTitle => "--- LÄGG TILL NYTT ORD ---";
        public string AddWordPrompt => "Ange ordet (A-Ö): ";
        public string AddWordInvalid => "\nOgiltigt ord. Ange endast bokstäver.";
        public string AddWordSuccess(string word, WordDifficulty difficulty, WordLanguage language) => $"\nOrdet '{word}' har lagts till i den anpassade listan ({language} - {difficulty})!"; // ÄNDRAD
        public string AddWordErrorExists(string message) => $"\nKunde inte lägga till ordet: {message}";
        public string AddWordSelectLanguageTitle => "--- VÄLJ SPRÅK FÖR ORDET ---"; // NY
        public string AddWordSelectLanguagePrompt => "\nVälj språk (1-2): "; // NY
        public string AddWordLanguageSwedish => "1. Svenska"; // NY
        public string AddWordLanguageEnglish => "2. Engelska"; // NY

        // Hjälpskärm
        public string HelpTitle => "--- HJÄLP / HUR MAN SPELAR ---";
        public string HelpLine1 => "Spelet går ut på att gissa det hemliga ordet, bokstav för bokstav.";
        public string HelpLine2 => "Du har 6 gissningar på dig innan gubben hängs (MAX 6 fel).";
        public string HelpModesTitle => "\nLägen:";
        public string HelpModesLine1 => "  1. Enspelare (Highscore): Spelet fortsätter tills du förlorar.";
        public string HelpModesLine2 => "  2. Turnering (Tvåspelare): Ni har 3 liv var. Liv återställs vid vinst.";
        public string HelpSourcesTitle => "\nOrdkällor:";
        public string HelpSourcesLine1 => "  API: Engelska ord med längd baserat på svårighetsgrad.";
        public string HelpSourcesLine2 => "  Lokal: Svenska ord från en inbyggd lista.";
        public string HelpSourcesLine3 => "  Anpassad: Ord som du själv har lagt till (både svenska och engelska)."; // ÄNDRAD

        // Spelrunda
        public string RoundTitleNewRound => "--- NY RUNDA STARTAD ---";
        public string RoundTitleGame => "--- HÄNGA GUBBE ---";
        public string RoundActivePlayer => "Aktiv spelare:";
        public string RoundActivePlayerWithLives(string playerName, int lives) => $"Aktiv spelare: {playerName} | Liv: {lives}";
        public string RoundWord => "Ordet:";
        public string RoundMistakes(int mistakes, int max) => $"Felgissningar: {mistakes} (av {max})";
        public string RoundGuessedLetters => "Gissade:";
        public string RoundFeedbackCorrectGuess(char guess) => $"Korrekt! '{guess}' finns i ordet.";
        public string RoundFeedbackWrongGuess(char guess) => $"Fel! '{guess}' finns inte i ordet.";
        public string RoundFeedbackCancelled => "Rundan avbröts av spelaren.";
        public string RoundLost => "förlorade rundan.";
        public string RoundWon => "vann rundan!";

        // Slutskärm
        public string EndScreenCongrats => "\nGRATTIS, DU VANN!";
        public string EndScreenCancelled => "\nDu avbröt rundan.";
        public string EndScreenLost => "\nDU FÖRLORADE...";
        public string EndScreenCorrectWord(string secret) => $"Det rätta ordet var: {secret}";

        // Input-hantering
        public string GetGuessPrompt => "Gissa på en bokstav (eller Escape för att ge upp): ";
        public string GetGuessInvalid(char letter) => $"\nOgiltig gissning '{letter}'. Endast bokstäver (A-Ö).";
        public string GetGuessAlreadyGuessed(char letter) => $"\nDu har redan gissat på '{letter}'. Försök igen.";
        public string GetPlayerNameEmpty => "\nNamnet kan inte vara tomt. Försök igen (eller Escape för att backa).";

        // --- NYA IMPLEMENTATIONER FÖR EXCEPTIONS ---

        public string ErrorWordAlreadyExists(string word, WordDifficulty difficulty, WordLanguage language)
            => $"\nKunde inte lägga till ordet: '{word}' finns redan i listan för {difficulty} ({language}).";

        public string ErrorNoCustomWordsFound(WordDifficulty difficulty, WordLanguage language)
            => $"\nKunde inte starta spelet: Hittade inga anpassade ord i listan för {difficulty} ({language}). Lägg till ord via menyn.";

        // --- NYTT FÖR TIMER ---
        public string RoundTimerDisplay(int seconds) => $"Tid kvar: {seconds:D2}s";
        public string RoundTimerExpired => "Tiden är ute!";
    }
}