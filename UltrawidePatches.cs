using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

namespace RF5Fix;

public partial class RF5Fix
{
    [HarmonyPatch]
    public class UltrawidePatches
    {
        public static float DefaultAspectRatio = (float)16 / 9;
        public static float NewAspectRatio = (float)Screen.width / Screen.height;
        public static float AspectMultiplier = NewAspectRatio / DefaultAspectRatio;
        public static float AspectDivider = DefaultAspectRatio / NewAspectRatio;

        // Set screen match mode when object has canvasscaler enabled
        [HarmonyPatch(typeof(CanvasScaler), nameof(CanvasScaler.OnEnable))]
        [HarmonyPostfix]
        public static void SetScreenMatchMode(CanvasScaler __instance)
        {
            if (NewAspectRatio > DefaultAspectRatio || NewAspectRatio < DefaultAspectRatio)
            {
                __instance.m_ScreenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            }
        }

        // ViewportRect
        [HarmonyPatch(typeof(ViewportRectController), nameof(ViewportRectController.OnEnable))]
        [HarmonyPatch(typeof(ViewportRectController), nameof(ViewportRectController.ResetRect))]
        [HarmonyPostfix]
        public static void ViewportRectDisable(ViewportRectController __instance)
        {
            __instance.m_Camera.rect = new Rect(0f, 0f, 1f, 1f);
            Log.LogInfo($"Camera viewport rect patched.");
        }

        // Letterbox
        [HarmonyPatch(typeof(LetterBoxController), nameof(LetterBoxController.OnEnable))]
        [HarmonyPostfix]
        public static void LetterboxDisable(LetterBoxController __instance)
        {
            if (bLetterboxing.Value)
            {
                // Do nothing if UI letterboxing is enabled
            }
            else
            {
                // If letterboxing is disabled
                __instance.transform.parent.gameObject.SetActive(false);
                Log.LogInfo("Letterboxing disabled. For good.");
            }
        }

        // Span UI fade to black
        [HarmonyPatch(typeof(UIFadeScreen), nameof(UIFadeScreen.ScreenFade))]
        [HarmonyPostfix]
        public static void UIFadeScreenFix(UIFadeScreen __instance)
        {
            if (NewAspectRatio < DefaultAspectRatio)
            {
                // Increase height to scale correctly
                __instance.BlackOutPanel.transform.localScale = new Vector3(1f, 1 * AspectDivider, 1f);
            }
            else if (NewAspectRatio > DefaultAspectRatio)
            {
                // Increase width to scale correctly
                __instance.BlackOutPanel.transform.localScale = new Vector3(1 * AspectMultiplier, 1f, 1f);
            }
        }

        // Span UI load fade
        // Can't find a better way to hook this. It shouldn't impact performance much and even if it does it's only during UI loading fades.
        [HarmonyPatch(typeof(UILoaderFade), nameof(UILoaderFade.Update))]
        [HarmonyPostfix]
        public static void UILoaderFadeFix(UILoaderFade __instance)
        {
            if (NewAspectRatio < DefaultAspectRatio)
            {
                // Increase height to scale correctly
                __instance.gameObject.transform.localScale = new Vector3(1f, 1 * AspectDivider, 1f);
            }
            else if (NewAspectRatio > DefaultAspectRatio)
            {
                // Increase width to scale correctly
                __instance.gameObject.transform.localScale = new Vector3(1 * AspectMultiplier, 1f, 1f);
            }
        }

    }
}
