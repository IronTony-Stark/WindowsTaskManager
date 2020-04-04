using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TaskManager.Models;

namespace TaskManager.Tools
{
    internal static class Utilities
    {
        internal static readonly Random Random = new Random();

        internal const int Sec = 1000;
        
         [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass,
            out System.IntPtr ppBuffer, out int pBytesReturned);

        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pointer);
        
        internal static string GetUsernameBySessionId(int sessionId, bool prependDomain)
        {
            IntPtr buffer;
            int strLen;
            var username = "SYSTEM";
            if (!WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out buffer, out strLen) ||
                strLen <= 1)
            {
                return username;
            }

            username = Marshal.PtrToStringAnsi(buffer);
            WTSFreeMemory(buffer);
            if (!prependDomain)
            {
                return username;
            }

            if (!WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSDomainName, out buffer,
                out strLen) || strLen <= 1)
            {
                return username;
            }

            username = Marshal.PtrToStringAnsi(buffer) + "\\" + username;
            WTSFreeMemory(buffer);

            return username;
        }

        internal static ETab GetTab(string name)
        {
            switch (name)
            {
                case "Info":
                    return ETab.Info;
                case "Threads":
                    return ETab.Threads;
                case "Modules":
                    return ETab.Modules;
                default:
                    throw new ArgumentException("Unknown Tab Name");
            }
        }
    }
}