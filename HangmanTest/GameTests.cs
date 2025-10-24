using Hangman.Core;
using Xunit;

/*
  ───────────────────────────────────────────────────────────────
   CLASS: GameTests
   Testklass som verifierar kärnlogiken i klassen Game.

   Huvudsyfte:
   - Säkerställa att spelet initieras korrekt (StartNew)
   - Validera att gissningar hanteras rätt (Guess)
   - Kontrollera att vinst- och förlustvillkor fungerar som avsett
   - Verifiera att GetMaskedWord returnerar korrekt maskering
   - Testa robusthet och edge cases (t.ex. dubbletter, tomma ord, icke-bokstäver)

   Testerna följer AAA-mönstret (Arrange–Act–Assert)
   och innehåller beskrivande kommentarer för tydlighet.
  ───────────────────────────────────────────────────────────────
*/


namespace HangmanTest
{
    public class GameTests
    {
        [Fact]
        public void StartNew_ShouldInitializeGameCorrectly()
        {
            // Arrange
            var game = new Game();

            // Act
            game.StartNew("Test");

            // Assert
            Assert.Equal(GameStatus.InProgress, game.Status);  // Spelet ska starta i "pågående"-läge
            Assert.Equal("TEST", game.Secret);                 // Ordet sparas i versaler
            Assert.Equal(0, game.Mistakes);                    // Inga fel vid start
            Assert.Empty(game.UsedLetters);                    // Inga bokstäver gissade ännu
        }

        [Fact]
        public void Guess_CorrectLetter_ShouldReturnTrueAndNotIncreaseMistakes()
        {
            // Arrange
            var game = new Game();
            game.StartNew("TEST");

            // Act
            var result = game.Guess('T');

            // Assert
            Assert.True(result);                 // Gissningen lyckades
            Assert.Equal(0, game.Mistakes);      // Inga felökningar
            Assert.Contains('T', game.UsedLetters); // Bokstaven sparas som använd
        }

        [Fact]
        public void Guess_WrongLetter_ShouldReturnFalseAndIncreaseMistakes()
        {
            // Arrange
            var game = new Game();
            game.StartNew("TEST");

            // Act
            var result = game.Guess('A');

            // Assert
            Assert.False(result);                   // Gissningen ska vara fel
            Assert.Equal(1, game.Mistakes);         // Ett misstag ska ha registrerats
            Assert.Contains('A', game.UsedLetters); // Bokstaven ska registreras som använd
        }

        [Fact]
        public void GetMaskedWord_ShouldHideUnrevealedLetters_AndShowCorrectGuesses()
        {
            // Arrange
            var game = new Game();
            game.StartNew("TEST");
            game.Guess('T'); // Bara T är gissad

            // Act
            var masked = game.GetMaskedWord();

            // Assert
            Assert.Equal("T__T", masked); // Visar T, döljer resten
        }

        [Fact]
        public void Guess_SameLetterTwice_ShouldNotIncreaseMistakes()
        {
            // Arrange
            var game = new Game();
            game.StartNew("TEST");

            // Act
            var first = game.Guess('A');  // fel gissning
            var second = game.Guess('A'); // samma felbokstav igen

            // Assert
            Assert.False(first);                 // första var fel
            Assert.False(second);                // andra ska inte plötsligt bli rätt
            Assert.Equal(1, game.Mistakes);      // får INTE öka till 2
            Assert.Contains('A', game.UsedLetters); // 'A' finns i loggen
                                                    // Extra sanity: inga dubbletter i UsedLetters (HashSet i implementationen)
            Assert.Equal(game.UsedLetters.Count, new HashSet<char>(game.UsedLetters).Count);
        }

        [Fact]
        public void Guess_AllDistinctLettersGuessed_ShouldSetStatusWon_AndRaiseGameEnded()
        {
            // Arrange
            var game = new Game();
            game.StartNew("TEST");
            GameStatus? endedWith = null;
            int endedCount = 0;
            game.GameEnded += (_, status) => { endedWith = status; endedCount++; };

            // Act – gissa alla unika bokstäver (T, E, S). T förekommer dubbelt i ordet men gissas bara en gång.
            Assert.True(game.Guess('T')); // rätt
            Assert.True(game.Guess('E')); // rätt
            Assert.True(game.Guess('S')); // rätt → NU ska spelet vara vunnet

            // Assert
            Assert.Equal(GameStatus.Won, game.Status);          // vinst
            Assert.Equal(1, endedCount);                        // GameEnded ska ha triggat exakt en gång
            Assert.Equal(GameStatus.Won, endedWith);            // och med status Won
            Assert.Equal("TEST", game.GetMaskedWord());         // allt synligt
        }

        [Fact]
        public void Guess_ReachingMaxMistakes_ShouldSetStatusLost_AndRaiseGameEnded()
        {
            // Arrange
            var game = new Game(maxMistakes: 2); // lågt för snabb förlust i test
            game.StartNew("TEST");
            GameStatus? endedWith = null;
            int endedCount = 0;
            game.GameEnded += (_, status) => { endedWith = status; endedCount++; };

            // Act – två felgissningar
            Assert.False(game.Guess('A')); // fel
            Assert.False(game.Guess('B')); // fel → NU ska spelet vara förlorat

            // Assert
            Assert.Equal(GameStatus.Lost, game.Status);         // förlust
            Assert.Equal(0, game.AttemptsLeft);                 // inga försök kvar
            Assert.Equal(1, endedCount);                        // GameEnded exakt en gång
            Assert.Equal(GameStatus.Lost, endedWith);           // och med status Lost

            // Extra: efter slut ska vidare gissningar inte ändra något
            var mistakesBefore = game.Mistakes;
            Assert.False(game.Guess('C'));                      // ignoreras
            Assert.Equal(mistakesBefore, game.Mistakes);        // oförändrat
        }

