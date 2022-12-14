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

        private readonly FileHelperAsyncEngine<T> _fileHelperEngine;

        public event EventHandler<EventArgs> OnProgress;
        public event EventHandler<EventArgs> AfterReadRecord;

        public T Current
        {
            get { return _fileHelperEngine.LastRecord; }
        }

        // SOLUTION 1
        //public EventHandler<ProgressEventArgs> OnProgress { set => _fileHelperEngine.Progress += value; }


        /// <summary>
        /// Creates new instance and attaches the <see cref="IServiceProvider"/>.
        /// </summary>
        public DelimitedFileService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _fileHelperEngine = new FileHelperAsyncEngine<T>();
            _fileHelperEngine.Progress += _fileHelperEngine_Progress;
            _fileHelperEngine.AfterReadRecord += _fileHelperEngine_AfterReadRecord;
        }

        private void _fileHelperEngine_AfterReadRecord(EngineBase engine, AfterReadEventArgs<T> e)
        {
            var handler = AfterReadRecord;
            if (handler != null) AfterReadRecord(engine, e);
        }

        private void _fileHelperEngine_Progress(object? sender, ProgressEventArgs e)
        {
            var handler = OnProgress;
            if (handler != null) OnProgress(sender, e);
        }

        public IDisposable BeginWriteFile(string path)=> _fileHelperEngine.BeginWriteFile(path); 
        public IDisposable BeginAppendToFile(string filePath) => _fileHelperEngine.BeginAppendToFile(filePath);
        public IDisposable BeginReadFile(string path) => _fileHelperEngine.BeginReadFile(path);
        public void WriteNext(T record) => _fileHelperEngine.WriteNext(record); 
        public T NextRecord() =>  _fileHelperEngine.ReadNext();
        public void Close() => _fileHelperEngine.Close();
        public void Dispose() => this.Close();
    }
}
