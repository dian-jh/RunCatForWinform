using System;
using System.Collections.Generic;
using System.Text;

namespace RunCatForWindow
{
    internal enum Road
    {
        Flat,
        Hill,
        Crater,
        Sprout
    }

    internal static class RoadExtension
    {
        internal static string GetString(this Road road)
        {
            return road switch
            {
                Road.Flat => "Flat",
                Road.Hill => "Hill",
                Road.Crater => "Crater",
                Road.Sprout => "Sprout",
                _ => "",
            };
        }
    }
}
