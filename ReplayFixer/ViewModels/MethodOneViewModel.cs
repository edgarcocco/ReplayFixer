using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ReplayFixer.Models.Data;
using ReplayFixer.Services;
using ReplayFixer.Services.Contracts;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using FileHelpers;
using System.Diagnostics;
using FileHelpers.Events;
using Microsoft.Win32;
using System.Windows;
using System.Reflection;
using System.IO;
using ReplayFixer.Models.Helpers;
using ReplayFixer.Extensions;
using FileHelpers.Converters;
using ReplayFixer.Models.Deserializers;
using Microsoft.Extensions.Options;
using ReplayFixer.Views.UserControls;
using Wpf.Ui.Controls;
using System.Windows.Threading;
using System.Threading;

namespace ReplayFixer.ViewModels
{
    public class MethodOneViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly IFileService<Replay> _replayFileService;
        private readonly IDelimitedFileService<Replay> _replayDelimitedFileService;
        private readonly ILogger<MethodOneViewModel> _logger;
        private readonly IOptions<AppConfig> _options;

        private OpenFileDialog _recOpenFileDialog;

        public ICommand NavigateCommand => new RelayCommand<Type>(OnNavigate);
        public ICommand OnFileDropCommand =>  new RelayCommand<DragEventArgs>(OnFileDrop);

        private int fileOperationProgress = 0;
        public int FileOperationProgress { get => fileOperationProgress; set => SetProperty(ref fileOperationProgress, value); }

        private ObservableCollection<Replay> workingReplaysList { get; set; } = new ObservableCollection<Replay>();

        public ObservableCollection<Replay> WorkingReplaysList { get { return workingReplaysList; } }

        public Replay ReplayToFix { get; private set; }
        private int replayIds = 1;
        public string LastWorkingReplayLoaded;
        private bool isLoadingWorkingReplays = true;
        public bool IsLoadingWorkingReplays
        {
            get { return isLoadingWorkingReplays; }
            set
            {
                SetProperty(ref isLoadingWorkingReplays, value);
            }
        }
        private bool enableFixSection = false;
        public bool EnableFixSection { get => enableFixSection; set => SetProperty(ref enableFixSection, value); }
        private string outputPath = string.Empty;
        public string OutputPath { get => outputPath; set => SetProperty(ref outputPath, value); }
        private string outputFileName = string.Empty;
        public string OutputFileName { get => outputFileName; set => SetProperty(ref outputFileName, value); }
        private bool matchReplayOutput = true;
        public bool MatchReplayOutput { get => matchReplayOutput; set => SetProperty(ref matchReplayOutput, value); }
        public MethodOneViewModel(INavigationService navigationService,
            IDelimitedFileService<Replay> replayDelimitedFileService, 
            IFileService<Replay> replayFileService,
            ILogger<MethodOneViewModel> logger,
            IOptions<AppConfig> options)
        {
            _navigationService = navigationService;
            _replayDelimitedFileService = replayDelimitedFileService;
            _replayFileService = replayFileService;
            _logger = logger;
            _options = options;

            _recOpenFileDialog = new OpenFileDialog();
            _recOpenFileDialog.Filter = "replay file (*.rec)|*.rec";
            
            _replayDelimitedFileService.OnProgress += (object? sender, EventArgs eventArgs) =>
            {
                if (sender is null)
                    return;
                ProgressEventArgs progressEventArgs = (ProgressEventArgs)eventArgs;
                FileOperationProgress = (int)progressEventArgs.Percent;
            };


            Task.Run(() =>
            {
                LoadWorkingReplays(new Progress<bool>(isLoading =>
                        IsLoadingWorkingReplays = isLoading));
            });
        }

