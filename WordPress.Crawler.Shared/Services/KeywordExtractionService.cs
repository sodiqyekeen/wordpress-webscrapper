using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WordPress.Crawler.Shared.Extensions;
using WordPress.Crawler.Shared.Models;

namespace WordPress.Crawler.Shared.Services
{
    public class KeywordExtractionService
    {
        private readonly HttpClient client;

        public KeywordExtractionService(HttpClient _client)
        {
            client = _client;
        }

        public async Task<IEnumerable<string>> ExtractKeyword(string text)
        {
            var requestPayload = new KeywordExtractionRequest
            {
                data = new List<string> { text.ToPlainText() }
            };
            var response = await client.PostAsync("extract/", new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {

                var extractionResponse = await GetExtractionResponse(response);
                var keywords = extractionResponse.Responses.First().extractions.Select(x => x.parsed_value).ToList();
                return keywords;
            }
            return null;

            static async Task<TagExtractionResponse> GetExtractionResponse(HttpResponseMessage response)
            {
                return JsonSerializer.Deserialize<TagExtractionResponse>(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
