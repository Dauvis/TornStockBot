using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common.Interfaces
{
    public delegate void PeriodEndedDelegate(string period);

    public interface IStockDataManager
    {
        public event PeriodEndedDelegate? PeriodEnded;
        public void AddStockPrices(List<StockPrice> stockPrices);
        public void AddPeriodSummaries(string acronym, List<PeriodSummary> summaries);
        public PeriodSummary GetPeriodSummary(string period, string acronym);
    }
}
