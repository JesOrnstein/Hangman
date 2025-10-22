namespace Hangman.Core;

/*
  En enkel enum som håller koll på spelets status:
  - InProgress = spelet pågår
  - Won = spelaren vann
  - Lost = spelaren förlorade
*/

public enum GameStatus
{
    InProgress,
    Won,
    Lost
}