using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TaskManager.Tools
{
    internal static class Utilities
    {
        internal static Random Random = new Random();
        internal static float GetCPU(Process process)
        {
            // PerformanceCounter cpuCounter = new PerformanceCounter("Process", 
                // "% Processor Time", process.ProcessName);  
            
            // cpuCounter.NextValue();  
            // await Task.Delay(500);  
            
            // return (int) cpuCounter.NextValue();
            return (float) (Random.NextDouble() * 100);
        }
        
        internal static float GetRAM(Process process)
        {
            // PerformanceCounter ramCount = new PerformanceCounter("Process", 
                // "Working Set - Private", process.ProcessName);

            // return Convert.ToInt32(ramCount.NextValue()) / 1024;
            return (float) (Random.NextDouble() * 100);
        }
    }
}