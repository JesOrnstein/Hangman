using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Core.Providers.Api
{
    // Enkel enum för att definiera svårighetsgraderna
    // Denna kan återanvändas av andra providers senare.
    public enum ApiDifficulty
    {
        Easy,
        Medium,
        Hard
    }
}
