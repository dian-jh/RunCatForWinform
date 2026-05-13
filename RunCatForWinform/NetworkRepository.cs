using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace RunCatForWinform
{
    struct NetworkInfo
    {
        internal float SentSpeed { get; set; }
        internal float ReceivedSpeed { get; set; }
    }

    internal static class NetworkInfoExtension
    {
        internal static List<string> GenerateIndicator(this NetworkInfo networkInfo)
        {
            return new List<string>
            {
                $"Network:",
                $"   ├─ Sent: {FormatSpeed(networkInfo.SentSpeed)}",
                $"   └─ Received: {FormatSpeed(networkInfo.ReceivedSpeed)}"
            };
        }

        private static string FormatSpeed(float speedBytes)
        {
            return ((long)speedBytes).ToByteFormatted() + "/s";
        }
    }

    internal class NetworkRepository
    {
        private readonly NetworkInterface networkInterface;
        private long lastSent;
        private long lastReceived;
        private DateTime lastUpdate;  
        private NetworkInfo networkInfo;

        internal NetworkRepository()
        {
            networkInterface = GetActiveNetworkInterface()
                ?? throw new InvalidOperationException("No valid network interface found.");
            //初始化
            var stats = networkInterface.GetIPStatistics();
            lastSent = stats.BytesSent;
            lastReceived = stats.BytesReceived;
            lastUpdate = DateTime.UtcNow;
        }

        private static NetworkInterface? GetActiveNetworkInterface()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            return interfaces.FirstOrDefault(IsValidNetworkInterface);
        }

        private static bool IsValidNetworkInterface(NetworkInterface networkInterface)
        {
            //排除回环和隧道(tunnel)
            if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback) return false;
            if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Tunnel) return false;
            //排除未联网的，拔网线，状态就是Down
            if (networkInterface.OperationalStatus != OperationalStatus.Up) return false;
            //进行排除判断
            var description = networkInterface.Description.ToLower();
            if (description.Contains("vpn")) return false;
            if (description.Contains("tap")) return false;
            if (description.Contains("virtual")) return false;
            if (description.Contains("tun")) return false;
            //剩下的就是真网卡
            return true;
        }

        internal void Update()
        {
            var stats = networkInterface.GetIPStatistics();
            var now = DateTime.UtcNow;
            var elapsedSec = (now - lastUpdate).TotalSeconds;//计算时间差
            if (elapsedSec > 0)
            {
                networkInfo.SentSpeed = (float)((stats.BytesSent - lastSent) / elapsedSec);
                networkInfo.ReceivedSpeed = (float)((stats.BytesReceived - lastReceived) / elapsedSec);
            }
            //存档，留给下一次用
            lastSent = stats.BytesSent;
            lastReceived = stats.BytesReceived;
            lastUpdate = now;
        }

        internal NetworkInfo Get()
        {
            Update();
            return networkInfo;
        }
    }
}
