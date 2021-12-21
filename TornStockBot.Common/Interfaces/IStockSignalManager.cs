using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common.Interfaces
{

    public interface IStockSignalManager
    {
        public event EventHandler<MovingAverageEventArgs>? MovingAverageCrossingDetected;

        public PeriodSignals CheckPeriod(string period, string acronym);
    }
}
