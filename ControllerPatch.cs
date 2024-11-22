using BepInEx.Configuration;
using HarmonyLib;

namespace RF5Fix;

[HarmonyPatch]
internal static class ControllerPatch
{
    private const string ControllerSection = "Controller Icon Override";
    private const string RecommendedControllerOverrideType = "Xbox";
    internal static ConfigEntry<bool> ControllerOverride;
    internal static ConfigEntry<string> ControllerOverrideType;
    private static readonly ConfigDescription ControllerOverrideDescription = new ConfigDescription(
        "Set desired controller icon type.",
        new AcceptableValueList<string>(nameof(RF5SteamInput.SteamInputManager.ControllerType.Xbox),
                                        nameof(RF5SteamInput.SteamInputManager.ControllerType.PS4),
                                        nameof(RF5SteamInput.SteamInputManager.ControllerType.PS5),
                                        nameof(RF5SteamInput.SteamInputManager.ControllerType.Switch)));

    internal static void LoadConfig(ConfigFile Config)
    {
        ControllerOverride = Config.Bind(ControllerSection, nameof(ControllerOverride), false, "Set to true to enable controller icon override.");
        ControllerOverrideType = Config.Bind(ControllerSection, nameof(ControllerOverrideType), RecommendedControllerOverrideType, ControllerOverrideDescription); // Others are broken/invalid
    }

    // Spoof RF5's steam input controller type
    [HarmonyPatch(typeof(RF5SteamInput.SteamInputManager), nameof(RF5SteamInput.SteamInputManager.GetConnectingControllerType))]
    [HarmonyPostfix]
    internal static void Glyphy(RF5SteamInput.SteamInputManager __instance, ref RF5SteamInput.SteamInputManager.ControllerType __result)
    {
        if (ControllerOverride.Value)
        {
            __result = ControllerOverrideType.Value switch
            {
                "Xbox" => RF5SteamInput.SteamInputManager.ControllerType.Xbox, // Yes
                "PS4" => RF5SteamInput.SteamInputManager.ControllerType.PS4, // Yes
                "PS5" => RF5SteamInput.SteamInputManager.ControllerType.PS5, // Yes
                "Switch" => RF5SteamInput.SteamInputManager.ControllerType.Switch, // Yes
                "Keyboard" => RF5SteamInput.SteamInputManager.ControllerType.Keyboard, // Nope, keyboard glyphs are loaded differently.
                "Max" => RF5SteamInput.SteamInputManager.ControllerType.Max, // Broken?
                "None" => RF5SteamInput.SteamInputManager.ControllerType.None, // Broken?
                "Default" => RF5SteamInput.SteamInputManager.ControllerType.Default, // Xbox (One) Glyphs
                _ => RF5SteamInput.SteamInputManager.ControllerType.Default,
            };
        }
    }
}
