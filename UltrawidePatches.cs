using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace RF5Fix;

[HarmonyPatch]
internal static class UltrawidePatches
{
    private const string UltrawideSection = "Ultrawide UI Fixes";
    internal static ConfigEntry<bool> UltrawideFixes;
    internal static ConfigEntry<bool> Letterboxing;

    internal static void LoadConfig(ConfigFile Config)
    {
        UltrawideFixes = Config.Bind(UltrawideSection, nameof(UltrawideFixes), true, "Set to true to enable ultrawide UI fixes.");
        Letterboxing = Config.Bind(UltrawideSection, nameof(Letterboxing), true, "Letterboxes UI (not gameplay). Set to false to disable letterboxing everywhere.");
    }

    // Set screen match mode when object has canvasscaler enabled
    [HarmonyPatch(typeof(CanvasScaler), nameof(CanvasScaler.OnEnable))]
    [HarmonyPostfix]
    internal static void SetScreenMatchMode(CanvasScaler __instance)
    {
        if (UltrawideFixes.Value && !AspectRatio.ScreenRatioEqualToDefault)
        {
            __instance.m_ScreenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }
    }

    // ViewportRect
    [HarmonyPatch(typeof(ViewportRectController), nameof(ViewportRectController.OnEnable))]
    [HarmonyPatch(typeof(ViewportRectController), nameof(ViewportRectController.ResetRect))]
    [HarmonyPostfix]
    internal static void ViewportRectDisable(ViewportRectController __instance)
    {
        if (UltrawideFixes.Value)
        {
            __instance.m_Camera.rect = new Rect(0f, 0f, 1f, 1f);
            RF5FixPlugin.Log.LogInfo("Camera viewport rect patched.");
        }
    }

    // Span UI fade to black
    [HarmonyPatch(typeof(UIFadeScreen), nameof(UIFadeScreen.ScreenFade))]
    [HarmonyPostfix]
    internal static void UIFadeScreenFix(UIFadeScreen __instance)
    {
        if (UltrawideFixes.Value)
        {
            switch (AspectRatio.ScreenAspectRatio)
            {
                case < AspectRatio.DefaultAspectRatio:
                    // Increase height to scale correctly
                    __instance.BlackOutPanel.transform.localScale = new Vector3(1f, 1 * AspectRatio.AspectDivider, 1f);
                    break;
                case > AspectRatio.DefaultAspectRatio:
                    // Increase width to scale correctly
                    __instance.BlackOutPanel.transform.localScale = new Vector3(1 * AspectRatio.AspectMultiplier, 1f, 1f);
                    break;
                default:
                    return;
            }
        }
    }

    // Span UI load fade
    // Can't find a better way to hook this. It shouldn't impact performance much and even if it does it's only during UI loading fades.
    [HarmonyPatch(typeof(UILoaderFade), nameof(UILoaderFade.Update))]
    [HarmonyPostfix]
    internal static void UILoaderFadeFix(UILoaderFade __instance)
    {
        if (UltrawideFixes.Value)
        {
            switch (AspectRatio.ScreenAspectRatio)
            {
                case < AspectRatio.DefaultAspectRatio:
                    // Increase height to scale correctly
                    __instance.gameObject.transform.localScale = new Vector3(1f, 1 * AspectRatio.AspectDivider, 1f);
                    break;
                case > AspectRatio.DefaultAspectRatio:
                    // Increase width to scale correctly
                    __instance.gameObject.transform.localScale = new Vector3(1 * AspectRatio.AspectMultiplier, 1f, 1f);
                    break;
                default:
                    return;
            }
        }
    }

    // Letterbox
    [HarmonyPatch(typeof(LetterBoxController), nameof(LetterBoxController.OnEnable))]
    [HarmonyPostfix]
    internal static void LetterboxDisable(LetterBoxController __instance)
    {
        if (Letterboxing.Value)
        {
            return;
        }

        // If letterboxing is disabled
        __instance.transform.parent.gameObject.SetActive(false);
        RF5FixPlugin.Log.LogInfo("Letterboxing disabled.");
    }

    internal static GameObject letterboxing;

    // Letterbox
    [HarmonyPatch(typeof(LetterBoxController), nameof(LetterBoxController.OnEnable))]
    [HarmonyPostfix]
    internal static void LetterboxAssign(LetterBoxController __instance)
    {
        letterboxing = __instance.transform.parent.gameObject;
        RF5FixPlugin.Log.LogInfo($"Letterboxing assigned.");
    }

    // Enable Letterboxing
    [HarmonyPatch(typeof(CampMenuMain), nameof(CampMenuMain.StartCamp))] // Camp menu
    [HarmonyPatch(typeof(UILoader), nameof(UILoader.OpenCanvas))] // UILoader - Map, Calendar, Task Board, Movie Player, Shop etc
    [HarmonyPostfix]
    internal static void EnableLetterboxing()
    {
        if (Letterboxing.Value)
        {
            letterboxing.SetActive(true);
            RF5FixPlugin.Log.LogInfo("Enabled UI letterboxing.");
        }
    }

    // Disable Letterboxing
    [HarmonyPatch(typeof(GameMain), nameof(GameMain.FieldLoadStart))] // Load game
    [HarmonyPatch(typeof(CampMenuMain), nameof(CampMenuMain.CloseCamp))] // Camp menu
    [HarmonyPatch(typeof(UILoader), nameof(UILoader.DoCloseCanvas))] // UILoader - Map, Calendar, Task Board, Movie Player, Shop etc
    [HarmonyPostfix]
    internal static void DisableLetterboxing()
    {
        if (Letterboxing.Value)
        {
            letterboxing.SetActive(false);
            RF5FixPlugin.Log.LogInfo("Disabled UI letterboxing.");
        }
    }
}
