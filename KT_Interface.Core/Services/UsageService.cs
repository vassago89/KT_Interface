using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
namespace KT_Interface.Core.Services
{
    public class UsageService
    {
        public static class PerformanceInfo
        {
            [DllImport("psapi.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

            [StructLayout(LayoutKind.Sequential)]
            public struct PerformanceInformation
            {
                public int Size;
                public IntPtr CommitTotal;
                public IntPtr CommitLimit;
                public IntPtr CommitPeak;
                public IntPtr PhysicalTotal;
                public IntPtr PhysicalAvailable;
                public IntPtr SystemCache;
                public IntPtr KernelTotal;
                public IntPtr KernelPaged;
                public IntPtr KernelNonPaged;
                public IntPtr PageSize;
                public int HandlesCount;
                public int ProcessCount;
                public int ThreadCount;
            }

            public static Int64 GetTotalMemoryInMiB()
            {
                PerformanceInformation pi = new PerformanceInformation();
                if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
                {
                    return Convert.ToInt64((pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1048576));
                }
                else
                {
                    return -1;
                }

            }
        }

        public double CpuUsage 
        {
              get
              {
                  return _cpuUsage.NextValue();
              }
        }

        public double MemoryAvailable
        {
            get
            {
               return _memoryAvailable.NextValue();
            }
        }

        public double MemoryTotal { get; private set; }

        public IEnumerable<DriveInfo> DriveInfos { get; private set; }

        PerformanceCounter _cpuUsage;
        PerformanceCounter _memoryAvailable;

        public UsageService()
        {
            _cpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _memoryAvailable = new PerformanceCounter("Memory", "Available MBytes");
            MemoryTotal = PerformanceInfo.GetTotalMemoryInMiB();
            DriveInfos = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed);
        }
    }
}
