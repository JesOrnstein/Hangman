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
    }
}

