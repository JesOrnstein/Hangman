namespace Hangman.Core;

/*
  Ett gränssnitt för att hämta ord till spelet.
  Gör det möjligt att byta ut ordlistan senare (t.ex. svårighetsnivåer).
*/

public interface IWordProvider
{
    string GetWord();
    string DifficultyName { get; }
}