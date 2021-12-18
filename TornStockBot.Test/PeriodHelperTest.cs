using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TornStockBot.Common;
using TornStockBot.Logic;

namespace TornStockBot.Test
{
    internal class PeriodHelperTest
    {
        [TestCase(15, 2021, 12, 11, 22, 30, 0, "12/11/2021 10:30 PM")]
        [TestCase(15, 2021, 12, 11, 22, 30, 1, "12/11/2021 10:30 PM")]
        [TestCase(15, 2021, 12, 11, 22, 30, 2, "12/11/2021 10:30 PM")]
        [TestCase(15, 2021, 12, 11, 22, 30, 5, "12/11/2021 10:30 PM")]
        [TestCase(15, 2021, 12, 11, 22, 30, 10, "12/11/2021 10:30 PM")]
        [TestCase(15, 2021, 12, 11, 22, 30, 30, "12/11/2021 10:30 PM")]
        [TestCase(15, 2021, 12, 11, 22, 30, 59, "12/11/2021 10:30 PM")]
        [TestCase(15, 2021, 12, 11, 22, 31, 0, "12/11/2021 10:45 PM")]
        [TestCase(15, 2021, 12, 11, 22, 31, 1, "12/11/2021 10:45 PM")]
        [TestCase(15, 2021, 12, 11, 22, 31, 5, "12/11/2021 10:45 PM")]
        [TestCase(15, 2021, 12, 11, 22, 31, 10, "12/11/2021 10:45 PM")]
        [TestCase(15, 2021, 12, 11, 22, 31, 30, "12/11/2021 10:45 PM")]
        [TestCase(15, 2021, 12, 11, 22, 35, 0, "12/11/2021 10:45 PM")]
        [TestCase(15, 2021, 12, 11, 22, 40, 0, "12/11/2021 10:45 PM")]
        [TestCase(15, 2021, 12, 11, 22, 44, 59, "12/11/2021 10:45 PM")]
        [TestCase(60, 2021, 12, 11, 22, 0, 0, "12/11/2021 10:00 PM")]
        [TestCase(60, 2021, 12, 11, 22, 0, 1, "12/11/2021 10:00 PM")]
        [TestCase(60, 2021, 12, 11, 22, 0, 2, "12/11/2021 10:00 PM")]
        [TestCase(60, 2021, 12, 11, 22, 0, 5, "12/11/2021 10:00 PM")]
        [TestCase(60, 2021, 12, 11, 22, 0, 10, "12/11/2021 10:00 PM")]
        [TestCase(60, 2021, 12, 11, 22, 0, 30, "12/11/2021 10:00 PM")]
        [TestCase(60, 2021, 12, 11, 22, 0, 59, "12/11/2021 10:00 PM")]
        [TestCase(60, 2021, 12, 11, 22, 1, 0, "12/11/2021 11:00 PM")]
        [TestCase(60, 2021, 12, 11, 22, 15, 0, "12/11/2021 11:00 PM")]
        [TestCase(60, 2021, 12, 11, 22, 30, 0, "12/11/2021 11:00 PM")]
        [TestCase(60, 2021, 12, 11, 22, 45, 0, "12/11/2021 11:00 PM")]
        [TestCase(60, 2021, 12, 11, 22, 59, 59, "12/11/2021 11:00 PM")]
        [TestCase(60, 2021, 12, 11, 23, 0, 0, "12/11/2021 11:00 PM")]
        public void TimestampToPeriodName(int periodMinutes, int year, int month, int day, int hour, int minute, int second, string expectedPeriod)
        {
            // arrange
            PeriodHelper helper = new(periodMinutes);
            DateTimeOffset dateTime = new(year, month, day, hour, minute, second, TimeSpan.Zero);
            long timestamp = dateTime.ToUnixTimeSeconds();

            // act
            string period = helper.TimestampToPeriod(timestamp);

            // assert
            Assert.AreEqual(expectedPeriod, period);
        }
        
        [TestCase(15, "12/11/2021 10:30 PM", 1639261800)]
        [TestCase(15, "12/11/2021 10:45 PM", 1639262700)]
        [TestCase(60, "12/11/2021 10:00 PM", 1639260000)]
        [TestCase(60, "12/11/2021 11:00 PM", 1639263600)]
        public void PeriodNameToTimestamp(int periodMinutes, string period, long expectedTimestamp)
        {
            // arrange
            PeriodHelper helper = new(periodMinutes);

            // act
            long timestamp = helper.PeriodToTimestamp(period);

            // assert
            Assert.AreEqual(expectedTimestamp, timestamp);
        }

