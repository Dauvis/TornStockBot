using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TornStockBot.Common;
using TornStockBot.Common.Interfaces;

namespace TornStockBot.Simulator
{
    internal class SimulationReader
    {
        public static List<PeriodSummary> LoadPeriodSummaries(string filename)
        {
            List<PeriodSummary> records = new();

            try
            {
                using var reader = new StreamReader(filename);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var loaded = csv.GetRecords<PeriodSummary>();

                foreach (var item in loaded)
                {
                    records.Add(item);
                }
            }
            catch (Exception ex)
            {
                throw new FileLoadException($"Failed to load period summaries file - {ex.Message}");
            }

            return records;
        }

        public static List<StockPrice> LoadStockPrices(string filename)
        {
            List<StockPrice> records = new();

            try
            {
                using var reader = new StreamReader(filename);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var loaded = csv.GetRecords<StockPrice>();

                foreach (var item in loaded)
                {
                    records.Add(item);
                }
            }
            catch(Exception ex)
            {
                throw new FileLoadException($"Failed to load stock prices file - {ex.Message}");
            }

            return records;
        }
    }
}
