using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TornStockBot.Archive;
using TornStockBot.Common;
using TornStockBot.Common.Interfaces;
using TornStockBot.Data;
using TornStockBot.Logic;

namespace TornStockBot
{
    internal class BotProcessor
    {
        private readonly DateTime _startDateTime = DateTime.Now;

        private readonly Parameters _parameters;
        private readonly Dictionary<int, string> _acronymMap;
        private readonly IFileManagement _stockPriceFileManager;
        private readonly IFileManagement _stockPeriodFileManager;
        private readonly IGenericArchiver<List<StockPrice>> _stockPriceArchiver;
        private readonly IGenericArchiver<List<PeriodStatistics>> _stockPeriodArchiver;
        private readonly ITornAPIReader _tornReader;
        private readonly ITornsyAPIReader _tornsyReader;
        private readonly IStockStatisticsCalc _statisticsCalc;
        private readonly IStockDataManager _dataManager;
        private readonly PeriodHelper _periodHelper;

        public BotProcessor(Parameters parameters)
        {
            _parameters = parameters;
            _acronymMap = AcronymHelper.LoadAcronyms();
            _stockPriceFileManager = new FileManagement(_parameters.BaseFolder, _parameters.StockFileSubfolder, _parameters.StockFileBase, _parameters.StockFileExtension);
            _stockPeriodFileManager = new FileManagement(_parameters.BaseFolder, _parameters.StockPeriodFileSubfolder, _parameters.StockPeriodFileBase, _parameters.StockPeriodFileExtension);
            _stockPriceArchiver = new StockPriceArchiver(_stockPriceFileManager);
            _stockPeriodArchiver = new StockPeriodArchiver(_stockPeriodFileManager);
            _tornReader = new TornAPIReader(_parameters.APIKey, _stockPriceArchiver);
            _tornsyReader = new TornsyAPIReader();
            _dataManager = new StockDataManager(_parameters);
            _statisticsCalc = new StockStatisticsCalc(_parameters, _dataManager);
            _periodHelper = new(_parameters.StockPeriodMinutes);
            _dataManager.PeriodEnded += DataManager_PeriodEnded;
        }

        private void DataManager_PeriodEnded(string period)
        {
            List<PeriodStatistics> statisticsList = new();

            foreach (var pair in _acronymMap)
            {
                string acronym = pair.Value;
                PeriodStatistics statistics = _statisticsCalc.GetPeriodStatistics(period, acronym);
                statisticsList.Add(statistics);
            }

            _stockPeriodArchiver.ArchiveData(_periodHelper.PeriodToTimestamp(period), statisticsList);
        }

        public async Task RunAsync()
        {
            await InitializeRepositoryAsync();

            bool isFirst = true;

            while (true)
            {
                if (!isFirst)
                {
                    Thread.Sleep(MillisecondsTillNextTick());
                }

                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                UpdateConsole(_periodHelper.TimestampToPeriod(timestamp));

                var stockData = await _tornReader.FetchStockPricesAsync(timestamp);

                if (stockData != null)
                {
                    _dataManager.AddStockPrices(stockData);
                }

                isFirst = false;
            }
        }

        private async Task InitializeRepositoryAsync()
        {
            Console.WriteLine("Initializing from tornsy.com");

            foreach (var acronymPair in _acronymMap)
            {
                var acronym = acronymPair.Value;
                var initData = await _tornsyReader.FetchStockSummariesAsync(acronym);

                if (initData != null)
                {
                    _dataManager.AddPeriodSummaries(acronym, initData);
                    Console.WriteLine($"Stock {acronym} loaded: {initData.Count} records");
                }
            }
        }

        private static int MillisecondsTillNextTick()
        {
            long t1 = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long t2 = t1 / 60;
            long t3 = t2 * 60;
            long t4 = t3 + 61;

            return (int)((t4 - t1) * 1000);
        }

        private void UpdateConsole(string period)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.Write($"Last: {DateTime.UtcNow}");

            var diff = DateTime.Now - _startDateTime;
            var uptime = diff.ToString(@"d\:hh\:mm");
            Console.SetCursorPosition(30, 0);
            Console.Write($"Uptime: {uptime}");

            Console.SetCursorPosition(50, 0);
            Console.Write($"Period: {period}");

            Console.WriteLine();
        }
    }
}
