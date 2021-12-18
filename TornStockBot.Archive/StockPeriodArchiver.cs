using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TornStockBot.Common;
using TornStockBot.Common.Interfaces;

namespace TornStockBot.Archive
{
    public class StockPeriodArchiver : IGenericArchiver<List<PeriodStatistics>>
    {
        private readonly IFileManagement _fileManager;

        public StockPeriodArchiver(IFileManagement fileManager)
        {
            _fileManager = fileManager;
            _fileManager.ArchiveCurrentFile(false);
        }

        public void ArchiveData(long timestamp, List<PeriodStatistics> data)
        {
            try
            {
                bool didArchive = ManageFile();

                if (didArchive)
                {
                    CreateArchiveFile(data);
                }
                else
                {
                    AppendArchiveFile(data);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error archiving stock period data");
                Console.Error.WriteLine(ex.Message);
            }
        }

        private void AppendArchiveFile(List<PeriodStatistics> data)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Don't write the header again.
                HasHeaderRecord = false,
            };

            using var stream = File.Open(_fileManager.CurrentFileName(), FileMode.Append);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(data);
        }

        private void CreateArchiveFile(List<PeriodStatistics> data)
        {
            using var writer = new StreamWriter(_fileManager.CurrentFileName());
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(data);
        }

        private bool ManageFile()
        {
            if (_fileManager.ShouldArchiveCurrentFile())
            {
                _fileManager.ArchiveCurrentFile();
                return true;
            }

            return false;
        }
    }
}
