using Newtonsoft.Json;
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
        private readonly IFileManagement _periodFileManager;
        private readonly IFileManagement _signaFileManager;
        private readonly IGenericArchiver<List<PeriodStatistics>> _periodArchiver;
        private readonly IGenericArchiver<List<PeriodSignals>> _signalArchiver;
        private readonly PeriodHelper _periodHelper;

        private IStockStatisticsCalc? _statisticsCalc;
        private IStockDataManager? _dataManager;
        private IStockSignalManager? _signalManager;

        public SimulationProcessor(Parameters parameters, SimParameters simParameters)
        {
            _parameters = parameters;
            _simParameters = simParameters;
            _acronymMap = new Dictionary<int, string>() { { 1, _simParameters.TestStockAcronym } };
            _periodHelper = new PeriodHelper(_parameters.StockPeriodMinutes);
            _periodFileManager = new FileManagement(_parameters.BaseFolder, _parameters.StockPeriodFileSubfolder, 
                _parameters.StockPeriodFileBase, _parameters.StockPeriodFileExtension);
            _signaFileManager = new FileManagement(_parameters.BaseFolder, _parameters.StockSignalFileSubfolder,
                _parameters.StockSignalFileBase, _parameters.StockSignalFileExtension);
            _periodArchiver = new StockPeriodArchiver(_periodFileManager);
            _signalArchiver = new SignalArchiver(_signaFileManager);
        }

        private void SignalManager_MovingAverageCrossingDetected(object? sender, MovingAverageEventArgs e)
        {
            Console.WriteLine($"{e.Period} - {e.Acronym}: Moving average cross detected: {e.AverageType} {e.FastAverage}/{e.SlowAverage} ({e.CrossingType})");
        }

        private void DataManager_PeriodEnded(string period)
        {
            if (_statisticsCalc == null || _signalManager == null)
            {
                return;
            }

            List<PeriodStatistics> statisticsList = new();
            List<PeriodSignals> signalsList = new();

            foreach (var pair in _acronymMap)
            {
                string acronym = pair.Value;
                PeriodStatistics statistics = _statisticsCalc.GetPeriodStatistics(period, acronym);
                statisticsList.Add(statistics);

                PeriodSignals signals = _signalManager.CheckPeriod(period, acronym);
                signalsList.Add(signals);
            }

            _periodArchiver.ArchiveData(_periodHelper.PeriodToTimestamp(period), statisticsList);
            _signalArchiver.ArchiveData(_periodHelper.PeriodToTimestamp(period), signalsList);
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
            }
        }

        private void SetupExecutionEnvironment()
        {
            // force files to archive
            _periodFileManager.ArchiveCurrentFile(false);
            _signaFileManager.ArchiveCurrentFile(false);

            _dataManager = new StockDataManager(_parameters);
            _statisticsCalc = new StockStatisticsCalc(_parameters, _dataManager);
            _signalManager = new StockSignalManager(_statisticsCalc, _periodHelper);

            _dataManager.PeriodEnded += DataManager_PeriodEnded;
            _signalManager.MovingAverageCrossingDetected += SignalManager_MovingAverageCrossingDetected;
        }

        private void TeardownExecutionEnvironment()
        {
            if (_dataManager != null)
            {
                _dataManager.PeriodEnded -= DataManager_PeriodEnded;
            }

            if (_signalManager != null)
            {
                _signalManager.MovingAverageCrossingDetected -= SignalManager_MovingAverageCrossingDetected;
            }

            _dataManager = null;
            _statisticsCalc = null;
            _signalManager = null;

            _periodFileManager.ArchiveSuffix = "";
        }

        private void ProcessRunFile(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Data file not specified");
                return;
            }

            _periodFileManager.ArchiveSuffix = args[0];
            SetupExecutionEnvironment();

            if (_dataManager == null)
            {
                return;
            }

            GetFilename(args[0], out var initialFilename, out var dataFilename);
            var initialList = SimulationReader.LoadPeriodSummaries(initialFilename);
            var dataList = SimulationReader.LoadStockPrices(dataFilename);

            _dataManager.AddPeriodSummaries(_simParameters.TestStockAcronym, initialList);

            foreach (StockPrice data in dataList)
            {
                _dataManager.AddStockPrices(new List<StockPrice>() { data });
            }

            TeardownExecutionEnvironment();
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
