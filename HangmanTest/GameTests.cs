using Hangman.Core;
using Xunit;

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
    }
}