        [TestCase(15, 2021, 12, 11, 22, 30, 0, 1639261800)]
        [TestCase(15, 2021, 12, 11, 22, 30, 1, 1639261800)]
        [TestCase(15, 2021, 12, 11, 22, 30, 2, 1639261800)]
        [TestCase(15, 2021, 12, 11, 22, 30, 5, 1639261800)]
        [TestCase(15, 2021, 12, 11, 22, 30, 10, 1639261800)]
        [TestCase(15, 2021, 12, 11, 22, 30, 30, 1639261800)]
        [TestCase(15, 2021, 12, 11, 22, 30, 59, 1639261800)]
        [TestCase(15, 2021, 12, 11, 22, 31, 0, 1639262700)]
        [TestCase(15, 2021, 12, 11, 22, 31, 1, 1639262700)]
        [TestCase(15, 2021, 12, 11, 22, 31, 5, 1639262700)]
        [TestCase(15, 2021, 12, 11, 22, 31, 10, 1639262700)]
        [TestCase(15, 2021, 12, 11, 22, 31, 30, 1639262700)]
        [TestCase(15, 2021, 12, 11, 22, 35, 0, 1639262700)]
        [TestCase(15, 2021, 12, 11, 22, 40, 0, 1639262700)]
        [TestCase(15, 2021, 12, 11, 22, 44, 59, 1639262700)]
        [TestCase(60, 2021, 12, 11, 22, 0, 0, 1639260000)]
        [TestCase(60, 2021, 12, 11, 22, 0, 1, 1639260000)]
        [TestCase(60, 2021, 12, 11, 22, 0, 2, 1639260000)]
        [TestCase(60, 2021, 12, 11, 22, 0, 5, 1639260000)]
        [TestCase(60, 2021, 12, 11, 22, 0, 10, 1639260000)]
        [TestCase(60, 2021, 12, 11, 22, 0, 30, 1639260000)]
        [TestCase(60, 2021, 12, 11, 22, 0, 59, 1639260000)]
        [TestCase(60, 2021, 12, 11, 22, 1, 0, 1639263600)]
        [TestCase(60, 2021, 12, 11, 22, 15, 0, 1639263600)]
        [TestCase(60, 2021, 12, 11, 22, 30, 0, 1639263600)]
        [TestCase(60, 2021, 12, 11, 22, 45, 0, 1639263600)]
        [TestCase(60, 2021, 12, 11, 22, 59, 59, 1639263600)]
        [TestCase(60, 2021, 12, 11, 23, 0, 0, 1639263600)]
        public void TimestampToPeriodTimestamp(int periodMinutes, int year, int month, int day, int hour, int minute, int second, long expectedTimestamp)
        {
            // arrange
            PeriodHelper helper = new(periodMinutes);
            DateTimeOffset dateTime = new(year, month, day, hour, minute, second, TimeSpan.Zero);
            long timestamp = dateTime.ToUnixTimeSeconds();

            // act
            long periodTimestamp = helper.TimestampToPeriodTimestamp(timestamp);

            // assert
            Assert.AreEqual(expectedTimestamp, periodTimestamp);
        }

        [TestCase(15, "12/11/2021 10:30 PM", 1, "12/11/2021 10:45 PM")]
        [TestCase(15, "12/11/2021 10:30 PM", 2, "12/11/2021 11:00 PM")]
        [TestCase(15, "12/11/2021 10:30 PM", 4, "12/11/2021 11:30 PM")]
        [TestCase(15, "12/11/2021 10:30 PM", 8, "12/12/2021 12:30 AM")]
        [TestCase(60, "12/11/2021 10:00 PM", 1, "12/11/2021 11:00 PM")]
        [TestCase(60, "12/11/2021 10:00 PM", 2, "12/12/2021 12:00 AM")]
        [TestCase(60, "12/11/2021 10:00 PM", 4, "12/12/2021 2:00 AM")]
        [TestCase(60, "12/11/2021 10:00 PM", 8, "12/12/2021 6:00 AM")]
        public void NextPeriodTest(int periodMinutes, string startPeriod, int jumpPeriods, string expectedPeriod)
        {
            // arrange
            PeriodHelper helper = new(periodMinutes);

            // act
            string period = helper.NextPeriod(startPeriod, jumpPeriods);

            // assert
            Assert.AreEqual(expectedPeriod, period);
        }

        [TestCase(15, "12/11/2021 10:30 PM", 1, "12/11/2021 10:15 PM")]
        [TestCase(15, "12/11/2021 10:30 PM", 2, "12/11/2021 10:00 PM")]
        [TestCase(15, "12/11/2021 10:30 PM", 4, "12/11/2021 9:30 PM")]
        [TestCase(15, "12/11/2021 10:30 PM", 8, "12/11/2021 8:30 PM")]
        [TestCase(60, "12/11/2021 10:00 PM", 1, "12/11/2021 9:00 PM")]
        [TestCase(60, "12/11/2021 10:00 PM", 2, "12/11/2021 8:00 PM")]
        [TestCase(60, "12/11/2021 10:00 PM", 4, "12/11/2021 6:00 PM")]
        [TestCase(60, "12/11/2021 10:00 PM", 8, "12/11/2021 2:00 PM")]
        public void PreviousPeriodTest(int periodMinutes, string startPeriod, int jumpPeriods, string expectedPeriod)
        {
            // arrange
            PeriodHelper helper = new(periodMinutes);

            // act
            string period = helper.PreviousPeriod(startPeriod, jumpPeriods);

            // assert
            Assert.AreEqual(expectedPeriod, period);
        }

    }
}
