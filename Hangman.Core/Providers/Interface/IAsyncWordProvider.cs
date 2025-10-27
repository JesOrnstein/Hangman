using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
  Ett gränssnitt för att hämta ord till spelet asynkront.
  Används för källor som kräver nätverksanrop eller fil-IO.
*/

namespace Hangman.Core.Providers.Interface
{
    public interface IAsyncWordProvider
    {
        // Metoden returnerar nu en Task<string> för asynkron hantering
        Task<string> GetWordAsync(CancellationToken ct = default);

        string DifficultyName { get; }
    }
}
