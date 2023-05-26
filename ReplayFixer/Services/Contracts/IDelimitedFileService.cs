using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplayFixer.Services.Contracts
{
    public interface IDelimitedFileService<T> : IAsyncFileService, IDisposable where T : class
    {
        void Close();
        T Current { get; }
        T NextRecord();
        void WriteNext(T record);
    }
}
