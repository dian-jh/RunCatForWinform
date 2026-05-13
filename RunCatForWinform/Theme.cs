using System;
using System.Collections.Generic;
using System.Text;

namespace RunCatForWinform
{
    enum  Theme
    {
        System,
        Light,
        Dark
    }

    internal static class  ThemeExtention
    {
        internal static string GetString(this Theme theme)
        {
            return theme switch
            {
                Theme.System => "System",
                Theme.Light => "Light",
                Theme.Dark => "Dark",
                _ => "",
            };
        }
    }
}
