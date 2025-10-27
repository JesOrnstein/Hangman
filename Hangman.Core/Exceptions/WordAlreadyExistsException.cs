using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Models;

namespace Hangman.Core.Exceptions
{
    /// <summary>
    /// Kastas när ett försök görs att lägga till ett ord som redan finns 
    /// i databasen med samma svårighetsgrad och språk.
    /// </summary>
    public class WordAlreadyExistsException : InvalidOperationException
    {
        public string Word { get; }
        public WordDifficulty Difficulty { get; }
        public WordLanguage Language { get; }

        public WordAlreadyExistsException(string word, WordDifficulty difficulty, WordLanguage language)
            : base($"Word '{word}' already exists for {difficulty} ({language}).") // Generiskt engelskt meddelande för loggning
        {
            Word = word;
            Difficulty = difficulty;
            Language = language;
        }
    }
}