        [Fact]
        public void StartNew_WithEmptyWord_ShouldThrowArgumentException()
        {
            // Arrange
            var game = new Game();

            // Act + Assert
            Assert.Throws<ArgumentException>(() => game.StartNew(""));
        }

        [Fact]
        public void Guess_AfterGameEnded_ShouldNotChangeState()
        {
            // Arrange
            var game = new Game(maxMistakes: 1);
            game.StartNew("TEST");
            game.Guess('A'); // gör ett fel → spelet förlorat

            var mistakesBefore = game.Mistakes;

            // Act – försök gissa efter att spelet är slut
            var result = game.Guess('T');

            // Assert
            Assert.False(result);                 // ingen effekt
            Assert.Equal(mistakesBefore, game.Mistakes); // inget ändras
            Assert.Equal(GameStatus.Lost, game.Status);  // status kvar
        }

        [Fact]
        public void GetMaskedWord_ImmediatelyAfterStart_ShouldReturnOnlyUnderscores()
        {
            // Arrange
            // Skapar nytt spel med ordet TEST, men inga gissningar ännu
            var game = new Game();
            game.StartNew("TEST");

            // Act
            // Hämtar maskerat ord direkt efter start
            var masked = game.GetMaskedWord();

            // Assert
            // Alla bokstäver ska vara dolda eftersom ingen gissning gjorts
            Assert.Equal("____", masked);
        }

        [Fact]
        public void Guess_AllLettersInWordWithDash_ShouldStillWin()
        {
            // Arrange
            // Skapar ett spel där ordet innehåller ett bindestreck
            // (icke-bokstav ska ignoreras vid vinstkontroll)
            var game = new Game();
            game.StartNew("A-B");

            GameStatus? ended = null;
            game.GameEnded += (_, status) => ended = status;

            // Act
            // Gissar på de två riktiga bokstäverna
            game.Guess('A');
            game.Guess('B');

            // Assert
            // När alla bokstäver är gissade ska spelet vara vunnet
            Assert.Equal(GameStatus.Won, game.Status);
            // Och eventet ska ha skickat status Won
            Assert.Equal(GameStatus.Won, ended);
        }

        [Fact]
        public void Guess_SameCorrectLetterTwice_ShouldNotRaiseEventsTwice()
        {
            // Arrange
            // Skapar nytt spel med ordet TEST och prenumererar på eventet LetterGuessed.
            // Testet kontrollerar att eventet inte triggas flera gånger
            // om samma bokstav gissas igen (även med olika bokstavsstorlek).
            var game = new Game();
            game.StartNew("TEST");
            int rightCount = 0;
            game.LetterGuessed += (_, _) => rightCount++;

            // Act
            // Första gissningen (rätt bokstav)
            Assert.True(game.Guess('T'));

            // Andra gissningen – samma bokstav fast i annan form (gemener)
            Assert.True(game.Guess('t'));

            // Assert
            // Eventet ska bara ha triggat en gång
            Assert.Equal(1, rightCount);

            // Inga fel ska ha registrerats
            Assert.Equal(0, game.Mistakes);
        }


        [Fact]
        public void Guess_WrongLetter_RaisesEventOnce_And_DecreasesAttemptsLeft()
        {
            // Arrange
            // Skapar spel med tre tillåtna fel för enklare kontroll.
            // Prenumererar på eventet WrongLetterGuessed för att räkna antal triggers.
            var game = new Game(maxMistakes: 3);
            game.StartNew("TEST");
            int wrongCount = 0;
            game.WrongLetterGuessed += (_, _) => wrongCount++;

            // Sparar antalet försök innan gissningen
            var before = game.AttemptsLeft;

            // Act
            // Första felgissning – ska trigga eventet
            Assert.False(game.Guess('A'));

            // Andra gång samma bokstav – ska INTE trigga eventet igen
            Assert.False(game.Guess('A'));

            // Assert
            // Eventet ska ha triggat exakt en gång
            Assert.Equal(1, wrongCount);

            // Antalet försök ska ha minskat med exakt 1
            Assert.Equal(before - 1, game.AttemptsLeft);

            // Antalet misstag ska vara 1
            Assert.Equal(1, game.Mistakes);
        }


        [Fact]
        public void Guess_IsCaseInsensitive()
        {
            // Arrange
            // Skapar nytt spel där ordet innehåller både små och stora bokstäver.
            // Testet säkerställer att gissningar behandlas oberoende av versaler/gemener.
            var game = new Game();
            game.StartNew("Test");

            // Act
            // Gissar samma bokstäver i olika former (stor och liten)
            var upper = game.Guess('T'); // Versal gissning
            var lower = game.Guess('e'); // Gemener gissning

            // Assert
            // Båda gissningarna ska lyckas oavsett bokstavsstorlek
            Assert.True(upper);
            Assert.True(lower);

            // Både 'T' och 'E' ska finnas bland använda bokstäver (lagras som versaler)
            Assert.Contains('T', game.UsedLetters);
            Assert.Contains('E', game.UsedLetters);

            // Maskerat ord ska visa rätt gissningar i rätt positioner
            Assert.Equal("TE_T", game.GetMaskedWord());
        }
    }
}