        private async void LoadWorkingReplays(IProgress<bool> completionNotification)
        {
            // Start loading replays from our database to use for fixing the non-working replay 
            isLoadingWorkingReplays = true;
            workingReplaysList.Clear();
            List<Replay> tempReplayList = new List<Replay>();
            await Task.Run(() =>
            {
                if (File.Exists(_options.Value.ReplayDatabase))
                {
                    using (_replayDelimitedFileService.BeginReadFile(_options.Value.ReplayDatabase))
                    {
                        Replay nextReplay = new Replay();
                        while (nextReplay != null)
                        {
                            try
                            {
                                nextReplay = _replayDelimitedFileService.NextRecord();
                                // this looks dirty double null check should be removed
                                if (nextReplay != null)
                                {
                                    nextReplay.ID = replayIds++;
                                    tempReplayList.Add(nextReplay);
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, $"{HelperMethods.GetCurrentMethod()} reported an exception: {e.Message}");
                            }
                        }
                    }
                    workingReplaysList = new ObservableCollection<Replay>(tempReplayList);
                    OnPropertyChanged(nameof(WorkingReplaysList));
                    completionNotification.Report(false);
                    _logger.LogInformation($"Loaded a total of {tempReplayList.Count} replays to the collection");


                }
                else { File.Create(_options.Value.ReplayDatabase); }
            });
        }
        public void OnLoadWorkingReplaysClick()
        {
            _logger.LogInformation($"Action called: {HelperMethods.GetCurrentMethod()}");

            if (_recOpenFileDialog.ShowDialog() != null)
            {
                if (_recOpenFileDialog.FileName != string.Empty)
                {
                    LastWorkingReplayLoaded = _recOpenFileDialog.FileName;
                    _logger.LogInformation($"{HelperMethods.GetCurrentMethod()}: Loaded file {LastWorkingReplayLoaded}");
                    try
                    {
                        using (FileStream stream = File.Open(LastWorkingReplayLoaded, FileMode.Open))
                        {
                            Replay replay = ReplayDeserializer.FromStream(stream);
                            using (_replayDelimitedFileService.BeginAppendToFile(_options.Value.ReplayDatabase))
                            {
                                _replayDelimitedFileService.WriteNext(replay);
                            }
                            workingReplaysList.Add(replay);
                            OnPropertyChanged(nameof(workingReplaysList));
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation($"{HelperMethods.GetCurrentMethod()} reported an exception: {e.Message}");
                        // display something went wrong alert
                    }
                }
            }
        }
        private void OnFileDrop(DragEventArgs eventArgs)
        {
            var fileDroppedPath = ((string[])eventArgs.Data.GetData(DataFormats.FileDrop, false));
            
            try
            {
                using (FileStream stream = File.Open(fileDroppedPath[0], FileMode.Open))
                {
                    ReplayToFix = ReplayDeserializer.FromStream(stream);
                    ReplayCard replayCard = new ReplayCard();
                    replayCard.Replay = ReplayToFix;
                    var sourceCard = (Card)eventArgs.Source;
                    sourceCard.Content= replayCard;

                    EnableFixSection = true;
                    OutputPath = ReplayToFix.FileDirectory;
                    var fileNameSplitted = ReplayToFix.FileName.Split('.');
                    fileNameSplitted[0] += "-fixed";

                    OutputFileName = String.Join('.', fileNameSplitted);

                }
            } catch(Exception e)
            {
                _logger.LogError(e, $"{HelperMethods.GetCurrentMethod()} reported an exception: {e.Message}");
            }
        }
        
        public void OnFixReplayClick()
        {
            Replay? suitableReplay = null;
            foreach(Replay workingReplay in workingReplaysList)
            {
                if(workingReplay.WorkshopFileName == ReplayToFix.WorkshopFileName)
                {
                    suitableReplay = workingReplay;
                    break;
                }
            }

            if (suitableReplay == null)
            {
                _logger.LogInformation($"{HelperMethods.GetCurrentMethod()}: No suitable replay found to fix given replay. ");
                return;
            }

            var workingReplayBytes = Convert.FromBase64String(suitableReplay.FixerBytes)
                                            .SplitBy(i => i == 0x24).ToArray();
            var damagedReplayBytes = Convert.FromBase64String(ReplayToFix.FixerBytes)
                                            .SplitBy(i => i == 0x24).ToArray();

            // make sure both replays contains the same length of data extracted
            if(workingReplayBytes.Length == damagedReplayBytes.Length)
            {
                string replayToFixFullPath = ReplayToFix.FileDirectory + "\\" + ReplayToFix.FileName;
                string workingReplayFullPath = suitableReplay.FileDirectory + "\\" + suitableReplay.FileName;
                string destinationFullPath = OutputPath + "\\" + OutputFileName;

                byte[] fixedReplayBuffer;
                using (FileStream fileStream = File.Open(replayToFixFullPath, FileMode.Open))
                {
                    byte[] damagedFileBuffer = new byte[fileStream.Length];
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        damagedFileBuffer = binaryReader.ReadBytes((int)fileStream.Length);
                    }
                    for (int i = 0; i < workingReplayBytes.Length; i++)
                    {
                        // it's easy to search and replace bytes that their count is the same
                        // where it gets a bit complicated it's when they have different bytes
                        if (workingReplayBytes[i].Count == damagedReplayBytes[i].Count)
                        {
                            // find the byte section part to fix and position the writer there
                            int indexToFix = HelperMethods.SearchFirst(damagedFileBuffer, damagedReplayBytes[i].ToArray());
                            //binaryWriter.Seek(indexToFix, SeekOrigin.Begin);
                            for(int j = 0; j < workingReplayBytes[i].Count; j++)
                            {
                                damagedFileBuffer[indexToFix + j] = workingReplayBytes[i][j];
                            }
                        } else
                        {
                            // we gotta attempt to remove the entire section without removing any other byte and replace it
                            // with the one that works.
                            int startIndexToFix = HelperMethods.SearchFirst(damagedFileBuffer, damagedReplayBytes[i].ToArray());
                            int dataLength = damagedReplayBytes[i].Count;
                            List<byte> tempList = damagedFileBuffer.ToList();
                            tempList.RemoveRange(startIndexToFix, dataLength);
                            tempList.InsertRange(startIndexToFix, workingReplayBytes[i].ToArray());
                            damagedFileBuffer = tempList.ToArray();
                            
                        }
                    }
                    fixedReplayBuffer = damagedFileBuffer;
                }
                if (fixedReplayBuffer.Length > 0)
                {
                    using var writer = new BinaryWriter(File.OpenWrite(destinationFullPath));
                    writer.Write(fixedReplayBuffer);
                }
            }
        }

        private void OnNavigate(Type? parameter)
        {
            _navigationService.Navigate(parameter);
        }
        public void OnNavigatedFrom()
        {
            _logger.LogInformation($"{typeof(MethodOneViewModel)} navigated", "ReplayFixer");
        }
        public void OnNavigatedTo()
        {
            _logger.LogInformation($"{typeof(MethodOneViewModel)} navigated", "ReplayFixer");
        }
    }
}
