using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TornStockBot.Common;
using TornStockBot.Common.Interfaces;

namespace TornStockBot.Data
{
    public class TornsyAPIReader : ITornsyAPIReader
    {
        private const string API_URI_BASE = "https://tornsy.com/api/";
        private const string API_URI_OPTIONS = "?interval=m15";

        public async Task<List<PeriodSummary>?> FetchStockSummariesAsync(string acronym)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync($"{API_URI_BASE}{acronym}{API_URI_OPTIONS}");

            if (response.IsSuccessStatusCode)
            {
                List<PeriodSummary> result = new();
                string stocksJson = await response.Content.ReadAsStringAsync();
                var stocks = JsonConvert.DeserializeObject<BaseItemList>(stocksJson);

                if (stocks == null)
                {
                    return null;
                }

                foreach (var entry in stocks.Data)
                {
                    var item = new PeriodSummary()
                    {
                        Timestamp = decimal.ToInt64(entry[0]),
                        Open = entry[1],
                        High = entry[2],
                        Low = entry[3],
                        Close = entry[4]
                    };

                    result.Add(item);
                }

                return result;
            }
            else
            {
                Console.WriteLine("Internal server error: fetching Tornsy stock data");

                return null;
            }
        }

        private class BaseItemList
        {
            public List<List<decimal>> Data { get; set; } = new();
        }
    }
}
