using HarmonyLib;

namespace RF5Fix;

public partial class RF5Fix
{
    [HarmonyPatch]
    public class CustomResolutionPatch
    {
        [HarmonyPatch(typeof(ScreenUtil), nameof(ScreenUtil.SetResolution), new Type[] { typeof(int), typeof(int), typeof(BootOption.WindowMode) })]
        [HarmonyPrefix]
        public static bool SetCustomRes(ref int __0, ref int __1, ref BootOption.WindowMode __2)
        {
            var fullscreenMode = iWindowMode.Value switch
            {
                1 => BootOption.WindowMode.FullScreen,
                2 => BootOption.WindowMode.Borderless,
                3 => BootOption.WindowMode.Window,
                _ => BootOption.WindowMode.FullScreen,
            };

            Log.LogInfo($"Original resolution is {__0}x{__1}. Fullscreen = {__2}.");

            __0 = (int)fDesiredResolutionX.Value;
            __1 = (int)fDesiredResolutionY.Value;
            __2 = fullscreenMode;

            Log.LogInfo($"Custom resolution set to {__0}x{__1}. Fullscreen = {__2}.");
            return true;
        }
    }
}
