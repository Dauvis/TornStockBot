using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common
{
    public class MovingAverageEventArgs : EventArgs
    {
        public MovingAverageEventArgs(string period, string acronym, CrossingType crossingType, 
            MovingAverageType averageType, MovingAverageLength slowAverage, MovingAverageLength fastAverage)
        {
            Period = period;
            Acronym = acronym;
            CrossingType = crossingType;
            AverageType = averageType;
            SlowAverage = slowAverage;
            FastAverage = fastAverage;
        }

        public string Period { get; }
        public string Acronym { get; }
        public CrossingType CrossingType { get; }
        public MovingAverageType AverageType { get; }
        public MovingAverageLength SlowAverage { get; }
        public MovingAverageLength FastAverage { get; }
    }
}
