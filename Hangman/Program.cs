using Hangman.Core;
using Hangman.IO;

/*
  Startar spelet Hänga Gubbe.
  Skapar spelets logik (Game), ordgenerator (RandomWordProvider)
  och konsolgränssnittet (ConsoleUi).
  Sedan körs spelet asynkront med RunAsync().
*/

namespace Hangman;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8; // så ÅÄÖ visas rätt

        var wordProvider = new RandomWordProvider();
        var game = new Game(maxMistakes: 6);
        var ui = new ConsoleUi(game, wordProvider);

        await ui.RunAsync();
    }
}