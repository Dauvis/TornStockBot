using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common.Interfaces
{
    public interface IStockStatisticsCalc
    {
        public PeriodStatistics GetPeriodStatistics(string period, string acronym);
    }
}
