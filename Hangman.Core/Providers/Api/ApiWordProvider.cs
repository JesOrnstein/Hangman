using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Threading;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Models;

namespace Hangman.Core.Providers.Api
{
    public sealed class ApiWordProvider : IAsyncWordProvider
    {
        private static readonly HttpClient _httpClient = new();
        private const string ApiUrl = "https://random-word-api.herokuapp.com/word";

        private readonly WordDifficulty _difficulty;
        private readonly Random _rng = new();

        public ApiWordProvider(WordDifficulty difficulty)
        {
            _difficulty = difficulty;
        }

        public string DifficultyName => $"API ({_difficulty})";

        public async Task<string> GetWordAsync(CancellationToken ct = default)
        {
            int minLength, maxLength;

            switch (_difficulty)
            {
                case WordDifficulty.Easy:
                    minLength = 3;
                    maxLength = 4;
                    break;

                case WordDifficulty.Medium:
                    minLength = 5;
                    maxLength = 7;
                    break;

                case WordDifficulty.Hard:
                    minLength = 8;
                    maxLength = 11;
                    break;

                default:
                    minLength = 5;
                    maxLength = 5;
                    break;
            }

            int length = _rng.Next(minLength, maxLength + 1);

            string requestUrl = $"{ApiUrl}?length={length}";

            try
            {
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