using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common
{
    public class PeriodHelper
    {
        private readonly int _periodMinutes;
        private readonly int _periodSeconds;

        public PeriodHelper(int periodMinutes)
        {
            _periodSeconds = periodMinutes * 60;
            _periodMinutes = periodMinutes;
        }

        public string PeriodKey(string period, string acronym)
        {
            return $"{acronym}.{period}";
        }

        public string PeriodKey(long timestamp, string acronym)
        {
            string period = TimestampToPeriod(timestamp);

            return PeriodKey(period, acronym);
        }

        public long TimestampToPeriodTimestamp(long timestamp)
        {
            long m = timestamp / 60;
            long p1 = m / _periodMinutes;
            long p2 = m % _periodMinutes;

            return (p1 * _periodSeconds) + (p2 != 0 ? _periodSeconds : 0);
        }

        public string TimestampToPeriod(long timestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(TimestampToPeriodTimestamp(timestamp)).ToString("g");
        }

        public long PeriodToTimestamp(string period)
        {
            var timezone = TimeZoneInfo.Utc;
            var parsedDateLocal = DateTimeOffset.Parse(period);
            var tzOffset = timezone.GetUtcOffset(parsedDateLocal.DateTime);
            var parsedDateTimeZone = new DateTimeOffset(parsedDateLocal.DateTime, tzOffset);

            return parsedDateTimeZone.ToUnixTimeSeconds();
        }

        public string NextPeriod(string period, int periods = 1)
        {
            long curTimestamp = PeriodToTimestamp(period);

            return TimestampToPeriod(curTimestamp + periods * _periodSeconds + 1);
        }

        public string PreviousPeriod(string period, int periods = 1)
        {
            long curTimestamp = PeriodToTimestamp(period);

            return TimestampToPeriod(curTimestamp - periods * _periodSeconds - 1);
        }
    }
}
