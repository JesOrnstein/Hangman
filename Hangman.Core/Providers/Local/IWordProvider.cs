using System;

/*
  Ett gränssnitt för att hämta ord till spelet.
  Gör det möjligt att byta ut ordlistan senare (t.ex. svårighetsnivåer).
*/

namespace Hangman.Core.Providers.Local
{
    public interface IWordProvider
    {
        string GetWord();
        string DifficultyName { get; }
    }
}