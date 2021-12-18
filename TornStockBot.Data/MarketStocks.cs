using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Data
{
    public class MarketStocks
    {
        public long Timestamp { get; set; } = 0;
        public Dictionary<string, MarketStock> Stocks { get; set; } = new();
    }

    public class MarketStock
    {
        [JsonProperty("stock_id")]
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string Acronym { get; set; } = String.Empty;
        [JsonProperty("current_price")]
        public decimal CurrentPrice { get; set; }
        [JsonProperty("market_cap")]
        public long MarketCap { get; set; }
        [JsonProperty("total_shares")]
        public long TotalShares { get; set; }
        public MarketBenefit Benefit { get; set; } = new();
    }

    public class MarketBenefit
    {
        public string Type { get; set; } = String.Empty;
        public int Frequency { get; set; }
        public long Requirement { get; set; }
        public string Description { get; set; } = String.Empty;
    }
}
