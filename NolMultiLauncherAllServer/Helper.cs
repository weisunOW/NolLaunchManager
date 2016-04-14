using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace NolMultiLauncherAllServer
{
    public static class Helper
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, uint lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);
 
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, uint lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        public static bool IsRunning(this int id)
        {
            try
            {
                Process.GetProcessById(id);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;   
        }
        public static bool WriteMemory(IntPtr handle, int address, long value, int length) 
        {
            int o = 0;
            byte[] val = BitConverter.GetBytes(value);
            return WriteProcessMemory(handle, (uint)address, val, length, out o);
        }
        public static byte[] ReadMemory(IntPtr handle, int address, int length) 
        {
            byte[] result = new byte[length];
            int o = 0;
            ReadProcessMemory(handle, (uint)address, result, length, out o);
            return result;
        }
    }
}
