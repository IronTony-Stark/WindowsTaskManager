using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using TaskManager.Models;
using TaskManager.Tools;
using TaskManager.Tools.Enums;

namespace TaskManager.ViewModels
{
    internal class TaskGridViewModel : BaseViewModel
    {
        #region Fields

        private ObservableCollection<ProcessEntity> _processes = new ObservableCollection<ProcessEntity>();
        private ProcessEntity _selectedProcess;

        private readonly Timer _updateProcesses;
        private readonly Timer _updateMetadata;

        private ETab _tab = ETab.Info;
        private ESortBy _sortBy = ESortBy.None;

        #region Commands

        private RelayCommand<object> _openFolderCommand;
        private RelayCommand<object> _killProcessCommand;

        #endregion

        #endregion

        #region Properties

        public ObservableCollection<ProcessEntity> Processes
        {
            get => _processes;
            set
            {
                _processes = value;
                OnPropertyChanged();
            }
        }

        public ProcessEntity SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                _selectedProcess = value;
                OnPropertyChanged();
            }
        }

        public TabItem SelectedTab
        {
            set
            {
                _tab = Utilities.GetTab((string) value.Header);
                OnPropertyChanged();
            }
        }

        internal Timer UpdateProcesses => _updateProcesses;
        internal Timer UpdateMetadata => _updateMetadata;

        public string SortBy
        {
            set
            {
                _sortBy = Utilities.GetSortBy(value);
                OnPropertyChanged();
            }
        }

        #region Commands

        public RelayCommand<object> OpenFolderCommand => _openFolderCommand ?? (_openFolderCommand =
            new RelayCommand<object>(OpenFolder,
                o => OpenFolderCanExecute()));

        public RelayCommand<object> KillProcessCommand => _killProcessCommand ?? (_killProcessCommand =
            new RelayCommand<object>(KillProcess,
                o => KillProcessCanExecute()));

        #endregion

        #endregion

        internal TaskGridViewModel()
        {
            // Timer updates data asynchronously
            _updateMetadata = new Timer(2 * Utilities.Sec);
            _updateMetadata.Elapsed += UpdateMetadataCallback;

            UpdateProcessesCallback(new object(), new EventArgs());

            _updateProcesses = new Timer(5 * Utilities.Sec);
            _updateProcesses.Elapsed += UpdateProcessesCallback;

            _updateProcesses.Start();
            _updateMetadata.Start();
        }

        #region Update

        private void UpdateProcessesCallback(object sender, EventArgs e)
        {
            var currentIds = Processes.Select(p => p.Id).ToList();

            foreach (Process p in Process.GetProcesses())
                if (!currentIds.Remove(p.Id))
                    Processes.Add(new ProcessEntity(p)); // new process

            // remove processes that do not exist anymore
            foreach (ProcessEntity process in currentIds.Select(id => Processes.First(p => p.Id == id)))
                Processes.Remove(process);
            
            SortProcesses();
        }
        
        private void SortProcesses()
        {
            switch (_sortBy)
            {
                case ESortBy.None:
                    break;
                case ESortBy.Name:
                    Processes = new ObservableCollection<ProcessEntity>(Processes.OrderBy(i => i.Name));
                    break;
                case ESortBy.IsActive:
                    Processes = new ObservableCollection<ProcessEntity>(Processes.OrderBy(i => i.IsActive));
                    break;
                case ESortBy.CPU:
                    Processes = new ObservableCollection<ProcessEntity>(Processes.OrderByDescending(i => i.CPU));
                    break;
                case ESortBy.RAM:
                    Processes = new ObservableCollection<ProcessEntity>(Processes.OrderByDescending(i => i.RAM));
                    break;
                default:
                    throw new ArgumentException("Sort By Unknown Property");
            }
        }

        private void UpdateMetadataCallback(object o, EventArgs eventArgs)
        {
            for (int i = 0; i < Processes.Count; i++)
            {
                ProcessEntity process = Processes[i];
                process.UpdateMetaData(SelectedProcess, _tab);
            }
        }

        #endregion

        #region CommandsImpl

        private void OpenFolder(object obj)
        {
            try
            {
                Process.Start($@"{SelectedProcess.Path}");
            }
            catch (Win32Exception win32Exception)
            {
                MessageBox.Show("File Not Found. " + win32Exception.Message);
            }
        }

        private bool OpenFolderCanExecute()
        {
            return !string.IsNullOrEmpty(SelectedProcess?.Path);
        }

        private void KillProcess(object obj)
        {
            try
            {
                Process process = Process.GetProcessById(SelectedProcess.Id);
                process.Kill();
                Processes.Remove(SelectedProcess);
                SelectedProcess = null;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private bool KillProcessCanExecute()
        {
            return SelectedProcess != null;
        }

        #endregion
    }
}