using Newtonsoft.Json;
using System.Net.Http.Headers;
using TornStockBot.Common;
using TornStockBot.Common.Interfaces;

namespace TornStockBot.Data
{
    public class TornAPIReader : ITornAPIReader
    {
        private const string STOCKS_API_URI = "https://api.torn.com/torn/?selections=stocks";

        private readonly string _apiKey;
        private readonly IGenericArchiver<List<StockPrice>> _marketArchiver;

        public TornAPIReader(string apiKey, IGenericArchiver<List<StockPrice>> marketArchiver)
        {
            _apiKey = apiKey;
            _marketArchiver = marketArchiver;
        }

        public async Task<List<StockPrice>> FetchStockPricesAsync(long timestamp)
        {
            return await FetchStockPricesAsync(timestamp, true);
        }

        public async Task<List<StockPrice>> FetchStockPricesAsync(long timestamp, bool archive)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(ApiPath(STOCKS_API_URI, _apiKey));

            if (response.IsSuccessStatusCode)
            {
                string stocksJson = await response.Content.ReadAsStringAsync();
                var stocks = JsonConvert.DeserializeObject<MarketStocks>(stocksJson);

                List<StockPrice> result = TranslateToStockPrices(timestamp, stocks);

                if (stocks != null && archive)
                {
                    _marketArchiver.ArchiveData(timestamp, result);
                }
                
                return result;
            }
            else
            {
                throw new HttpRequestException("Internal server error: fetching stocks");
            }
        }

        private List<StockPrice> TranslateToStockPrices(long timestamp, MarketStocks? stocks)
        {
            List<StockPrice> result = new();

            if (stocks != null)
            {
                foreach (var stockPair in stocks.Stocks)
                {
                    var stock = stockPair.Value;

                    result.Add(new StockPrice()
                    {
                        Acronym = stock.Acronym,
                        Price = stock.CurrentPrice,
                        Timestamp = timestamp
                    });
                }
            }

            return result;
        }

        private string ApiPath(string apiUri, string key)
        {
            return $"{apiUri}&key={key}";
        }
    }
}