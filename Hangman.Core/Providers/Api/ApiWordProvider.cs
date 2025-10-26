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
  Den kan konfigureras med en svårighetsgrad (ApiDifficulty)
  för att hämta ord av olika längd.
*/

namespace Hangman.Core.Providers.Api
{
    public sealed class ApiWordProvider : IAsyncWordProvider
    {
        private static readonly HttpClient _httpClient = new();
        private const string ApiUrl = "https://random-word-api.herokuapp.com/word";

        // Spara svårighetsgraden som valdes när klassen skapades
        private readonly ApiDifficulty _difficulty;

        // Vi behöver en Random-generator för att välja längd inom ett spann
        private readonly Random _rng = new();

        // Konstruktorn tar nu emot en svårighetsgrad
        public ApiWordProvider(ApiDifficulty difficulty)
        {
            _difficulty = difficulty;
        }

        public ApiWordProvider()
        {
        }

        // DifficultyName kan nu reflektera det valda läget
        public string DifficultyName => $"API ({_difficulty})";

        public async Task<string> GetWordAsync(CancellationToken ct = default)
        {
            // 1. Bestäm ordlängd baserat på svårighetsgrad
            int minLength, maxLength;

            switch (_difficulty)
            {
                case ApiDifficulty.Easy:
                    minLength = 3; // 1-2 bokstäver är inte så kul i hänga gubbe
                    maxLength = 4;
                    break;

                case ApiDifficulty.Medium:
                    minLength = 5;
                    maxLength = 7;
                    break;

                case ApiDifficulty.Hard:
                    minLength = 8;
                    maxLength = 11; // Sätt en övre gräns
                    break;

                default: // Fallback
                    minLength = 5;
                    maxLength = 5;
                    break;
            }

            // 2. Välj en slumpmässig längd inom spannet
            // (Max-värdet i Next() är exklusivt, därav +1)
            int length = _rng.Next(minLength, maxLength + 1);

            // 3. Bygg den nya URL:en med query-parametern
            string requestUrl = $"{ApiUrl}?length={length}";

            try
            {
                // 4. Gör anropet med den nya URL:en
                var words = await _httpClient.GetFromJsonAsync<string[]>(requestUrl, ct);

                if (words == null || words.Length == 0 || string.IsNullOrWhiteSpace(words[0]))
                {
                    throw new InvalidOperationException("API:et returnerade ett ogiltigt eller tomt ord.");
                }

                return words[0];
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Kunde inte hämta ord (längd {length}) från API:et.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ett oväntat fel inträffade vid hämtning av API-ord.", ex);
            }
        }
    }
}
