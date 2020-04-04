using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using TaskManager.Models;

namespace TaskManager.ViewModels
{
    // process updates every 5s
    // process metadata(isActive, CPU, RAM) updates every 2s
    // data grid
    // sort data
    // preserve sorting and selection after update

    internal class TaskGridViewModel : BaseViewModel
    {

        #region Fields

        private Process _selectedProcess;
        private ObservableCollection<ProcessEntity> _processes = new ObservableCollection<ProcessEntity>();

        private readonly DispatcherTimer _updateProcesses;
        private readonly DispatcherTimer _updateMetadata;

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

        public Process SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                _selectedProcess = value;
                OnPropertyChanged();
            }
        }

        internal DispatcherTimer UpdateProcesses => _updateProcesses;
        internal DispatcherTimer UpdateMetadata => _updateMetadata;

        #endregion

        internal TaskGridViewModel()
        {
            _updateMetadata = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _updateMetadata.Tick += UpdateMetadataCallback;

            _updateProcesses = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _updateProcesses.Tick += UpdateProcessesCallback;
            
            UpdateProcessesCallback(new object(), new EventArgs());
            UpdateMetadataCallback(new object(), new EventArgs());

            _updateProcesses.Start();
            _updateMetadata.Start();
        }

        private void UpdateProcessesCallback(object sender, EventArgs e)
        {
            UpdateMetadata.IsEnabled = false;
            
            var currentIds = Processes.Select(p => p.Id).ToList();

            foreach (Process p in Process.GetProcesses())
                if (!currentIds.Remove(p.Id))
                    Processes.Add(new ProcessEntity(p)); // new process

            // remove processes that do not exist anymore
            foreach (ProcessEntity process in currentIds.Select(id => Processes.First(p => p.Id == id)))
                Processes.Remove(process);
            
            UpdateMetadata.IsEnabled = true;
        }
        
        private void UpdateMetadataCallback(object o, EventArgs eventArgs)
        {
            foreach (ProcessEntity process in Processes)
                process.UpdateMetaData();
        }

        private void OpenFolder()
        {
            try
            {
                Process.Start($@"{SelectedProcess.StartInfo.WorkingDirectory}");
            } 
            catch (Win32Exception win32Exception)
            {
                MessageBox.Show("File Not Found. " + win32Exception.Message);
            }
        }

        private void KillSelectedProcess()
        {
            SelectedProcess.Kill();
        }
    }
}