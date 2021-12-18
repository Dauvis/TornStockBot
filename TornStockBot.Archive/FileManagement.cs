using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TornStockBot.Common.Interfaces;

namespace TornStockBot.Archive
{
    public class FileManagement : IFileManagement
    {
        private readonly string _botFolder;
        private readonly string _subFolder;
        private readonly string _fileNameBase;
        private readonly string _extension;
        private readonly long _maxFileSize;

        public string ArchiveSuffix { get; set; } = string.Empty;

        public FileManagement(string botFolder, string subFolder, string fileNameBase, string extension, long maxFileSize)
        {
            _botFolder = botFolder;
            _subFolder = subFolder;
            _fileNameBase = fileNameBase;
            _extension = extension;
            _maxFileSize = maxFileSize;
        }

        public FileManagement(string botFolder, string subFolder, string fileNameBase, string extension)
        {
            _botFolder = botFolder;
            _subFolder = subFolder;
            _fileNameBase = fileNameBase;
            _extension = extension;
            _maxFileSize = 1024 * 1024;
        }

        public string CurrentFileName()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _botFolder, _subFolder, $"{_fileNameBase}.{_extension}");
        }

        public bool ShouldArchiveCurrentFile()
        {
            FileInfo fileInfo = new(CurrentFileName());

            if (!fileInfo.Exists)
            {
                return true;
            }

            return fileInfo.Length > _maxFileSize;
        }

        public void ArchiveCurrentFile()
        {
            ArchiveCurrentFile(true);
        }

        public void ArchiveCurrentFile(bool createNew)
        {
            try
            {
                string currentFileName = CurrentFileName();
                FileInfo fileInfo = new(currentFileName);

                if (fileInfo.Exists)
                {
                    string archiveFileName = ArchiveFileName();
                    File.Move(currentFileName, archiveFileName);
                }

                if (createNew)
                {
                    using var stream = File.Create(currentFileName);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        private string ArchiveFileName()
        {
            string dateString = DateTime.Now.ToString("yyyyMMddHHmmss");
            string suffix = string.IsNullOrEmpty(ArchiveSuffix) ? "" : $"_{ArchiveSuffix}";

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _botFolder, 
                _subFolder, $"{_fileNameBase}{suffix}_{dateString}.{_extension}");
        }
    }
}
