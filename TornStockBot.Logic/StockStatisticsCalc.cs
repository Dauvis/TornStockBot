using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TornStockBot.Common;
using TornStockBot.Common.Interfaces;

namespace TornStockBot.Logic
{
    public class StockStatisticsCalc : IStockStatisticsCalc
    {
        private readonly Dictionary<string, PeriodStatistics> _periodCache = new();
        private readonly IStockDataManager _dataManager;
        private readonly PeriodHelper _periodHelper;
        private readonly Parameters _parameters;

        private string _currentPeriod = string.Empty;

        public StockStatisticsCalc(Parameters parameters, IStockDataManager dataManager)
        {
            _parameters = parameters;
            _dataManager = dataManager;
            _periodHelper = new(_parameters.StockPeriodMinutes);
        }

        public PeriodStatistics GetPeriodStatistics(string period, string acronym)
        {
            string key = _periodHelper.PeriodKey(period, acronym);

            if (_periodCache.ContainsKey(key))
            {
                return _periodCache[key];
            }

            PeriodStatistics stockPeriod = new()
            {
                Acronym = acronym,
                Period = period,
                Timestamp = _periodHelper.PeriodToTimestamp(period)
            };

            PeriodSummary summary = _dataManager.GetPeriodSummary(period, acronym);
            stockPeriod.ApplySummary(summary);

            var periodClosings = ClosingValuesForLastPeriods(period, acronym, _parameters.PeriodStatisticsMaxCalculationPeriods);
            stockPeriod.SMAShort = CalculateSimpleMovingAverage(_parameters.SMAShortPeriods, periodClosings);
            stockPeriod.SMAMedium = CalculateSimpleMovingAverage(_parameters.SMAMediumPeriods, periodClosings);
            stockPeriod.SMALong = CalculateSimpleMovingAverage(_parameters.SMALongPeriods, periodClosings);
            stockPeriod.EMAShort = CalculateExponentialMovingAverage(_parameters.EMAShortPeriods, periodClosings);
            stockPeriod.EMAMedium = CalculateExponentialMovingAverage(_parameters.EMAMediumPeriods, periodClosings);
            stockPeriod.EMALong = CalculateExponentialMovingAverage(_parameters.EMALongPeriods, periodClosings);
            stockPeriod.RSI = CalculateRelativeStrengthIndex(periodClosings);
            stockPeriod.Stoch = CalculateStochasticOscillator(period, acronym, _parameters.StochasticPeriods);
            stockPeriod.RateOfChange = CalculateRateOfChange(periodClosings, _parameters.RateOfChangePeriods);
            
            CalculateBollingerBands(periodClosings, stockPeriod.SMAShort, out var bollUpper, out var bollLower);
            stockPeriod.BollLower = bollLower;
            stockPeriod.BollUpper = bollUpper;

            CalculateMovingAverageConvergenceDivergence(periodClosings, out var macd, out var macdHist);
            stockPeriod.MACD = macd;
            stockPeriod.MACDHist = macdHist;

            _periodCache.Add(key, stockPeriod);

            return stockPeriod;
        }

        private static decimal CalculateExponentialMovingAverage(int periods, List<decimal> periodClosings)
        {
            int count = Math.Min(periods, periodClosings.Count);

            if (count == 0)
            {
                return 0;
            }

            decimal weight = 2m / (count + 1);
            decimal ema = periodClosings[count - 1];

            for (int i = count - 2; i >= 0; i--)
            {
                decimal t1 = weight * periodClosings[i];
                decimal t2 = (1 - weight) * ema;
                ema = t1 + t2;
            }

            return decimal.Round(ema, 2);
        }

        private static decimal CalculateSimpleMovingAverage(int periods, List<decimal> periodClosings)
        {
            decimal total = 0;
            int count = Math.Min(periods, periodClosings.Count);

            if (count == 0)
            {
                return 0;
            }

            for (int i = 0; i < count; i++)
            {
                total += periodClosings[i];
            }

            return decimal.Round(total / count, 2);
        }

        private List<decimal> ClosingValuesForLastPeriods(string period, string acronym, int periods)
        {
            List<decimal> periodClosings = new();
            string p = period;

            for (int i = 0; i < periods; i++)
            {
                PeriodSummary summary = _dataManager.GetPeriodSummary(p, acronym);

                if (summary != null && summary.Close != 0)
                {
                    periodClosings.Add(summary.Close);
                }

                p = _periodHelper.PreviousPeriod(p);
            }

            return periodClosings;
        }

