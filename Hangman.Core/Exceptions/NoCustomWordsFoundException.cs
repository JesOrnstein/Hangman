using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Models;

namespace Hangman.Core.Exceptions
{
    /// <summary>
    /// Kastas när en spelrunda försöker hämta ett ord från en anpassad ordlista
    /// som är tom för de valda kriterierna.
    /// </summary>
    public class NoCustomWordsFoundException : InvalidOperationException
    {
        public WordDifficulty Difficulty { get; }
        public WordLanguage Language { get; }

        public NoCustomWordsFoundException(WordDifficulty difficulty, WordLanguage language)
            : base($"No custom words found for {difficulty} ({language}).") // Generiskt engelskt meddelande för loggning
        {
            Difficulty = difficulty;
            Language = language;
        }
    }
}