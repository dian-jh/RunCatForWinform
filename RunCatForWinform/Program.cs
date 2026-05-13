using RunCatForWindow.Properties;
using System;
using FormsTimer = System.Windows.Forms.Timer;
namespace RunCatForWinform
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using var mutex = new Mutex(true, "_RUNCAT_MUTEX", out var result);
            if (!result) return;
            try
            {
                ApplicationConfiguration.Initialize();
                Application.SetColorMode(SystemColorMode.System);
                Application.Run(new RunCatApplicationContext());
            }
            finally
            {

                mutex?.ReleaseMutex();
            }

        }
    }

    internal class RunCatApplicationContext : ApplicationContext
    {
        #region 变量定义
        //采集定时器，读取系统数据比较耗费资源，不能太快
        private const int FETCH_TIMER_DEFAULT_INTERVAL = 1000;
        private const int FETCH_COUNTER_SIZE = 5;
        //动画定时器，控制猫猫动图切换速度，要跑得快动画才流畅
        private const int ANIMATE_TIMER_DEFAULT_INTERVAL = 200;
        private readonly ContextMenuManager contextMenuManager;
        private Theme manualTheme = Theme.System;
        private FPSMaxLimit fpsMaxLimit = FPSMaxLimit.FPS40;
        private int fetchCounter = 5;
        private readonly FormsTimer fetchTimer;
        private readonly FormsTimer animateTimer;

        private readonly CPURepository cpuRepository;
        private readonly MemoryRepository memRepository;
        private readonly StorageRepository storageRepository;
        private readonly NetworkRepository networkRepository;
        #endregion

        #region 初始化
        public RunCatApplicationContext()
        {
            cpuRepository = new CPURepository();
            memRepository = new MemoryRepository();
            storageRepository = new StorageRepository();
            networkRepository = new NetworkRepository();
            contextMenuManager = new ContextMenuManager(
                () => fpsMaxLimit,
                f => ChangeFPSMaxLimit(f),
                //() => launchAtStartupManager.GetStartup(),
                //s => launchAtStartupManager.SetStartup(s),
                () => Application.Exit()
                );
            //定时器
            animateTimer = new FormsTimer
            {
                Interval = ANIMATE_TIMER_DEFAULT_INTERVAL
            };
            animateTimer.Tick += new EventHandler(AnimationTick);
            animateTimer.Start();

            fetchTimer = new FormsTimer
            {
                Interval = FETCH_TIMER_DEFAULT_INTERVAL
            };
            fetchTimer.Tick += new EventHandler(FetchTick);
            fetchTimer.Start();
        }

        private void ChangeFPSMaxLimit(FPSMaxLimit f)
        {
            fpsMaxLimit = f;
            UserSettings.Default.FPSMaxLimit = fpsMaxLimit.ToString();
            UserSettings.Default.Save();
        }
        #endregion

        #region 定时器
        private void FetchTick(object? sender, EventArgs e)
        {
            cpuRepository.Update();

            fetchCounter += 1;
            if (fetchCounter < FETCH_COUNTER_SIZE) return;
            fetchCounter = 0;

            var cpuInfo = cpuRepository.Get();
            var memInfo = memRepository.Get();
            var storageInfo = storageRepository.Get();
            var networkInfo = networkRepository.Get();
            FetchSystemInfo(cpuInfo, memInfo, storageInfo, networkInfo);

            animateTimer.Stop();
            animateTimer.Interval = CalculateInterval(cpuInfo.Total);
            animateTimer.Start();

        }


        private void AnimationTick(object? sender, EventArgs e)
        {
            contextMenuManager.AdvanceFrame();
        }
        #endregion

        private void FetchSystemInfo(
            CPUInfo cpuInfo,
            MemoryInfo memoryInfo,
            List<StorageInfo> storageValue,
            NetworkInfo networkInfo
            )
        {
            contextMenuManager.SetNotifyIconText(cpuInfo.GetDescription());

            var systemInfoValues = new List<string>();
            systemInfoValues.AddRange(cpuInfo.GenerateIndicator());
            systemInfoValues.AddRange(memoryInfo.GenerateIndicator());
            systemInfoValues.AddRange(storageValue.GenerateIndicator());
            systemInfoValues.AddRange(networkInfo.GenerateIndicator());
            contextMenuManager.SetSystemInfoMenuText(string.Join("\n", [.. systemInfoValues]));
        }

        #region 动画间隔计算
        private int CalculateInterval(float cpuTotalValue)
        {
            var speed = (float)Math.Max(1.0f, (cpuTotalValue / 5.0f) * fpsMaxLimit.GetRate());
            return (int)(500.0f / speed);
        }
        #endregion
    }
}