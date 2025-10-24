

/*
  En enkel ordgenerator som väljer ett slumpmässigt ord
  från en fast lista av svenska ord.
  Använder .NETs Random för att välja ord.
*/

namespace Hangman.Core
{
    public sealed class RandomWordProvider : IWordProvider
    {
        // Liten lista för att hålla det minimalt
        private static readonly string[] Words = { "TEST", "BANAN", "DATOR" };
        private readonly Random _rng = new();

        public string GetWord() => Words[_rng.Next(Words.Length)];
        public string DifficultyName => "Standard";
    }
}