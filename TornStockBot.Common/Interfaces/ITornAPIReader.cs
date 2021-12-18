using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common.Interfaces
{
    public interface ITornAPIReader
    {
        public Task<List<StockPrice>> FetchStockPricesAsync(long timestamp);
        public Task<List<StockPrice>> FetchStockPricesAsync(long timestamp, bool archive);
    }
}
