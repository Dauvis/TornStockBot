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

            CheckForMovingAverageCrossings(period, acronym, signals);

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

        private void CheckForMovingAverageCrossings(string period, string acronym, PeriodSignals signals)
        {
            var curStats = _statsCalc.GetPeriodStatistics(period, acronym);
            var prevStats = _statsCalc.GetPeriodStatistics(_periodHelper.PreviousPeriod(period), acronym);
            var distStats = _statsCalc.GetPeriodStatistics(_periodHelper.PreviousPeriod(period, 2), acronym);

            signals.SMAShortMediumCrossing = DetectMovingAverageCrossing(curStats.SMAShort, curStats.SMAMedium, 
                prevStats.SMAShort, prevStats.SMAMedium, distStats.SMAShort, distStats.SMAMedium);
            signals.SMAShortLongCrossing = DetectMovingAverageCrossing(curStats.SMAShort, curStats.SMALong, 
                prevStats.SMAShort, prevStats.SMALong, distStats.SMAShort, distStats.SMALong);
            signals.SMAMediumLongCrossing = DetectMovingAverageCrossing(curStats.SMAMedium, curStats.SMALong, 
                prevStats.SMAMedium, prevStats.SMALong, distStats.SMAMedium, distStats.SMALong);
            signals.EMAShortMediumCrossing = DetectMovingAverageCrossing(curStats.EMAShort, curStats.EMAMedium, 
                prevStats.EMAShort, prevStats.EMAMedium, distStats.EMAShort, distStats.EMAMedium);
            signals.EMAShortLongCrossing = DetectMovingAverageCrossing(curStats.EMAShort, curStats.EMALong, 
                prevStats.EMAShort, prevStats.EMALong, distStats.EMAShort, distStats.EMALong);
            signals.EMAMediumLongCrossing = DetectMovingAverageCrossing(curStats.EMAMedium, curStats.EMALong, 
                prevStats.EMAMedium, prevStats.EMALong, distStats.EMAMedium, distStats.EMALong);
        }

        private static CrossingType DetectMovingAverageCrossing(decimal curFast, decimal curSlow, 
            decimal prevFast, decimal prevSlow, decimal distFast, decimal distSlow)
        {
            if ((prevFast > prevSlow) && (curSlow > curFast))
            {
                return CrossingType.Death;
            }

            if ((prevSlow > prevFast) && (curFast > curSlow))
            {
                return CrossingType.Gold;
            }

            if (prevSlow == prevFast)
            {
                if ((distFast >= distSlow) && (curSlow > curFast))
                {
                    return CrossingType.Death;
                }

                if ((distSlow >= distFast) && (curFast > curSlow))
                {
                    return CrossingType.Gold;
                }
            }

            return CrossingType.None;
        }
    }
}
