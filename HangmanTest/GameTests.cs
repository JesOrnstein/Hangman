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
            Assert.Equal(GameStatus.InProgress, game.Status);
            Assert.Equal("TEST", game.Secret);
            Assert.Equal(0, game.Mistakes);
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
    }
}

