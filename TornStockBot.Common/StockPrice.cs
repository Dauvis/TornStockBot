using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common
{
    public class StockPrice
    {
        public long Timestamp { get; set; }
        public string Acronym { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
