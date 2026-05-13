using System;
using System.Collections.Generic;
using System.Text;

namespace RunCatForWinform
{
    internal class ContextMenuRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Text) && e.Item is CustomToolStripMenuItem item)
            {
                var textRectangle = e.TextRectangle;
                textRectangle.Height = item.Bounds.Height;
                TextRenderer.DrawText(
                    e.Graphics,
                    e.Text,
                    e.TextFont,
                    textRectangle,
                    item.ForeColor,
                    item.Flags()
                );
            }
            else
            {
                //基类方法
                base.OnRenderItemText(e);
            }
        }
    }
}
