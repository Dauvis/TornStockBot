using TornStockBot;
using TornStockBot.Common;

try
{
    bool isTest = false;

    if (args.Length == 1 && args[0] == "test")
    {
        Console.WriteLine("Executing in test mode");
        isTest = true;
    }

    var parameters = Parameters.Load(isTest);
    var processor = new BotProcessor(parameters);
    await processor.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    Console.ReadKey();
}