        private void CalculateMovingAverageConvergenceDivergence(List<decimal> periodClosings, out decimal macd, out decimal macdHist)
        {
            decimal emaShort = CalculateExponentialMovingAverage(_parameters.MACDShortPeriods, periodClosings);
            decimal emaLong = CalculateExponentialMovingAverage(_parameters.MACDLongPeriods, periodClosings);
            macd = emaShort - emaLong;

            List<decimal> previousMacd = new() { macd };

            for (int i = 1; i < _parameters.MACDMovingAveragePeriods; i++)
            {
                List<decimal> subset = periodClosings.Skip(i).Take(_parameters.MACDLongPeriods).ToList();
                decimal eShort = CalculateExponentialMovingAverage(_parameters.MACDShortPeriods, subset);
                decimal eLong = CalculateExponentialMovingAverage(_parameters.MACDLongPeriods, subset);

                previousMacd.Add(eShort - eLong);
            }

            decimal signal = CalculateExponentialMovingAverage(_parameters.MACDMovingAveragePeriods, previousMacd);
            macdHist = macd - signal;
        }

        private void CalculateBollingerBands(List<decimal> periodClosings, decimal smaShort, out decimal bollUpper, out decimal bollLower)
        {
            List<decimal> closePrices;

            if (periodClosings.Count > _parameters.SMAShortPeriods)
            {
                closePrices = periodClosings.Take(_parameters.SMAShortPeriods).ToList();
            }
            else
            {
                closePrices = periodClosings;
            }

            decimal stdDev = CalculateStandardDeviation(smaShort, closePrices);

            bollLower = decimal.Round(smaShort - 2 * stdDev, 2);
            bollUpper = decimal.Round(smaShort + 2 * stdDev, 2);
        }

        private decimal CalculateRelativeStrengthIndex(List<decimal> periodClosings)
        {
            List<decimal> upMoves = new();
            List<decimal> downMoves = new();

            for (int i = 1; i <= _parameters.RSIPeriods; i++)
            {
                decimal previousPeriod = periodClosings[i - 1];
                decimal currentPeriod = periodClosings[i];

                if (currentPeriod > previousPeriod)
                {
                    upMoves.Add(currentPeriod - previousPeriod);
                    downMoves.Add(decimal.Zero);
                }
                else if (previousPeriod > currentPeriod)
                {
                    upMoves.Add(decimal.Zero);
                    downMoves.Add(previousPeriod - currentPeriod);
                }
                else
                {
                    upMoves.Add(decimal.Zero);
                    downMoves.Add(decimal.Zero);
                }
            }

            decimal upAverage = CalculateExponentialMovingAverage(_parameters.RSIPeriods, upMoves);
            decimal downAverage = CalculateExponentialMovingAverage(_parameters.RSIPeriods, downMoves);
            decimal relativeStrength = downAverage != 0 ? upAverage / downAverage : decimal.MaxValue;
            decimal rsi = 100 - 100 / (1 + relativeStrength);

            return decimal.Round(rsi, 2);
        }

        private static decimal CalculateStandardDeviation(decimal mean, List<decimal> observations)
        {
            int count = 0;
            double sumDevSqr = 0;

            foreach (var close in observations)
            {
                count++;
                sumDevSqr += Math.Pow(decimal.ToDouble(close - mean), 2);
            }

            double stdDev = Math.Sqrt(sumDevSqr / count);

            return decimal.Round((decimal)stdDev, 2);
        }

        private decimal CalculateStochasticOscillator(string period, string acronym, int periods)
        {
            decimal curClose = _dataManager.GetPeriodSummary(period, acronym).Close;
            decimal highest = 0;
            decimal lowest = 0;

            for (int i = 0; i < periods; i++)
            {
                string p = _periodHelper.PreviousPeriod(period, i);
                PeriodSummary prevSummary = _dataManager.GetPeriodSummary(p, acronym);

                highest = (prevSummary.High > highest) ? prevSummary.High : highest;
                lowest = (lowest == 0 || prevSummary.Low < lowest) ? prevSummary.Low : lowest;
            }

            decimal k = 0;

            if (highest != lowest)
            {
                k = 100 * ((curClose - lowest) / (highest - lowest));
            }

            return decimal.Round(k, 2);
        }

        private static decimal CalculateRateOfChange(List<decimal> periodClosings, int periods)
        {
            if (periodClosings.Count == 0)
            {
                return 0;
            }

            decimal pc = periodClosings[0];
            decimal pn = periodClosings[Math.Min(periods - 1, periodClosings.Count - 1)];

            return decimal.Round(100*((pc - pn) / pn), 2);
        }

    }
}
