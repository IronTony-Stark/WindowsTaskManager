using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TaskManager.Tools;

namespace TaskManager.Models
{
    public class ProcessEntity : INotifyPropertyChanged, IComparable<ProcessEntity>
    {

        #region Fields

        private readonly Process _process;
        
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

        #endregion

        // threads {id, state, startTime, ...}
        public ProcessThreadCollection Threads => _process.Threads;
        // modules {name, path, ...}
        public ProcessModuleCollection Modules => _process.Modules;

        internal ProcessEntity(Process process)
        {
            _process = process;
            _id = _process.Id;
            _name = _process.ProcessName;
            _username = _process.StartInfo.UserName;
            _path = _process.StartInfo.WorkingDirectory + _process.StartInfo.FileName;

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
        }

        internal void UpdateMetaData()
        {
            IsActive = _process.Responding;
            CPU = Utilities.GetCPU(_process);
            RAM = Utilities.GetRAM(_process);
            ThreadsNum = _process.Threads.Count;
        }
        
        [field: NonSerialized] public event PropertyChangedEventHandler PropertyChanged;

        private protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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