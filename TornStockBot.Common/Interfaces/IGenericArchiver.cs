using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TornStockBot.Common.Interfaces
{
    public interface IGenericArchiver<T> where T : class
    {
        public void ArchiveData(long timestamp, T data);
    }
}
