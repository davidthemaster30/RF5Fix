using HarmonyLib;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RF5Fix;

public partial class RF5Fix
{
    [HarmonyPatch]
    public class MiscellaneousPatch
    {
        public static RenderTexture rt;

        // Load game settings
        [HarmonyPatch(typeof(BootSystem), nameof(BootSystem.ApplyOption))]
        [HarmonyPostfix]
        public static void GameSettingsOverride(BootSystem __instance)
        {
            // Anisotropic Filtering
            if (iAnisotropicFiltering.Value > 0)
            {
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                Texture.SetGlobalAnisotropicFilteringLimits(iAnisotropicFiltering.Value, iAnisotropicFiltering.Value);
                Log.LogInfo($"Anisotropic filtering force enabled. Value = {iAnisotropicFiltering.Value}");
            }

            // Shadow Cascades
            if (iShadowCascades.Value == 4)
            {
                QualitySettings.shadowCascades = 4; // Default = 1
                                                    // Need to set ShadowProjection to CloseFit or we get visual glitches at 4 cascades.
                QualitySettings.shadowProjection = ShadowProjection.CloseFit; // Default = StableFit
                Log.LogInfo($"Shadow Cascades set to {QualitySettings.shadowCascades}. ShadowProjection = CloseFit");
            }
            else if (iShadowCascades.Value == 2)
            {
                QualitySettings.shadowCascades = 2; // Default = 1
                Log.LogInfo($"Shadow Cascades set to {QualitySettings.shadowCascades}");
            }

            // Shadow Distance
            if (fShadowDistance.Value >= 1f)
            {
                QualitySettings.shadowDistance = fShadowDistance.Value; // Default = 120f
                Log.LogInfo($"Shadow Distance set to {QualitySettings.shadowDistance}");
            }

            // LOD Bias
            if (fLODBias.Value >= 0.1f)
            {
                QualitySettings.lodBias = fLODBias.Value; // Default = 1.5f    
                Log.LogInfo($"LOD Bias set to {fLODBias.Value}");
            }

            // Mouse Sensitivity
            if (bMouseSensitivity.Value)
            {
                BootSystem.m_Option.MouseSensitivity = iMouseSensitivity.Value;
                Log.LogInfo($"Mouse sensitivity override. Value = {BootSystem.m_Option.MouseSensitivity}");
            }

            // NPC Distances
            if (fNPCDistance.Value >= 1f)
            {
                NpcSetting.ShowDistance = fNPCDistance.Value;
                NpcSetting.HideDistance = fNPCDistance.Value;
                Log.LogInfo($"NPC Distance set to {NpcSetting.ShowDistance}");
            }

            // Unity update rate
            // TODO: Replace this with camera movement interpolation?
            if (fUpdateRate.Value == 0) // Set update rate to screen refresh rate
            {
                Time.fixedDeltaTime = (float)1 / Screen.currentResolution.refreshRate;
                Log.LogInfo($"fixedDeltaTime set to {(float)1} / {Screen.currentResolution.refreshRate} = {Time.fixedDeltaTime}");
            }
            else if (fUpdateRate.Value > 50)
            {
                Time.fixedDeltaTime = (float)1 / fUpdateRate.Value;
                Log.LogInfo($"fixedDeltaTime set to {(float)1} / {fUpdateRate.Value} = {Time.fixedDeltaTime}");
            }

        }

        // Sun & Moon | Shadow Resolution
        [HarmonyPatch(typeof(Funly.SkyStudio.OrbitingBody), nameof(Funly.SkyStudio.OrbitingBody.LayoutOribit))]
        [HarmonyPostfix]
        public static void AdjustSunMoonLight(Funly.SkyStudio.OrbitingBody __instance)
        {
            if (iShadowResolution.Value >= 64)
            {
                __instance.BodyLight.shadowCustomResolution = iShadowResolution.Value; // Default = ShadowQuality (i.e VeryHigh = 4096)
            }
        }

        // RealtimeBakeLight | Shadow Resolution
        [HarmonyPatch(typeof(RealtimeBakeLight), nameof(RealtimeBakeLight.Start))]
        [HarmonyPostfix]
        public static void AdjustLightShadow(RealtimeBakeLight __instance)
        {
            if (iShadowResolution.Value >= 64)
            {
                __instance.Light.shadowCustomResolution = iShadowResolution.Value; // Default = ShadowQuality (i.e VeryHigh = 4096)
            }
        }

        // Fix low res render textures
        [HarmonyPatch(typeof(CampMenuMain), nameof(CampMenuMain.Start))]
        [HarmonyPatch(typeof(UIMonsterNaming), nameof(UIMonsterNaming.Start))]
        [HarmonyPostfix]
        public static void CampRenderTextureFix(CampMenuMain __instance)
        {
            if (bCampRenderTextureFix.Value)
            {
                if (!rt)
                {
                    float DefaultAspectRatio = (float)16 / 9;

                    // Render from UI camera at higher resolution and with anti-aliasing
                    float newHorizontalRes = Mathf.Floor(Screen.currentResolution.height * DefaultAspectRatio);
                    rt = new RenderTexture((int)newHorizontalRes, (int)Screen.currentResolution.height, 24, RenderTextureFormat.ARGB32);
                    rt.antiAliasing = QualitySettings.antiAliasing;

                    var UICam = UIMainManager.Instance.GetComponent<Camera>(UIMainManager.AttachId.UICamera);
                    UICam.targetTexture = rt;
                    UICam.Render();

                    Log.LogInfo($"Created new render texture for UI Camera.");
                }

                // Find raw images, even inactive ones
                // This is probably quite performance intensive.
                // There's probably a better way to do this.
                List<RawImage> rawImages = new List<RawImage>();
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if (s.isLoaded)
                    {
                        var allGameObjects = s.GetRootGameObjects();
                        for (int j = 0; j < allGameObjects.Length; j++)
                        {
                            var go = allGameObjects[j];
                            rawImages.AddRange(go.GetComponentsInChildren<RawImage>(true));
                        }
                    }
                }

                // Find RawImages that use UICameraRenderTexture
                foreach (RawImage rawImage in rawImages)
                {
                    if (rawImage.m_Texture.name == "UICameraRenderTexture")
                    {
                        rawImage.m_Texture = rt;
                        Log.LogInfo($"Set {rawImage.gameObject.GetParent().name} texture to new high-res render texture.");
                    }
                }
            }
        }

        // Disable Hatching
        [HarmonyPatch(typeof(MeshFadeController), nameof(MeshFadeController.OnEnable))]
        [HarmonyPostfix]
        public static void DisableHatching(MeshFadeController __instance)
        {
            if (bDisableCrossHatching.Value)
            {
                // This is super hacky
                var meshRenderer = __instance.Renderers[0];
                var sketchTex = meshRenderer.material.GetTexture("_SketchTex");
                sketchTex.wrapMode = TextureWrapMode.Clamp;
            }

        }
    }
}
