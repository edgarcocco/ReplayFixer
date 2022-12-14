using FileHelpers.Events;
using ReplayFixer.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplayFixer.Services
{
    public class FileService<T> : IFileService<T> where T : class
    {
        private readonly IServiceProvider _serviceProvider;
        public byte[] RawDataLoaded;

        public event EventHandler<EventArgs> AfterReadRecord;

        public T? Value { get; private set; }

        public FileService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        event EventHandler<EventArgs> IAsyncFileService.OnProgress
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public IDisposable BeginReadFile(string filePath)
        {
            IDisposable disposable = default!;
            {
                string filename = @"c:\Temp\userinputlog.txt";

                using (FileStream SourceStream = File.Open(filename, FileMode.Open))
                {
                    RawDataLoaded = new byte[SourceStream.Length];
                    SourceStream.ReadAsync(RawDataLoaded, 0, (int)SourceStream.Length);
                }

                Value = (T?)null;

            };
            return disposable;
        }

        public IDisposable BeginWriteFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginAppendToFile(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
