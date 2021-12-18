using TornStockBot.Common;
using TornStockBot.Simulator;

try
{
    Parameters parameters = Parameters.Load();
    SimParameters simParameters = SimParameters.Load();
    SimulationProcessor processor = new(parameters, simParameters);
    processor.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.ToString());
    Console.ReadKey();
}