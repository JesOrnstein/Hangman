using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Threading;

/*
  En ordprovider som hämtar ett slumpmässigt ord från ett externt API.
  Använder HttpClient för att göra nätverksanrop.
  Implementerar IAsyncWordProvider eftersom nätverksanrop är asynkrona.
*/

namespace Hangman.Core.Providers
{
    public sealed class ApiWordProvider : IAsyncWordProvider
    {
        // En enda, statisk HttpClient rekommenderas för prestanda och för att undvika socket-utmattning.
        private static readonly HttpClient _httpClient = new();

        // URL till API-endpointen
        private const string ApiUrl = "https://random-word-api.herokuapp.com/word";

        // Svårighetsnamn som kan visas i UI
        public string DifficultyName => "API (Engelska)";

        /*
          Hämtar ett ord asynkront från API:et.
          - Anropar API:et.
          - API:et returnerar en JSON-array, t.ex. ["word"]
          - Deserialiserar svaret och returnerar det första ordet i arrayen.
        */
        public async Task<string> GetWordAsync(CancellationToken ct = default)
        {
            try
            {
                // Gör anropet och deserialisera JSON-svaret (en string-array)
                var words = await _httpClient.GetFromJsonAsync<string[]>(ApiUrl, ct);

                // Kontrollera att vi fick ett giltigt svar
                if (words == null || words.Length == 0 || string.IsNullOrWhiteSpace(words[0]))
                {
                    // Om API:et misslyckas, kasta ett tydligt undantag
                    throw new InvalidOperationException("API:et returnerade ett ogiltigt eller tomt ord.");
                }

                // Returnera det första ordet i arrayen
                return words[0];
            }
            catch (HttpRequestException ex)
            {
                // Hantera nätverksfel eller om API:et är nere
                throw new InvalidOperationException("Kunde inte hämta ord från API:et. Kontrollera nätverksanslutningen.", ex);
            }
            catch (Exception ex)
            {
                // Fånga andra oväntade fel (t.ex. deserialisering, cancellation)
                throw new InvalidOperationException("Ett oväntat fel inträffade vid hämtning av API-ord.", ex);
            }
        }
    }
}
