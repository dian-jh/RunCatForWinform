using RunCatForWindow.Properties;
using RunCatForWinform;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using FormsTimer = System.Windows.Forms.Timer;

namespace RunCatForWindow
{
    public partial class EndlessGameForm : Form
    {
        private const int JUMP_THREDHOLD = 17;
        private readonly FormsTimer timer;
        private GameStatus status = GameStatus.NewGame;
        private Cat cat = new Cat.Running(Cat.Running.Frame.Frame0);
        private readonly List<Road> roads = [];
        private int counter = 0;
        private int limit = 5;
        private int score = 0;
        private bool isJumpRequested = false;
        private readonly bool isAutoPlay = false;

        public EndlessGameForm()
        {
            //减少闪烁，让动画平滑一点
            DoubleBuffered = true;
            //窗口的“内部可用区域”大小
            ClientSize = new Size(600, 250);
            FormBorderStyle = FormBorderStyle.FixedDialog;//禁止调整窗口大小
            MaximizeBox = false;//进入最大化
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Endless Game";
            Icon = Resources.app_icon;
            BackColor = Color.Gainsboro;
            //窗口需要重绘时被调用的核心事件
            Paint += RenderScene;

            KeyDown += HandleKeyDown;

            timer = new FormsTimer
            {
                Interval = 80//每 100 毫秒触发一次 Tick
            };
            timer.Tick += GameTick;//游戏逻辑主循环

            Initialize();

            timer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            timer.Stop();
            timer.Dispose();
        }

        private void Initialize()
        {
            counter = JUMP_THREDHOLD;
            isJumpRequested = false;
            score = 0;
            cat = new Cat.Running(Cat.Running.Frame.Frame0);
            roads.RemoveAll(r => r == Road.Sprout);
            Enumerable.Range(0, 20 - roads.Count).ToList().ForEach(
                _ => roads.Add((Road)(new Random().Next(0, 3)))
            );
        }

        //碰撞检测和状态判定
        private bool Judge()
        {
            if (status != GameStatus.Playing) return false;
            //寻找障碍物坐标
            var sproutIndices = roads
                .Select((road, index) => { return road == Road.Sprout ? index : (int?)null; })
                .OfType<int>()
                .ToList();
            //碰撞检测，跟猫占的坐标进行交集检测，有交集就碰撞
            if (cat.ViolationIndices().HasCommonElements(sproutIndices))
            {
                status = GameStatus.GameOver;
                return false;
            }
            else
            {
                return true;
            }
        }

        private void UpdateRoads()
        {
            var firstRoad = roads.First();
            roads.RemoveAt(0);
            if (firstRoad == Road.Sprout)
            {
                score += 1;
            }
            counter = counter > 0 ? counter - 1 : limit - 1;//冷却机制
            if (counter == 0)//倒计时结束，触发一次生成事件
            {
                //3、9、27整除都增加一个地刺，也就是最多三个连续的地刺
                var randomValue = new Random().Next(0, 27);
                var subRoads = new List<Road>();
                if (randomValue % 3 == 0)
                {
                    subRoads.Add(Road.Sprout);
                }
                if (randomValue % 9 == 0)
                {
                    subRoads.Add(Road.Sprout);
                }
                if (randomValue % 27 == 0)
                {
                    subRoads.Add(Road.Sprout);
                }
                roads.AddRange(subRoads);
                //动态难度调整：如果刚刚生成了障碍物，休息时间(limit)设为 5，否则设为 10
                limit = subRoads.Count == 0 ? 5 : 10;
            }
            if (roads.Count < 20)//保证屏幕右边永远有路
            {
                roads.Add((Road)(new Random().Next(0, 3)));//随机生成平地或小坡
            }
        }

