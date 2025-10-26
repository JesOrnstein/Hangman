using System;

namespace Hangman.Core
{
    // En enkel word provider som hämtar slumpmässiga ord från en intern lista.
    public sealed class WordProvider : IWordProvider
    {
        // 1. Din ordlista (du kan lägga till / ta bort ord här)
        private static readonly string[] _words =
        {
            "BANAN",
            "DATOR",
            "PROGRAMMERING",
            "KATT",
            "ELEFANT",
            "ROBOTT",
            "KLAVERSPEL",
            "SOCKERDRICKA",
            "VISUALSTUDIO",
            "HÄNGMAN"
        };

        // 2. Random-generator
        private readonly Random _rng = new Random();

        // 3. Returnera ett slumpmässigt ord från listan
        public string GetWord()
        {
            if (_words.Length == 0)
                throw new InvalidOperationException("Word list is empty.");

            int index = _rng.Next(_words.Length); // slumpad index
            return _words[index];
        }

        // 4. Bara lite info (kan användas i UI om du vill visa vilken 'källa' orden kom från)
        public string DifficultyName => "Custom Array";
    }
}