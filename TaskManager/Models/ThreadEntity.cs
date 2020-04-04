using System;
using System.Diagnostics;

namespace TaskManager.Models
{
    internal class ThreadEntity
    {
        private readonly int _id;
        private readonly ThreadState _state;
        private readonly DateTime _startTime;

        public int Id => _id;
        public ThreadState State => _state;
        public DateTime StartTime => _startTime;
        
        internal ThreadEntity(ProcessThread thread)
        {
            _id = thread.Id;
            _state = thread.ThreadState;
            _startTime = thread.StartTime;
        }
    }
}