        private void UpdateCat()
        {
            if (cat is Cat.Running runningCat)
            {
                //跑到了第四帧并且按了空格
                if (runningCat.CurrentFrame == Cat.Running.Frame.Frame4 && isJumpRequested)
                {
                    cat = new Cat.Jumping(Cat.Jumping.Frame.Frame0);
                    isJumpRequested = false;
                    return;
                }
            }
            else if (cat is Cat.Jumping jumpingCat)
            {
                //跳到了最后一帧
                if (jumpingCat.CurrentFrame == Cat.Jumping.Frame.Frame9)
                {
                    if (isJumpRequested)
                    {
                        cat = cat.Next();
                        isJumpRequested = false;
                        return;
                    }
                    else
                    {
                        cat = new Cat.Running(Cat.Running.Frame.Frame0);
                        return;
                    }
                }
            }
            cat = cat.Next();//普通一帧，就正常播放下一帧
        }

        private void AutoJump()
        {
            if (isAutoPlay && roads[JUMP_THREDHOLD - 1] == Road.Sprout)
            {
                isJumpRequested = true;
            }
        }

        private void GameTick(object? sender, EventArgs e)
        {
            //如果游戏没结束且没有撞死
            if (Judge())
            {
                UpdateRoads();//地形生成
                UpdateCat();
                AutoJump(); //自动挂机逻辑
            }
            Invalidate();//强制重绘
        }

        private void HandleKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                switch (status)
                {
                    case GameStatus.NewGame:
                    case GameStatus.GameOver:
                        Initialize();
                        status = GameStatus.Playing;
                        break;
                    case GameStatus.Playing when !isAutoPlay:
                        isJumpRequested = true;
                        break;
                    default:
                        break;
                }
            }
        }

        //private void InitializeComponent()
        //{

        //}

        private void RenderScene(object? sender, PaintEventArgs e)
        {
            var rm = Resources.ResourceManager;//获取资源管理器，用于后续从 Properties.Resources 里拿图片
            var prefix = "light";
            var textColor = Color.Black;
            var g = e.Graphics;//获取画布对象
            //using 创建字体和画笔
            using (Font font15 = new("Courier New", 15))
            using (Brush brush = new SolidBrush(textColor))
            {
                var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Far,
                    LineAlignment = StringAlignment.Center
                };
                //绘制指定矩形区域内画出分数
                g.DrawString($"Score: {score}", font15, brush, new Rectangle(20, 0, 560, 50), stringFormat);
            }
            //每次只取前20个路块
            roads.Take(20).Select((road, index) => new { road, index }).ToList().ForEach(
                item =>
                {
                    var fileName = $"{prefix}_road_{item.road.GetString()}".ToLower();
                    using Bitmap? image = rm.GetObject(fileName) as Bitmap;
                    if (image is null) return;
                    g.DrawImage(image, new Rectangle(item.index * 30, 200, 30, 50));
                }
            );
            //绘制猫咪
            var fileName = $"{prefix}_cat_{cat.GetString()}".ToLower();
            using Bitmap? image = rm.GetObject(fileName) as Bitmap;
            if (image is null) return;
            g.DrawImage(image, new Rectangle(120, 130, 120, 100));//固定位置画猫

            if (status != GameStatus.Playing)
            {
                //半透明画笔
                using Brush fillBrush = new SolidBrush(Color.FromArgb(77, 0, 0, 0));
                g.FillRectangle(fillBrush, new Rectangle(0, 0, 600, 250));//半透明填满整个屏幕
                //准备大体字号
                using Font font18 = new("Courier New", 18, FontStyle.Bold);
                using Brush brush = new SolidBrush(textColor);
                var message = "Press space to play.";
                if (status == GameStatus.GameOver)
                {
                    message = "GAME OVER\n\n" + message;
                }
                var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(message, font18, brush, new Rectangle(0, 0, 600, 250), stringFormat);
            }
        }
    }

    internal static class ListExtension
    {
        internal static bool HasCommonElements(this List<int> list1, List<int> list2)
        {
            return list1.Intersect(list2).Any();
        }
    }
}
