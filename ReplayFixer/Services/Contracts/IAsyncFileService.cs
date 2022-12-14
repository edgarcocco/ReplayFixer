using FileHelpers.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplayFixer.Services.Contracts
{
    public interface IAsyncFileService
    {
        IDisposable BeginReadFile(string filePath);
        IDisposable BeginWriteFile(string filePath);
        IDisposable BeginAppendToFile(string filePath);


        // SOLUTION 1
        // EventHandler<ProgressEventArgs> OnProgress { set; }

        event EventHandler<EventArgs> OnProgress;

        event EventHandler<EventArgs> AfterReadRecord;

    }
}
