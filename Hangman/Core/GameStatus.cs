

/*
  En enkel enum som håller koll på spelets status:
  - InProgress = spelet pågår
  - Won = spelaren vann
  - Lost = spelaren förlorade
*/

namespace Hangman.Core
{
    public enum GameStatus
    {
        InProgress,
        Won,
        Lost
    }
}