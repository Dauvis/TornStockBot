using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Simulator
{
    internal class SimParameters
    {
        public int TestStockId { get; set; }
        public string TestStockAcronym { get; set; } = string.Empty;
        public string SimulationDataFolder { get; set; } = string.Empty;
        public string SimulationRunExtension { get; set; } = string.Empty;
        public string SimulationInitExtension { get; set; } = string.Empty;
        public string SimulationDataExtension { get; set; } = string.Empty;

        public static SimParameters Load()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "simparameters.json");
                string json = File.ReadAllText(path);
                SimParameters? parameters = JsonConvert.DeserializeObject<SimParameters>(json);

                if (parameters == null)
                {
                    throw new FileLoadException($"Unable to load simulation parameters from {path}");
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
