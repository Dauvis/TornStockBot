using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TornStockBot.Common;
using TornStockBot.Common.Interfaces;

namespace TornStockBot.Logic
{
    public class StockDataManager : IStockDataManager
    {
        public event PeriodEndedDelegate? PeriodEnded;

        private readonly Dictionary<string, List<StockPrice>> _periodPrices = new();
        private readonly Dictionary<string, PeriodSummary> _summaryCache = new();
        private readonly PeriodHelper _periodHelper;
        private readonly Parameters _parameters;

        private string _currentPeriod = string.Empty;

        public StockDataManager(Parameters parameters)
        {
            _parameters = parameters;
            _periodHelper = new(_parameters.StockPeriodMinutes); ;
        }

        public void AddStockPrices(List<StockPrice> stockPrices)
        {
            if (stockPrices == null || stockPrices.Count == 0)
            {
                return;
            }

            string period = _periodHelper.TimestampToPeriod(stockPrices[0].Timestamp);

            if (_currentPeriod == string.Empty)
            {
                _currentPeriod = period;
            }

            bool periodEnded = false;

            foreach (var stockPrice in stockPrices)
            {
                if (!_periodPrices.ContainsKey(stockPrice.Acronym))
                {
                    _periodPrices[stockPrice.Acronym] = new List<StockPrice>();
                }

                if (_currentPeriod != period)
                {
                    periodEnded = true;
                    // add to summary cache before clearing data
                    GetPeriodSummary(_currentPeriod, stockPrice.Acronym);   
                    _periodPrices[stockPrice.Acronym] = new List<StockPrice>();
                }
                else
                {
                    string key = _periodHelper.PeriodKey(period, stockPrice.Acronym);
                    _summaryCache.Remove(key);
                }

                _periodPrices[stockPrice.Acronym].Add(stockPrice);
            }

            if (periodEnded)
            {
                PeriodEnded?.Invoke(_currentPeriod);
                _currentPeriod = period;
            }
        }

        public PeriodSummary GetPeriodSummary(string period, string acronym)
        {
            string key = _periodHelper.PeriodKey(period, acronym);

            if (_summaryCache.ContainsKey(key))
            {
                return _summaryCache[key];
            }

            PeriodSummary summary = new();

            decimal sum = 0;
            int count = 0;

            foreach (var item in _periodPrices[acronym])
            {
                summary.Close = item.Price;

                if (summary.Open == decimal.Zero)
                {
                    summary.Open = item.Price;
                }

                if (summary.High < item.Price)
                {
                    summary.High = item.Price;
                }

                if (summary.Low == decimal.Zero || summary.Low > item.Price)
                {
                    summary.Low = item.Price;
                }

                sum += item.Price;
                count++;
            }

            if (count > 0)
            {
                _summaryCache[key] = summary;
            }

            return summary;
        }

        public void AddPeriodSummaries(string acronym, List<PeriodSummary> summaries)
        {
            foreach (PeriodSummary summary in summaries)
            {
                _summaryCache.Add(_periodHelper.PeriodKey(summary.Timestamp, acronym), summary);
            }
        }
    }
}
