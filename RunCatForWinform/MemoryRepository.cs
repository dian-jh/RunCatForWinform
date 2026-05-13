using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RunCatForWinform
{

    struct MemoryInfo
    {
        internal uint MemoryLoad { get; set; }
        internal long TotalMemory { get; set; }
        internal long AvailableMemory { get; set; }
        internal long UsedMemory { get; set; }
    }

    internal static class MemoryInfoExtension
    {
        internal static List<string> GenerateIndicator(this MemoryInfo memoryInfo)
        {
            var resultLines = new List<string>
            {
                $"Memory: {memoryInfo.MemoryLoad}%",
                $"   ├─ Total: {memoryInfo.TotalMemory.ToByteFormatted()}",
                $"   ├─ Used: {memoryInfo.UsedMemory.ToByteFormatted()}",
                $"   └─ Available: {memoryInfo.AvailableMemory.ToByteFormatted()}"
            };
            return resultLines;
        }

        internal static string ToByteFormatted(this long bytes)
        {
            string[] units = ["B", "KB", "MB", "GB", "TB"];
            int i = 0;
            double doubleBytes = bytes;
            while (1024 <= doubleBytes && i < units.Length - 1)
            {
                doubleBytes /= 1024;
                i++;
            }
            return string.Format("{0:0.##} {1}", doubleBytes, units[i]);
        }
    }


    internal partial class MemoryRepository
    {
        private MemoryInfo memoryInfo;

        internal MemoryRepository()
        {
            memoryInfo = new MemoryInfo();
        }

        internal void Update()
        {
            var memStatus = new MemoryStatusEx();
            //winapi在调用前要告诉结构体大小
            memStatus.dwLength = (uint)Marshal.SizeOf(memStatus);

            if (GlobalMemoryStatusEx(ref memStatus))
            {
                //搬运数据
                memoryInfo.MemoryLoad = memStatus.dwMemoryLoad;
                memoryInfo.TotalMemory = (long)memStatus.ullTotalPhys;
                memoryInfo.AvailableMemory = (long)memStatus.ullAvailPhys;
                memoryInfo.UsedMemory = (long)(memStatus.ullTotalPhys - memStatus.ullAvailPhys);
            }
        }

        internal MemoryInfo Get()
        {
            Update();
            return memoryInfo;
        }

        [StructLayout(LayoutKind.Sequential)]//禁止打乱顺序,
        internal struct MemoryStatusEx
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;//物理可用内存
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);
    }
}
