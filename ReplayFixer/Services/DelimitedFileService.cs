#nullable enable
using FileHelpers;
using FileHelpers.Events;
using Microsoft.Extensions.Logging;
using ReplayFixer.Services.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Mvvm.Contracts;

namespace ReplayFixer.Services
{
    /// <summary>
    /// Service that ease the handling of files.
    /// </summary>
    public class DelimitedFileService<T> : IDelimitedFileService<T> where T : class
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DelimitedFileService<T>> _logger;
        private readonly FileHelperAsyncEngine<T> _fileHelperEngine;
        public event EventHandler<EventArgs> OnProgress;
        public event EventHandler<EventArgs> AfterReadRecord;
        public T Current => _fileHelperEngine.LastRecord;
        public ErrorManager ErrorManager => _fileHelperEngine.ErrorManager;

        // SOLUTION 1
        //public EventHandler<ProgressEventArgs> OnProgress { set => _fileHelperEngine.Progress += value; }

        public DelimitedFileService(ILogger<DelimitedFileService<T>> logger)
        {
            _logger = logger;
            _fileHelperEngine = new FileHelperAsyncEngine<T>();
            _fileHelperEngine.Progress += _fileHelperEngine_Progress;
            _fileHelperEngine.AfterReadRecord += _fileHelperEngine_AfterReadRecord;
        }

        private void _fileHelperEngine_AfterReadRecord(EngineBase engine, AfterReadEventArgs<T> e)
        {
            var handler = AfterReadRecord;
            handler?.Invoke(engine, e);
        }

        private void _fileHelperEngine_Progress(object? sender, ProgressEventArgs e)
        {
            var handler = OnProgress;
            handler?.Invoke(sender, e);
        }

        public IDisposable BeginWriteFile(string path)=> _fileHelperEngine.BeginWriteFile(path); 
        public IDisposable BeginAppendToFile(string filePath) => _fileHelperEngine.BeginAppendToFile(filePath);
        public IDisposable BeginReadFile(string path) => _fileHelperEngine.BeginReadFile(path);
        public void WriteNext(T record) => _fileHelperEngine.WriteNext(record); 
        public T NextRecord() =>  _fileHelperEngine.ReadNext();
        public void Close() => _fileHelperEngine.Close();
        public void Dispose() => Close();
    }
}
