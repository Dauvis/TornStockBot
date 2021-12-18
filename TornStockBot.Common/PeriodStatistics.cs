using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common
{
    public class PeriodStatistics
    {
        public string Period { get; set; } = string.Empty;
        public long Timestamp { get; set; }
        public string Acronym { get; set; } = string.Empty;
        public decimal OpenPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public decimal SMAShort { get; set; }
        public decimal SMAMedium { get; set; }
        public decimal SMALong { get; set; }
        public decimal BollUpper { get; set; }
        public decimal BollLower { get; set; }
        public decimal EMAShort { get; set; }
        public decimal EMAMedium { get; set; }
        public decimal EMALong { get; set; }
        public decimal MACD { get; set; }
        public decimal MACDHist { get; set; }
        public decimal RSI { get; set; }
        public decimal Stoch { get; set; }
        public decimal RateOfChange { get; set; }

        public void ApplySummary(PeriodSummary summary)
        {
            OpenPrice = summary.Open;
            ClosePrice = summary.Close;
            HighPrice = summary.High;
            LowPrice = summary.Low;
        }

    }
}
