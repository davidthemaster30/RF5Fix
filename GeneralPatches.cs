using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RF5Fix;

internal static class GeneralPatches
{
    private const string GeneralSection = "General";
    internal static ConfigEntry<bool> IntroSkip;
    internal static ConfigEntry<bool> LowResMenuFix;
    internal static ConfigEntry<bool> DisableCrossHatching;

    internal static void LoadConfig(ConfigFile Config)
    {
        IntroSkip = Config.Bind(GeneralSection, nameof(IntroSkip), true, "Skip intro logos.");

        LowResMenuFix = Config.Bind(GeneralSection, nameof(LowResMenuFix), true, "Fixes low-resolution 3D models in the equip menu/3D model viewer.");
        DisableCrossHatching = Config.Bind(GeneralSection, nameof(DisableCrossHatching), false, "Set to true to disable the crosshatch/sketch effect.");
    }

    [HarmonyPatch]
    internal static class IntroSkipPatch
    {
        // TitleMenu Skip
        // Should be okay using the update method as it's only in the title menu and shouldn't tank performance.
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.Update))]
        [HarmonyPostfix]
        internal static void GetTitleState(TitleMenu __instance)
        {
            if (IntroSkip.Value)
            {
                if (__instance.m_mode == TitleMenu.MODE.INIT_OP)
                {
                    __instance.m_mode = TitleMenu.MODE.INIT_END_OP;
                    RF5FixPlugin.Log.LogInfo($"Title state = {__instance.m_mode}. Skipping.");
                }

                if (__instance.m_mode == TitleMenu.MODE.SHOW_SYSTEM_INFOAUTOSAVE)
                {
                    RF5FixPlugin.Log.LogInfo($"Title state = {__instance.m_mode}. Skipping.");
                    __instance.m_mode = TitleMenu.MODE.END_SYSTEM;
                }
            }
        }

        // Intro logos skip
        [HarmonyPatch(typeof(UILogoControl), nameof(UILogoControl.Start))]
        [HarmonyPostfix]
        internal static void SkipIntroLogos(UILogoControl __instance)
        {
            __instance.m_mode = UILogoControl.MODE.END;
            RF5FixPlugin.Log.LogInfo("Skipped intro logos.");
        }
    }

    [HarmonyPatch]
    internal static class HatchingPatch
    {
        // Disable Hatching
        [HarmonyPatch(typeof(MeshFadeController), nameof(MeshFadeController.OnEnable))]
        [HarmonyPostfix]
        internal static void DisableHatching(MeshFadeController __instance)
        {
            if (DisableCrossHatching.Value && __instance is not null && __instance.Renderers.Count > 0)
            {
                // This is super hacky
                var meshRenderer = __instance.Renderers[0];
                var sketchTex = meshRenderer.material.GetTexture("_SketchTex");
                if (sketchTex is not null)
                {
                    sketchTex.wrapMode = TextureWrapMode.Clamp;
                }
            }
        }
    }

    [HarmonyPatch]
    internal static class CampRenderTexturePatch
    {
        internal static RenderTexture? rt;
        // Fix low res render textures
        [HarmonyPatch(typeof(CampMenuMain), nameof(CampMenuMain.Start))]
        [HarmonyPatch(typeof(UIMonsterNaming), nameof(UIMonsterNaming.Start))]
        [HarmonyPostfix]
        internal static void CampRenderTextureFix(CampMenuMain __instance)
        {
            if (LowResMenuFix.Value)
            {
                if (rt is null)
                {
                    // Render from UI camera at higher resolution and with anti-aliasing
                    float newHorizontalRes = Mathf.Floor(Screen.currentResolution.height * AspectRatio.DefaultAspectRatio);
                    rt = new RenderTexture((int)newHorizontalRes, Screen.currentResolution.height, 24, RenderTextureFormat.ARGB32);
                    rt.antiAliasing = QualitySettings.antiAliasing;

                    var UICam = UIMainManager.Instance.GetComponent<Camera>(UIMainManager.AttachId.UICamera);
                    UICam.targetTexture = rt;
                    UICam.Render();

                    RF5FixPlugin.Log.LogInfo("Created new render texture for UI Camera.");
                }

                // Find raw images, even inactive ones
                // This is probably quite performance intensive.
                // There's probably a better way to do this.
                List<RawImage> rawImages = [];
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if (s.isLoaded)
                    {
                        var allGameObjects = s.GetRootGameObjects();
                        for (int j = 0; j < allGameObjects.Length; j++)
                        {
                            var gameObjects = allGameObjects[j];
                            // Find RawImages that use UICameraRenderTexture
                            rawImages.AddRange(gameObjects.GetComponentsInChildren<RawImage>(true).Where(rawImage => rawImage.m_Texture.name == "UICameraRenderTexture"));
                        }
                    }
                }

                foreach (RawImage rawImage in rawImages)
                {
                    rawImage.m_Texture = rt;
                    RF5FixPlugin.Log.LogInfo($"Set {rawImage.gameObject.GetParent().name} texture to new high-res render texture.");
                }
            }
        }
    }
}