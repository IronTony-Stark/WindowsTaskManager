using System;
using System.Collections.Generic;
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
            var newProcesses = Processes.ToList();
            var currentIds = newProcesses.Select(p => p.Id).ToList();

            newProcesses.AddRange(
                from p in Process.GetProcesses()
                where !currentIds.Remove(p.Id)
                select new ProcessEntity(p) // new process
            );

            // remove processes that do not exist anymore
            foreach (ProcessEntity process in currentIds.Select(id => Processes.First(p => p.Id == id)))
                newProcesses.Remove(process);

            SortProcesses(newProcesses);
        }

        private void SortProcesses(List<ProcessEntity> newProcesses)
        {
            switch (_sortBy)
            {
                case ESortBy.None:
                    break;
                case ESortBy.Name:
                    newProcesses.Sort((a, b) =>
                        string.Compare(a.Name, b.Name, StringComparison.Ordinal));
                    break;
                case ESortBy.IsActive:
                    newProcesses.Sort((a, b) => 
                        a.IsActive.CompareTo(b.IsActive));
                    break;
                case ESortBy.CPU:
                    newProcesses.Sort((a, b) => 
                        b.CPU.CompareTo(a.CPU));
                    break;
                case ESortBy.RAM:
                    newProcesses.Sort((a, b) => 
                        b.RAM.CompareTo(a.RAM));
                    break;
                default:
                    throw new ArgumentException("Sort By Unknown Property");
            }

            Application.Current.Dispatcher?.Invoke(delegate
            {
                Processes = new ObservableCollection<ProcessEntity>(newProcesses);
            });
        }

        private void UpdateMetadataCallback(object o, EventArgs eventArgs)
        {
            foreach (ProcessEntity process in Processes)
                process.UpdateMetaData(SelectedProcess, _tab);
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