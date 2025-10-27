using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Core.Models
{
    /// Definierar de standardiserade svårighetsgraderna för ordhämtning.
    /// Används av alla WordProviders (API och Lokal) för enhetlighet.
    public enum WordDifficulty
    {
        Easy,
        Medium,
        Hard
    }
}
