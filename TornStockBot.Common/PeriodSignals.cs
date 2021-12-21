using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common
{
    public class PeriodSignals
    {
        public string Period { get; set; } = string.Empty;
        public long Timestamp { get; set; }
        public CrossingType SMAShortMediumCrossing { get; set; }
        public CrossingType SMAShortLongCrossing { get; set; }
        public CrossingType SMAMediumLongCrossing  { get; set; }
        public CrossingType EMAShortMediumCrossing { get; set; }
        public CrossingType EMAShortLongCrossing { get; set; }
        public CrossingType EMAMediumLongCrossing { get; set; }
    }
}
