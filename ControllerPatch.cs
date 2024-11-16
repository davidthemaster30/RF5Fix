using HarmonyLib;

namespace RF5Fix;

public partial class RF5Fix
{
    [HarmonyPatch]
    public class ControllerPatch
    {
        // Spoof RF5's steam input controller type
        [HarmonyPatch(typeof(RF5SteamInput.SteamInputManager), nameof(RF5SteamInput.SteamInputManager.GetConnectingControllerType))]
        [HarmonyPostfix]
        public static void Glyphy(RF5SteamInput.SteamInputManager __instance, ref RF5SteamInput.SteamInputManager.ControllerType __result)
        {
            var controllerType = sControllerType.Value switch
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

            __result = controllerType;
        }
    }
}
