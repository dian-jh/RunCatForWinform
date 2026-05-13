using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RunCatForWinform
{
    enum FPSMaxLimit
    {
        FPS40,
        FPS30,
        FPS20,
        FPS10,
    }

    internal static class FPSMaxLimitExtension
    {
        internal static string GetString(this FPSMaxLimit fpsMaxLimit)
        {
            return fpsMaxLimit switch
            {
                FPSMaxLimit.FPS40 => "40 FPS",
                FPSMaxLimit.FPS30 => "30 FPS",
                FPSMaxLimit.FPS20 => "20 FPS",
                FPSMaxLimit.FPS10 => "10 FPS",
                _ => "",
            };
        }

        internal static float GetRate(this FPSMaxLimit fPSMaxLimit)
        {
            return fPSMaxLimit switch
            {
                FPSMaxLimit.FPS40 => 1f,
                FPSMaxLimit.FPS30 => 0.75f,
                FPSMaxLimit.FPS20 => 0.5f,
                FPSMaxLimit.FPS10 => 0.25f,
                _ => 1f,
            };
        }

        internal static bool TryParse([NotNullWhen(true)] string? value, out FPSMaxLimit result)
        {
            FPSMaxLimit? nullableResult = value switch
            {
                "40fps" => FPSMaxLimit.FPS40,
                "30fps" => FPSMaxLimit.FPS30,
                "20fps" => FPSMaxLimit.FPS20,
                "10fps" => FPSMaxLimit.FPS10,
                _ => null,
            };

            if (nullableResult is FPSMaxLimit nonNullableResult)
            {
                result = nonNullableResult;
                return true;
            }
            else
            {
                result = FPSMaxLimit.FPS40;
                return false;
            }
        }
    }
}
