using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace RunCatForWinform
{
    [StructLayout(LayoutKind.Sequential)]
    struct CPUInfo
    {
        internal float Total { get; set; }
        internal float User { get; set; }
        internal float Kernel { get; set; }
        internal float Idle { get; set; }
    }

    internal static class CPUInfoExtension
    {
        internal static string GetDescription(this CPUInfo cpuInfo)
        {
            return $"猫猫!\nCPU: {cpuInfo.Total:f1}%";
        }

        internal static List<string> GenerateIndicator(this CPUInfo cpuInfo)
        {
            var resultLines = new List<string>
            {
                $"CPU: {cpuInfo.Total:f1}%",
                $"   ├─ User: {cpuInfo.User:f1}%",
                $"   ├─ Kernel: {cpuInfo.Kernel:f1}%",
                $"   └─ Available: {cpuInfo.Idle:f1}%"
            };
            return resultLines;
        }
    }

    internal class CPURepository
    {
        [DllImport("CpuMonitor.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitCpuMonitor();
        [DllImport("CpuMonitor.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool GetCpuUsage(ref CPUInfo data);
        private readonly List<CPUInfo> cpuInfos = [];
        private const int CPU_INFO_LIMIT_COUNT = 5;
        internal CPURepository() 
        {
            // 构造函数里调用 C++ 的初始化
            InitCpuMonitor();
        }

        internal void Update()
        {
            CPUInfo data = new CPUInfo();

            // 传入引用 (ref)，让 C++ 把数据填进去
            if (GetCpuUsage(ref data))
            {
                // 拿到数据后，存入列表 (滑动平均逻辑)
                // 这部分逻辑保留在 C#，因为 List<T> 在 C# 用起来太舒服了
                cpuInfos.Add(data);
                if (cpuInfos.Count > CPU_INFO_LIMIT_COUNT)
                {
                    cpuInfos.RemoveAt(0);
                }
            }
        }

        internal CPUInfo Get()
        {
            if (cpuInfos.Count == 0)
            {
                return new CPUInfo();
            }
           return new CPUInfo
            {
                Total = cpuInfos.Average(x => x.Total),
                User = cpuInfos.Average(x => x.User),
                Kernel = cpuInfos.Average(x => x.Kernel),
                Idle = cpuInfos.Average(x => x.Idle)
           };
        }

        internal void Dispose()
        {}
    }
}
