using System;
using System.Collections.Generic;
using System.Text;

namespace RunCatForWinform
{
    internal class CustomToolStripMenuItem : ToolStripMenuItem
    {
        #region 构造函数
        internal CustomToolStripMenuItem() : base() { }

        internal CustomToolStripMenuItem(string? text) : base(text) { }

        private CustomToolStripMenuItem(string? text, Image? image, bool isChecked, EventHandler? onClick) : base(text, image, onClick)
        {
            Checked = isChecked;
        }
        #endregion

        #region 单/多行文本绘制标志准备
        private readonly TextFormatFlags multiLineTextFlags =
            //左右内边距
            TextFormatFlags.LeftAndRightPadding |
            //垂直居中
            TextFormatFlags.VerticalCenter |
            //自动换行
            TextFormatFlags.WordBreak |
            //文本框控件行为
            TextFormatFlags.TextBoxControl;

        private readonly TextFormatFlags singleLineTextFlags =
            //左右内边距
            TextFormatFlags.LeftAndRightPadding |
            //垂直居中
            TextFormatFlags.VerticalCenter |
            //超出省略号
            TextFormatFlags.EndEllipsis;
        #endregion

        #region 自定义控件尺寸计算
        //自定义控件尺寸计算
        public override Size GetPreferredSize(Size constrainingSize)
        {
            //获取基类的建议尺寸
            Size baseSize = base.GetPreferredSize(constrainingSize);
            if (string.IsNullOrEmpty(Text))
            {
                //没有文本时，固定高度22,防止看不到菜单栏
                return new Size(baseSize.Width, 22);
            }
            //计算文本渲染的最大宽度，左右各留10像素的内边距
            var textRenderWidth = Math.Max(constrainingSize.Width - 20, 1);
            //测量文本尺寸
            // TextRenderer 是 WinForms 中用来绘制 GDI 文本的工具。
            SizeF measuredSize = TextRenderer.MeasureText(
                Text,
                Font,
                new Size(textRenderWidth, int.MaxValue),
                Flags()//获取渲染标志
            );
            //计算最终高度，向上取整并加4像素的额外空间
            var calculatedHeight = (int)Math.Ceiling(measuredSize.Height) + 4;
            // 决定最终高度
            // 如果是单行文本，就用算出来的高度。
            // 如果是多行，为了保险起见，取“基类建议高度”和“计算出的高度”中的较大值。
            var height = IsSingleLine() ? calculatedHeight : Math.Max(baseSize.Height, calculatedHeight);
            return new Size(baseSize.Width, height);
        }
        #endregion

        internal bool IsSingleLine()
        {
            return string.IsNullOrEmpty(Text) || !Text.Contains('\n');
        }

        internal TextFormatFlags Flags()
        {
            return IsSingleLine() ? singleLineTextFlags : multiLineTextFlags;
        }

        #region 动态菜单生成
        //这里有策略模式的体现,比较值得研究
        internal void SetupSubMenusFromEnum<T>(
            Func<T, string> getTitle,
            // 点击事件
            Action<CustomToolStripMenuItem, object?, EventArgs> onClick,
            Func<T, bool> isChecked
            //Func<Bitmap?> getRunnerThumbnailBitmap//获取菜单图标
        ) where T : Enum //限制泛型参数T必须是枚举类型
        {
            var items = new List<CustomToolStripMenuItem>();
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                var entityName = getTitle(value);
                //var iconImage = getRunnerThumbnailBitmap();
                //创建自定义菜单项
                var item = new CustomToolStripMenuItem(
                    entityName,
                    null,
                    isChecked(value),
                    (sender, e) => onClick(this, sender, e)
                );
                items.Add(item);
            }
            // 一次性把所有生成的子菜单加到当前菜单里
            DropDownItems.AddRange([.. items]);
        }
        #endregion
    }
}
