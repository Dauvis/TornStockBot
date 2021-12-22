using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TornStockBot.Common;
using TornStockBot.Common.Interfaces;

namespace TornStockBot.Logic
{
    public class StockSignalManager : IStockSignalManager
    {
        public event EventHandler<MovingAverageEventArgs>? MovingAverageCrossingDetected;

        private readonly IStockStatisticsCalc _statsCalc;
        private readonly PeriodHelper _periodHelper;
        private readonly Dictionary<string, SignalState> _signalStates = new();

        public StockSignalManager(IStockStatisticsCalc statsCalc, PeriodHelper periodHelper)
        {
            _statsCalc = statsCalc;
            _periodHelper = periodHelper;
        }

        public PeriodSignals CheckPeriod(string period, string acronym)
        {
            PeriodSignals signals = new()
            {
                Period = period,
                Timestamp = _periodHelper.PeriodToTimestamp(period)
            };

            if (!_signalStates.ContainsKey(acronym))
            {
                SignalState newState = new();
                newState.InitializeState(period, acronym, _statsCalc, _periodHelper);
                _signalStates[acronym] = newState;
            }

            SignalState state = _signalStates[acronym];

            CheckForMovingAverageCrossings(period, acronym, state, signals);

            RaiseEvents(period, acronym, signals);

            return signals;
        }

        private void RaiseEvents(string period, string acronym, PeriodSignals signals)
        {
            RaiseMovingAverageEvents(period, acronym, signals);
        }

        private void RaiseMovingAverageEvents(string period, string acronym, PeriodSignals signals)
        {
            RaiseMovingAverageCrossingEvent(period, acronym, signals.SMAShortMediumCrossing,
                MovingAverageType.Simple, MovingAverageLength.Short, MovingAverageLength.Medium);
            RaiseMovingAverageCrossingEvent(period, acronym, signals.SMAShortLongCrossing,
                MovingAverageType.Simple, MovingAverageLength.Short, MovingAverageLength.Long);
            RaiseMovingAverageCrossingEvent(period, acronym, signals.SMAMediumLongCrossing,
                MovingAverageType.Simple, MovingAverageLength.Medium, MovingAverageLength.Long);
            RaiseMovingAverageCrossingEvent(period, acronym, signals.EMAShortMediumCrossing,
                MovingAverageType.Exponential, MovingAverageLength.Short, MovingAverageLength.Medium);
            RaiseMovingAverageCrossingEvent(period, acronym, signals.EMAShortLongCrossing,
                MovingAverageType.Exponential, MovingAverageLength.Short, MovingAverageLength.Long);
            RaiseMovingAverageCrossingEvent(period, acronym, signals.EMAMediumLongCrossing,
                MovingAverageType.Exponential, MovingAverageLength.Medium, MovingAverageLength.Long);
        }

        private void RaiseMovingAverageCrossingEvent(string period, string acronym, CrossingType crossType, 
            MovingAverageType avgType, MovingAverageLength fastAvg, MovingAverageLength slowAvg)
        {
            if (crossType == CrossingType.None)
            {
                return;
            }

            var eventArgs = new MovingAverageEventArgs(period, acronym, crossType, avgType, slowAvg, fastAvg);

            MovingAverageCrossingDetected?.Invoke(this, eventArgs);
        }

        private void CheckForMovingAverageCrossings(string period, string acronym, SignalState state, PeriodSignals signals)
        {
            var curStats = _statsCalc.GetPeriodStatistics(period, acronym);

            (signals.SMAShortMediumCrossing, state.PreviousSMAShortMediumDiff) = DetectMovingAverageCrossing(
                curStats.SMAShort, curStats.SMAMedium, state.PreviousSMAShortMediumDiff);
            (signals.SMAShortLongCrossing, state.PreviousSMAShortLongDiff) = DetectMovingAverageCrossing(
                curStats.SMAShort, curStats.SMALong, state.PreviousSMAShortLongDiff);
            (signals.SMAMediumLongCrossing, state.PreviousSMAMediumLongDiff) = DetectMovingAverageCrossing(
                curStats.SMAMedium, curStats.SMALong, state.PreviousSMAMediumLongDiff);
            (signals.EMAShortMediumCrossing, state.PreviousEMAShortMediumDiff) = DetectMovingAverageCrossing(
                curStats.EMAShort, curStats.EMAMedium, state.PreviousEMAShortMediumDiff);
            (signals.EMAShortLongCrossing, state.PreviousEMAShortLongDiff) = DetectMovingAverageCrossing(
                curStats.EMAShort, curStats.EMALong, state.PreviousEMAShortLongDiff);
            (signals.EMAMediumLongCrossing, state.PreviousEMAMediumLongDiff) = DetectMovingAverageCrossing(
                curStats.EMAMedium, curStats.EMALong, state.PreviousEMAMediumLongDiff);
        }

        private static (CrossingType crossingType, decimal newDiff) DetectMovingAverageCrossing(
            decimal curFast, decimal curSlow, decimal previousDiff)
        {
            decimal diff = curFast - curSlow;

            if (diff == 0)
            {
                return (CrossingType.None, previousDiff);
            }

            if (diff > 0 && previousDiff < 0)
            {
                return (CrossingType.Gold, diff);
            }

            if (diff < 0 && previousDiff > 0)
            {
                return (CrossingType.Death, diff);
            }

            return (CrossingType.None, diff);
        }
     
        private class SignalState
        {
            public decimal PreviousSMAShortMediumDiff { get; set; }
            public decimal PreviousSMAShortLongDiff { get; set; }
            public decimal PreviousSMAMediumLongDiff { get; set; }
            public decimal PreviousEMAShortMediumDiff { get; set; }
            public decimal PreviousEMAShortLongDiff { get; set; }
            public decimal PreviousEMAMediumLongDiff { get; set; }

            internal void InitializeState(string period, string acronym, IStockStatisticsCalc statsCalc, PeriodHelper periodHelper)
            {
                string p = period;
                int counter = 0;
                while (counter < 6)
                {
                    p = periodHelper.PreviousPeriod(p);
                    PeriodStatistics stats = statsCalc.GetPeriodStatistics(p, acronym);

                    if (PreviousSMAShortMediumDiff == 0 && stats.SMAShort != stats.SMAMedium)
                    {
                        PreviousSMAShortMediumDiff = stats.SMAShort - stats.SMAMedium;
                        counter++;
                    }

                    if (PreviousSMAShortLongDiff == 0 && stats.SMAShort != stats.SMALong)
                    {
                        PreviousSMAShortLongDiff = stats.SMAShort - stats.SMALong;
                        counter++;
                    }

                    if (PreviousSMAMediumLongDiff == 0 && stats.SMAMedium != stats.SMALong)
                    {
                        PreviousSMAMediumLongDiff = stats.SMAMedium - stats.SMALong;
                        counter++;
                    }

                    if (PreviousEMAShortMediumDiff == 0 && stats.EMAShort != stats.EMAMedium)
                    {
                        PreviousEMAShortMediumDiff = stats.EMAShort - stats.EMAMedium;
                        counter++;
                    }

                    if (PreviousEMAShortLongDiff == 0 && stats.EMAShort != stats.EMALong)
                    {
                        PreviousEMAShortLongDiff = stats.EMAShort - stats.EMALong;
                        counter++;
                    }

                    if (PreviousEMAMediumLongDiff == 0 && stats.EMAMedium != stats.EMALong)
                    {
                        PreviousEMAMediumLongDiff = stats.EMAMedium - stats.EMALong;
                        counter++;
                    }
                }
            }
        }
    }
}
