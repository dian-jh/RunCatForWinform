using RunCatForWindow;
using RunCatForWindow.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Forms.Application;

namespace RunCatForWinform
{
    internal class ContextMenuManager : IDisposable
    {
        private readonly CustomToolStripMenuItem systemInfoMenu = new();
        //系统托盘图标
        private readonly NotifyIcon notifyIcon = new();
        private readonly List<Icon> icons = [];
        private int current = 0;
        private readonly object iconLock = new();
        private EndlessGameForm? endlessGameForm;
        public ContextMenuManager(
            Func<FPSMaxLimit> getFPSMaxLimit,
            Action<FPSMaxLimit> setFPSMaxLimit,
            //Func<bool> getLaunchAtStartup,
            //Func<bool, bool> toggleLaunchAtStartup,
            Action onExit
            )
        {
            systemInfoMenu.Text = "-\n-\n-\n-\n-";
            systemInfoMenu.Enabled = false;

            var fpsMaxLimitMenu = new CustomToolStripMenuItem("FPS Max Limit");
            //动态生成子菜单
            fpsMaxLimitMenu.SetupSubMenusFromEnum<FPSMaxLimit>(
                f => f.GetString(),
                //点击事件 
                (parent, sender, e) =>
                {
                    HandleMenuItemSelection<FPSMaxLimit>(
                        parent,
                        sender,
                        (string? s, out FPSMaxLimit f) => FPSMaxLimitExtension.TryParse(s, out f),
                        f => setFPSMaxLimit(f)
                    );
                },
                f => getFPSMaxLimit() == f
            );

            var settingsMenu = new CustomToolStripMenuItem("Settings");
            settingsMenu.DropDownItems.AddRange(
                fpsMaxLimitMenu
            );

            var endlessGameMenu = new CustomToolStripMenuItem("Endless Game");
            endlessGameMenu.Click += (sender, e) => ShowOrActivateGameWindow();

            var appVersionMenu = new CustomToolStripMenuItem(
           $"{Application.ProductName} v{Application.ProductVersion}"
            )
            {
                Enabled = false
            };

            var informationMenu = new CustomToolStripMenuItem("Information");
            informationMenu.DropDownItems.AddRange(
                appVersionMenu
            );

            var exitMenu = new CustomToolStripMenuItem("Exit");
            exitMenu.Click += (sender, e) => onExit();

            var contextMenuStrip = new ContextMenuStrip(new Container());
            contextMenuStrip.Items.AddRange(
                systemInfoMenu,
                new ToolStripSeparator(),
                settingsMenu,
                informationMenu,
                endlessGameMenu,
                new ToolStripSeparator(),
                exitMenu
            );
            contextMenuStrip.Renderer = new ContextMenuRenderer();

            SetIcon();
            notifyIcon.Visible = true;
            notifyIcon.Text = "-";
            notifyIcon.Icon = icons[0];
            notifyIcon.ContextMenuStrip = contextMenuStrip;


        }

        private void ShowOrActivateGameWindow()
        {
            if (endlessGameForm is null)
            {
                endlessGameForm = new EndlessGameForm();
                endlessGameForm.FormClosed += (sender, e) =>
                {
                    endlessGameForm = null;
                };
                endlessGameForm.Show();
            }
            else
            {
                endlessGameForm.Activate();
            }
        }

        //单选按钮切换逻辑
        private static void HandleMenuItemSelection<T>(
           ToolStripMenuItem parentMenu,
           object? sender,
           CustomTryParseDelegate<T> tryParseMethod,
           Action<T> assignValueAction
       )
        {
            if (sender is null) return;
            var item = (ToolStripMenuItem)sender;
            foreach (ToolStripMenuItem childItem in parentMenu.DropDownItems)
            {
                //全部取消选中
                childItem.Checked = false;
            }
            //选中当前点击
            item.Checked = true;
            if (tryParseMethod(item.Text, out T parsedValue))
            {
                //执行-真正去修改程序的设置
                assignValueAction(parsedValue);
            }
        }

        #region 猫猫图标加载
        internal void SetIcon()
        {
            var rm = Resources.ResourceManager;
            for (int i = 0; i < 5; i++)
            {
                var iconName = $"light_cat_{i}".ToLower();
                var obj = rm.GetObject(iconName);
                if (obj is null) continue;

                if (obj is byte[] bytes)
                {
                    using var ms = new MemoryStream(bytes);
                    icons.Add(new Icon(ms));
                }
                else if (obj is Icon ico)
                {
                    icons.Add(ico);
                }
            }

        }
        #endregion

        #region 猫猫动图切换
        internal void AdvanceFrame()
        {
            lock (iconLock)
            {
                if (icons.Count == 0) return;
                if (icons.Count <= current) current = 0;
                notifyIcon.Icon = icons[current];
                current = (current + 1) % icons.Count;
            }
        }
        #endregion

        #region 资源释放
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (iconLock)
                {
                    icons.ForEach(icon => icon.Dispose());
                    icons.Clear();

                }
                if (notifyIcon is not null)
                {
                    notifyIcon.ContextMenuStrip?.Dispose();
                    notifyIcon.Dispose();
                }
            }
        }

        #region 文本设置
        internal void SetNotifyIconText(string text)
        {
            notifyIcon.Text = text;
        }

        internal void SetSystemInfoMenuText(string text)
        {
            systemInfoMenu.Text = text;
        }
        #endregion

        #endregion

        private delegate bool CustomTryParseDelegate<T>(string? value, out T result);
    }
}
