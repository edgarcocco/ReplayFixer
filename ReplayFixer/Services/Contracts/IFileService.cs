using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplayFixer.Services.Contracts
{
    public interface IFileService<T> : IAsyncFileService where T : class
    {
    }
}
