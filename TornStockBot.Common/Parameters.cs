using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common
{
    public class Parameters
    {
        public string APIKey { get; set; } = string.Empty;
        public string BaseFolder { get; set; } = string.Empty;
        public string StockFileSubfolder { get; set; } = string.Empty;
        public string StockFileBase { get; set; } = string.Empty;
        public string StockFileExtension { get; set; } = string.Empty;
        public string StockPeriodFileSubfolder { get; set; } = string.Empty;
        public string StockPeriodFileBase { get; set; } = string.Empty;
        public string StockPeriodFileExtension { get; set; } = string.Empty;
        public int StockPeriodMinutes { get; set; }
        public int PeriodStatisticsMaxCalculationPeriods { get; set; }
        public int SMAShortPeriods { get; set; }
        public int SMAMediumPeriods { get; set; }
        public int SMALongPeriods { get; set; }
        public int EMAShortPeriods { get; set; }
        public int EMAMediumPeriods { get; set; }
        public int EMALongPeriods { get; set; }
        public int StochasticPeriods { get; set; }
        public int RateOfChangePeriods { get; set; }
        public int MACDShortPeriods { get; set; }
        public int MACDLongPeriods { get; set; }
        public int MACDMovingAveragePeriods { get; set; }
        public int RSIPeriods { get; set; }

        public static Parameters Load(bool isTest = false)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, isTest ? "parameterstest.json" : "parameters.json");
                string json = File.ReadAllText(path);
                Parameters? parameters = JsonConvert.DeserializeObject<Parameters>(json);

                if (parameters == null)
                {
                    throw new FileLoadException($"Unable to load parameters from {path}");
                }

                return parameters;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
