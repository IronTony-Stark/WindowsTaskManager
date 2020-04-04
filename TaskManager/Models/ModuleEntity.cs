using System.Diagnostics;

namespace TaskManager.Models
{
    internal class ModuleEntity
    {
        private readonly string _name;
        private readonly string _path;

        public string Name => _name;
        public string Path => _path;
        
        internal ModuleEntity(ProcessModule module)
        {
            _name = module.ModuleName;
            _path = module.FileName;
        } 
    }
}