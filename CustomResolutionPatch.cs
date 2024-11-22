using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace RF5Fix;

[HarmonyPatch]
internal static class CustomResolutionPatch
{
    private const string CustomResolutionSection = "Custom Resolution";
    internal static ConfigEntry<int> ResolutionWidth;
    internal static ConfigEntry<int> ResolutionHeight;
    internal static ConfigEntry<int> WindowMode;
    internal static ConfigEntry<bool> CustomResolutionOverride;
    private static readonly ConfigDescription WindowModeDescription = new ConfigDescription(
        $"Set window mode. {BootOption.WindowMode.Window} = Windowed, {BootOption.WindowMode.FullScreen} = Fullscreen, {BootOption.WindowMode.Borderless} = Borderless.",
        new AcceptableValueList<int>((int)BootOption.WindowMode.Window, (int)BootOption.WindowMode.FullScreen, (int)BootOption.WindowMode.Borderless));

    internal static void LoadConfig(ConfigFile Config)
    {
        CustomResolutionOverride = Config.Bind(CustomResolutionSection, nameof(CustomResolutionOverride),
                            false, // Disable by default as launcher should suffice.
                            "Set to true to enable the custom resolution below.");

        ResolutionWidth = Config.Bind(CustomResolutionSection, nameof(ResolutionWidth),
                            Display.main.systemWidth, // Set default to display width so we don't leave an unsupported resolution as default.
                            "Set desired resolution width.");

        ResolutionHeight = Config.Bind(CustomResolutionSection, nameof(ResolutionHeight),
                            Display.main.systemHeight, // Set default to display height so we don't leave an unsupported resolution as default.
                            "Set desired resolution height.");

        WindowMode = Config.Bind(CustomResolutionSection, nameof(WindowMode), 1, WindowModeDescription);
    }

    [HarmonyPatch(typeof(ScreenUtil), nameof(ScreenUtil.SetResolution))]
    [HarmonyPrefix]
    internal static bool SetCustomRes(ref int __0, ref int __1, ref BootOption.WindowMode __2)
    {
        if (CustomResolutionOverride.Value)
        {
            RF5FixPlugin.Log.LogInfo($"Original resolution is {__0}x{__1}. Fullscreen = {__2}.");

            __0 = ResolutionWidth.Value;
            __1 = ResolutionHeight.Value;
            __2 = (BootOption.WindowMode)WindowMode.Value;

            RF5FixPlugin.Log.LogInfo($"Custom resolution set to {__0}x{__1}. Fullscreen = {__2}.");
        }
        return true;
    }
}
