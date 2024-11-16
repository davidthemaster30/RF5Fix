using HarmonyLib;

namespace RF5Fix;

public partial class RF5Fix
{
    [HarmonyPatch]
    public class IntroSkipPatch
    {
        // TitleMenu Skip
        // Should be okay using the update method as it's only in the title menu and shouldn't tank performance.
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.Update))]
        [HarmonyPostfix]
        public static void GetTitleState(TitleMenu __instance)
        {
            if (__instance.m_mode == TitleMenu.MODE.INIT_OP)
            {
                __instance.m_mode = TitleMenu.MODE.INIT_END_OP;
                Log.LogInfo($"Title state = {__instance.m_mode}. Skipping.");
            }

            if (__instance.m_mode == TitleMenu.MODE.SHOW_SYSTEM_INFOAUTOSAVE)
            {
                Log.LogInfo($"Title state = {__instance.m_mode}. Skipping.");
                __instance.m_mode = TitleMenu.MODE.END_SYSTEM;
            }
        }

        // Intro logos skip
        [HarmonyPatch(typeof(UILogoControl), nameof(UILogoControl.Start))]
        [HarmonyPostfix]
        public static void SkipIntroLogos(UILogoControl __instance)
        {
            __instance.m_mode = UILogoControl.MODE.END;
            Log.LogInfo("Skipped intro logos.");
        }
    }
}
