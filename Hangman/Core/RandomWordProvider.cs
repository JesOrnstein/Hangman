namespace Hangman.Core;

/*
  En enkel ordgenerator som väljer ett slumpmässigt ord
  från en fast lista av svenska ord.
  Använder .NETs Random för att välja ord.
*/

public sealed class RandomWordProvider : IWordProvider
{
    private static readonly string[] Words =
    {
        "BANAN", "PROGRAM", "DATOR", "UTVECKLARE", "ALGORITM", "KOMPILERING",
        "ÅNGEST", "ÄPPLE", "ÖRN", "SYSTEM", "KONSTRUKTION", "FUNKTION",
        "INTERFACE", "TRÅD", "ASYNKRON", "HÄNDELSE", "HÄNGAGUBBE", "TESTNING",
        "LAMBDOR", "LISTA"
    };

    private readonly Random _rng = new();

    public string GetWord() => Words[_rng.Next(Words.Length)];

    public string DifficultyName => "Standard";
}