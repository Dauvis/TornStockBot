﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TornStockBot.Archive;
using TornStockBot.Common;
using TornStockBot.Common.Interfaces;
using TornStockBot.Logic;

namespace TornStockBot.Simulator
{
    internal class SimulationProcessor
    {
        private readonly Parameters _parameters;
        private readonly SimParameters _simParameters;
        private readonly Dictionary<int, string> _acronymMap;
        private readonly IFileManagement _periodManager;
        private readonly IGenericArchiver<List<PeriodStatistics>> _periodArchiver;
        private readonly PeriodHelper _periodHelper;

        private IStockStatisticsCalc _statisticsCalc;
        private IStockDataManager _dataManager;

        public SimulationProcessor(Parameters parameters, SimParameters simParameters)
        {
            _parameters = parameters;
            _simParameters = simParameters;
            _acronymMap = new Dictionary<int, string>() { { 1, _simParameters.TestStockAcronym } };
            _periodHelper = new PeriodHelper(_parameters.StockPeriodMinutes);
            _periodManager = new FileManagement(_parameters.BaseFolder, _parameters.StockPeriodFileSubfolder, _parameters.StockPeriodFileBase, _parameters.StockPeriodFileExtension);
            _periodArchiver = new StockPeriodArchiver(_periodManager);
            _dataManager = new StockDataManager(_parameters);
            _statisticsCalc = new StockStatisticsCalc(_parameters, _dataManager);
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

            _periodArchiver.ArchiveData(_periodHelper.PeriodToTimestamp(period), statisticsList);
        }

        public void RunAsync()
        {

            while (true)
            {
                Console.Write("> ");

                if (!GetCommand(out string cmd, out string[] args))
                {
                    break;
                }

                switch (cmd)
                {
                    case "run":
                        ProcessRunFile(args);
                        break;

                    default:
                        Console.Error.WriteLine($"Invalid command: {cmd}");
                        break;
                }

                Reset();
            }
        }

        private void Reset()
        {
            Thread.Sleep(1000);
            _dataManager.PeriodEnded -= DataManager_PeriodEnded;
            _periodManager.ArchiveCurrentFile(false);
            _dataManager = new StockDataManager(_parameters);
            _statisticsCalc = new StockStatisticsCalc(_parameters, _dataManager);
            _dataManager.PeriodEnded += DataManager_PeriodEnded;
            _periodManager.ArchiveSuffix = "";
        }

        private void ProcessRunFile(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Data file not specified");
                return;
            }

            _periodManager.ArchiveSuffix = args[0];
            GetFilename(args[0], out var initialFilename, out var dataFilename);
            var initialList = SimulationReader.LoadPeriodSummaries(initialFilename);
            var dataList = SimulationReader.LoadStockPrices(dataFilename);

            _dataManager.AddPeriodSummaries(_simParameters.TestStockAcronym, initialList);

            foreach (StockPrice data in dataList)
            {
                _dataManager.AddStockPrices(new List<StockPrice>() { data });
            }
        }

        private void GetFilename(string filenameBase, out string initialFilename, out string dataFilename)
        {
            string runFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _parameters.BaseFolder, 
                _simParameters.SimulationDataFolder, $"{filenameBase}.{_simParameters.SimulationRunExtension}");

            try
            {
                string json = File.ReadAllText(runFilename);
                RunFileFormat? runInfo = JsonConvert.DeserializeObject<RunFileFormat>(json);

                if (runInfo == null)
                {
                    throw new FileLoadException($"Unable to load run file from {runFilename}");
                }

                initialFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _parameters.BaseFolder,
                    _simParameters.SimulationDataFolder, $"{runInfo.Initial}.{_simParameters.SimulationInitExtension}");
                dataFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _parameters.BaseFolder,
                    _simParameters.SimulationDataFolder, $"{runInfo.Data}.{_simParameters.SimulationDataExtension}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw;
            }
        }

        private static bool GetCommand(out string cmd, out string[] args)
        {
            string? line = Console.ReadLine();

            if (line == null)
            {
                cmd = string.Empty;
                args = Array.Empty<string>();

                return true;
            }

            line = line.ToLower();
            var parsed = line.Split(' ');
            cmd = parsed[0];

            if (cmd == "exit")
            {
                cmd = string.Empty;
                args = Array.Empty<string>();

                return false;
            }

            if (parsed.Length > 1)
            {
                args = new string[parsed.Length - 1];
                Array.Copy(parsed, 1, args, 0, parsed.Length - 1);
            }
            else
            {
                args = Array.Empty<string>();
            }

            return true;
        }

        private class RunFileFormat
        {
            public string Initial { get; set; } = string.Empty;
            public string Data { get; set; } = string.Empty;
        }
    }
}
