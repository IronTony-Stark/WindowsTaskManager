using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using TaskManager.Tools;
using TaskManager.Tools.Enums;

namespace TaskManager.Models
{
    // TODO better CPU
    internal sealed class ProcessEntity : INotifyPropertyChanged, IComparable<ProcessEntity>
    {
        #region Fields

        private readonly Process _process;
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _ramCounter;

        // static data
        private readonly int _id;
        private readonly string _name;
        private readonly string _username;
        private readonly string _path;
        private readonly DateTime? _startTime;
        private readonly ProcessModule _mainModule;

        // dynamic data
        private bool _isActive;
        private float _cpu;
        private float _ram;
        private int _threadsNum;
        private List<ThreadEntity> _threads;
        private List<ModuleEntity> _modules;

        #endregion

        #region Properties

        // static data
        public int Id => _id;
        public string Name => _name;
        public string Username => _username;
        public string Path => _path;
        public DateTime? StartTime => _startTime;
        public ProcessModule MainModule => _mainModule;

        // dynamic data
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public float CPU
        {
            get => _cpu;
            set
            {
                _cpu = value;
                OnPropertyChanged();
            }
        }

        public float RAM
        {
            get => _ram;
            set
            {
                _ram = value;
                OnPropertyChanged();
            }
        }

        public int ThreadsNum
        {
            get => _threadsNum;
            set
            {
                _threadsNum = value;
                OnPropertyChanged();
            }
        }

        public List<ThreadEntity> Threads
        {
            get => _threads;
            set
            {
                _threads = value;
                OnPropertyChanged();
            }
        }

        public List<ModuleEntity> Modules
        {
            get => _modules;
            set
            {
                _modules = value;
                OnPropertyChanged();
            }
        }

        #endregion

        internal ProcessEntity(Process process)
        {
            _process = process;
            _id = _process.Id;
            _name = _process.ProcessName;
            _username = Utilities.GetUsernameBySessionId(process.SessionId, true);
            
            try
            {
                _path = _process.MainModule?.FileName;
                if (_path != null && _path.EndsWith(".exe"))
                {
                    int lastSlash = _path.LastIndexOf("\\", StringComparison.Ordinal);
                    _path = _path.Substring(0, lastSlash);
                }
            }
            catch (Exception)
            {
                _path = null;
            }

            try
            {
                _startTime = _process.StartTime;
            }
            catch (Exception)
            {
                _startTime = null;
            }

            try
            {
                _mainModule = _process.MainModule;
            }
            catch (Exception)
            {
                _mainModule = null;
            }

            _cpuCounter = new PerformanceCounter("Process",
                "% Processor Time", process.ProcessName, true);

            _ramCounter = new PerformanceCounter("Process",
                "Working Set - Private", process.ProcessName);
        }

        internal void UpdateMetaData(ProcessEntity selectedProcess, ETab tab)
        {
            _process.Refresh();
            
            IsActive = _process.Responding;
            // It's not always possible to track process's CPU
            try
            {
                CPU = _cpuCounter.NextValue();
            }
            catch (Exception)
            {
                CPU = -1;
            }
            RAM = (float) Math.Round((double) _ramCounter.RawValue / 1024 / 1024, 2);
            ThreadsNum = _process.Threads.Count;

            if (this != selectedProcess) return;
            if (tab == ETab.Threads) UpdateThreads();
            else if (tab == ETab.Modules) UpdateModules();
        }

        private void UpdateThreads()
        {
            // not all threads are accessible
            try
            {
                var threads = (
                    from ProcessThread thread in _process.Threads
                    select new ThreadEntity(thread)
                ).ToList();

                Threads = threads;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void UpdateModules()
        {
            // not all modules are accessible due to 32-64 bit system problem
            try
            {
                var modules = (
                    from ProcessModule module in _process.Modules
                    select new ModuleEntity(module)
                ).ToList();

                Modules = modules;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [field: NonSerialized] public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int CompareTo(ProcessEntity other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return _id.CompareTo(other._id);
        }
    }
}