using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common.Interfaces
{
    public interface IFileManagement
    {
        public string CurrentFileName();
        public bool ShouldArchiveCurrentFile();
        public void ArchiveCurrentFile();
        public void ArchiveCurrentFile(bool createNew);

        public string ArchiveSuffix { get; set; }
    }
}
