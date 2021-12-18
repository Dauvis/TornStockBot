using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TornStockBot.Common.Interfaces;

namespace TornStockBot.Archive
{
    public class NullArchiver<T> : IGenericArchiver<T> where T : class
    {
        public void ArchiveData(long timestamp, T data)
        {
            // do nothing as the point of the null archiver
            // is to stand in for an actual archiver but
            // not actuall write something.
        }
    }
}
