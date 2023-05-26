using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ReplayFixer.Models.Data;
using ReplayFixer.Services.Contracts;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using FileHelpers.Events;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using ReplayFixer.Models.Helpers;
using ReplayFixer.Extensions;
using ReplayFixer.Models.Deserializers;
using Microsoft.Extensions.Options;
using ReplayFixer.Views.UserControls;
using Wpf.Ui.Controls.Interfaces;

namespace ReplayFixer.ViewModels
{
    public class MethodOneViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly IDelimitedFileService<Replay> _replayDelimitedFileService;
        private readonly ILogger<MethodOneViewModel> _logger;
        private readonly IOptions<AppConfig> _options;

        private OpenFileDialog _recOpenFileDialog;
        private readonly IDialogControl _dialogControl;

        public ICommand NavigateCommand => new RelayCommand<Type>(OnNavigate);
        public ICommand OnFileDropCommand =>  new RelayCommand<string>(execute: ReplayToFixCardMouseDown);
        public ICommand OpenFileDialogCommand => new RelayCommand<string>(execute: OpenFileDialogRouter);
        public ICommand ViewReplayWindowCommand => new RelayCommand<Replay>(OpenReplayMessageBox);

       

        private int _fileOperationProgress = 0;
        public int FileOperationProgress { get => _fileOperationProgress; set => SetProperty(ref _fileOperationProgress, value); }

        private ObservableCollection<Replay> workingReplaysList { get; set; } = new ObservableCollection<Replay>();

        public ObservableCollection<Replay> WorkingReplaysList => workingReplaysList;

        public Replay ReplayToFix { get; private set; } = new Replay();
        private int _replayIds = 1;
        public string? LastWorkingReplayLoaded;
        private bool _isLoadingWorkingReplays = true;
        public bool IsLoadingWorkingReplays
        {
            get => _isLoadingWorkingReplays;
            set => SetProperty(ref _isLoadingWorkingReplays, value);
        }
        private bool _enableFixSection = false;
        public bool EnableFixSection { get => _enableFixSection; set => SetProperty(ref _enableFixSection, value); }
        private string _outputPath = string.Empty;
        public string OutputPath { get => _outputPath; set => SetProperty(ref _outputPath, value); }
        private string _outputFileName = string.Empty;
        public string OutputFileName { get => _outputFileName; set => SetProperty(ref _outputFileName, value); }
        private bool _matchReplayOutput = true;
        public bool MatchReplayOutput { get => _matchReplayOutput; set => SetProperty(ref _matchReplayOutput, value); }
        private string _replayToFixStatus = "This box will hold data about the replay to fix...";
        public string ReplayToFixStatus
        {
            get => _replayToFixStatus;
            set => SetProperty(ref _replayToFixStatus, value);
        }
        public MethodOneViewModel(INavigationService navigationService,
            IDialogService dialogService, 
            IDelimitedFileService<Replay> replayDelimitedFileService,
            ILogger<MethodOneViewModel> logger,
            IOptions<AppConfig> options)
        {
            _navigationService = navigationService;
            _replayDelimitedFileService = replayDelimitedFileService;
            _logger = logger;
            _options = options;
            _dialogControl = dialogService.GetDialogControl();

            _recOpenFileDialog = new OpenFileDialog
            {
                Filter = "replay file (*.rec)|*.rec"
            };

            _replayDelimitedFileService.OnProgress += (sender, eventArgs) =>
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
            _isLoadingWorkingReplays = true;
            workingReplaysList.Clear();
            var tempReplayList = new List<Replay>();
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
                                if (nextReplay == null) continue;
                                nextReplay.ID = _replayIds++;
                                tempReplayList.Add(nextReplay);
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
                else
                {
                    if (_options.Value.ReplayDatabase != null) File.Create(_options.Value.ReplayDatabase).Close();
                }
            });
        }
        public void OpenFileDialogRouter(string? methodName)
        {
            _logger.LogInformation($"Action called: {HelperMethods.GetCurrentMethod()}");

            _recOpenFileDialog.Reset();
            if (_recOpenFileDialog.ShowDialog() != null)
            {
                if (_recOpenFileDialog.FileName != string.Empty)
                {
                    if (methodName != null)
                    {
                        MethodInfo? dynMethod = this.GetType().GetMethod(methodName,
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        if (dynMethod != null) dynMethod.Invoke(this, new object[] { _recOpenFileDialog.FileName });
                    }

                    //OpenFileDialogWorkingReplay(_recOpenFileDialog);
                }
            }
        }

        private void OpenFileDialogWorkingReplay(string? fileName)
        {
            LastWorkingReplayLoaded = fileName;
            _logger.LogInformation($"{HelperMethods.GetCurrentMethod()}: Loaded file {LastWorkingReplayLoaded}");
            try
            {
                using (FileStream stream = File.Open(LastWorkingReplayLoaded, FileMode.Open))
                {
                    Replay replay = ReplayDeserializer.FromStream(stream);
                    replay.ID = _replayIds++;
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
        private void ReplayToFixCardMouseDown(string fileName)
        {
            var fileDroppedPath = fileName;//(eventArgs.Data.GetData(DataFormats.FileDrop, false) as string[])?.FirstOrDefault();
            try
            {
                if (fileDroppedPath != null)
                {
                    using var stream = File.Open(fileDroppedPath, FileMode.Open);
                    ReplayToFix = ReplayDeserializer.FromStream(stream);
                    ReplayToFixStatus = "Replay Loaded \n\n"+ReplayToFix+"\n";

                    //var sourceCard = (Card)eventArgs.Source;
                    //sourceCard.Content = replayCard;

                    EnableFixSection = true;
                    OutputPath = ReplayToFix.FileDirectory;
                    var fileNameExploded = ReplayToFix.FileName.Split('.');
                    fileNameExploded[0] += "-fixed";

                    OutputFileName = string.Join('.', fileNameExploded);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{HelperMethods.GetCurrentMethod()} reported an exception: {e.Message}");
            }
        }
        private void OpenReplayMessageBox(Replay? replay)
        {
            var messageBox = new Wpf.Ui.Controls.MessageBox();

            var content = replay.ToString();
            messageBox.ShowFooter = false;
            messageBox.Show("Replay Card", content);
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
                    byte[] damagedFileBuffer;
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

        #region Interface Implementations
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
        #endregion
    }
}
