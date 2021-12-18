using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common.Interfaces
{
    public interface ITornsyAPIReader
    {
        public Task<List<PeriodSummary>?> FetchStockSummariesAsync(string acronym);
    }
}
