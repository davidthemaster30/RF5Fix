using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace RF5Fix;

[HarmonyPatch]
internal static class MiscellaneousPatch
{
    private const string GraphicalSection = "Graphical Tweaks";
    private const int RecommendedShadowResolution = 4096;
    private const int RecommendedShadowCascadeCount = 4;
    private const float RecommendedShadowDistance = 180f;
    private const int RecommendedAnisotropicFiltering = 16;
    private const float RecommendedLODBias = 4f;
    private const float RecommendedNPCDistance = 10000f;
    internal static ConfigEntry<int> ShadowCascadeCount;
    internal static ConfigEntry<float> ShadowDistance;
    internal static ConfigEntry<int> ShadowResolution;
    internal static ConfigEntry<int> AnisotropicFiltering;
    internal static ConfigEntry<float> LODBias;
    internal static ConfigEntry<float> NPCDistance;
    private static readonly ConfigDescription ShadowResolutionDescription = new ConfigDescription(
        "Set Shadow Resolution. 4096 is recommended for quality.",
        new AcceptableValueRange<int>(64, 32768));
    private static readonly ConfigDescription ShadowCascadeDescription = new ConfigDescription(
        "Set number of Shadow Cascades. 4 is recommended for quality but 2 is decent. Game Default = 1",
        new AcceptableValueList<int>(1, 2, 4));
    private static readonly ConfigDescription ShadowDistanceDescription = new ConfigDescription(
        "Set Shadow Distance. Controls distance at which shadows render. 180 is recommended for quality. Game Default = 120",
        new AcceptableValueRange<float>(1f, 999f));
    private static readonly ConfigDescription AnisotropicFilteringDescription = new ConfigDescription(
        "Set Anisotropic Filtering level. 16 is recommended for quality.",
        new AcceptableValueRange<int>(1, 16));
    private static readonly ConfigDescription LODBiasDescription = new ConfigDescription(
        "Set LOD Bias. Controls distance for level of detail switching. 4 is recommended for quality. Game Default = 1.5",
        new AcceptableValueRange<float>(0.1f, 10f));
    private static readonly ConfigDescription NPCDistanceDescription = new ConfigDescription(
        "Set NPC Draw Distance. Controls distance at which NPCs render. 10000 is recommended for quality. Game Default = 2028",
        new AcceptableValueRange<float>(1f, 100000f));

    private const string MouseSection = "Mouse Sensitivity";
    private const int RecommendedMouseSensitivity = 100;
    internal static ConfigEntry<bool> MouseSensitivityOverride;
    internal static ConfigEntry<int> MouseSensitivity;
    private static readonly ConfigDescription MouseSensitivityDescription = new ConfigDescription(
        "Set desired mouse sensitivity.",
        new AcceptableValueRange<int>(1, 9999));

    private const string AdvancedSection = "Advanced";
    private const int RecommendedPhysicsUpdateRate = 0;
    private const int DefaultPhysicsUpdateRate = 50;
    internal static ConfigEntry<int> PhysicsUpdateRate;
    private static readonly ConfigDescription PhysicsUpdateRateDescription = new ConfigDescription(
        $"Set desired update rate. This will improve camera smoothness in particular. {Environment.NewLine}0 = Auto (Set to refresh rate). Values under 50 are ignored. Game default = 50",
        new AcceptableValueRange<int>(0, 5000));

    internal static void LoadConfig(ConfigFile Config)
    {
        ShadowResolution = Config.Bind(GraphicalSection, nameof(ShadowResolution), RecommendedShadowResolution, ShadowResolutionDescription);
        ShadowCascadeCount = Config.Bind(GraphicalSection, nameof(ShadowCascadeCount), RecommendedShadowCascadeCount, ShadowCascadeDescription);
        ShadowDistance = Config.Bind(GraphicalSection, nameof(ShadowDistance), RecommendedShadowDistance, ShadowDistanceDescription);
        AnisotropicFiltering = Config.Bind(GraphicalSection, nameof(AnisotropicFiltering), RecommendedAnisotropicFiltering, AnisotropicFilteringDescription);
        LODBias = Config.Bind(GraphicalSection, nameof(LODBias), RecommendedLODBias, LODBiasDescription);
        NPCDistance = Config.Bind(GraphicalSection, nameof(NPCDistance), RecommendedNPCDistance, NPCDistanceDescription);

        MouseSensitivityOverride = Config.Bind(MouseSection, nameof(MouseSensitivityOverride), false, "Set to true to enable mouse sensitivity override.");
        MouseSensitivity = Config.Bind(MouseSection, nameof(MouseSensitivity), RecommendedMouseSensitivity, MouseSensitivityDescription);

        PhysicsUpdateRate = Config.Bind(AdvancedSection, nameof(PhysicsUpdateRate), RecommendedPhysicsUpdateRate, PhysicsUpdateRateDescription);
    }

    // Load game settings
    [HarmonyPatch(typeof(BootSystem), nameof(BootSystem.ApplyOption))]
    [HarmonyPostfix]
    internal static void GameSettingsOverride(BootSystem __instance)
    {
        // Anisotropic Filtering
        QualitySettings.anisotropicFiltering = UnityEngine.AnisotropicFiltering.ForceEnable;
        Texture.SetGlobalAnisotropicFilteringLimits(AnisotropicFiltering.Value, AnisotropicFiltering.Value);
        RF5FixPlugin.Log.LogInfo($"Anisotropic filtering force enabled. Value = {AnisotropicFiltering.Value}");

        // Shadow Cascades
        switch (ShadowCascadeCount.Value)
        {
            case 4:
                QualitySettings.shadowCascades = 4;
                // Need to set ShadowProjection to CloseFit or we get visual glitches at 4 cascades.
                QualitySettings.shadowProjection = ShadowProjection.CloseFit;
                RF5FixPlugin.Log.LogInfo($"Shadow Cascades set to {QualitySettings.shadowCascades}. ShadowProjection = CloseFit");
                break;
            case 2:
                QualitySettings.shadowCascades = 2;
                RF5FixPlugin.Log.LogInfo($"Shadow Cascades set to {QualitySettings.shadowCascades}");
                break;
        }

        // Shadow Distance
        QualitySettings.shadowDistance = ShadowDistance.Value;
        RF5FixPlugin.Log.LogInfo($"Shadow Distance set to {QualitySettings.shadowDistance}");

        // LOD Bias
        QualitySettings.lodBias = LODBias.Value;
        RF5FixPlugin.Log.LogInfo($"LOD Bias set to {LODBias.Value}");

        // Mouse Sensitivity
        if (MouseSensitivityOverride.Value)
        {
            BootSystem.m_Option.MouseSensitivity = MouseSensitivity.Value;
            RF5FixPlugin.Log.LogInfo($"Mouse sensitivity override. Value = {BootSystem.m_Option.MouseSensitivity}");
        }

        // NPC Distances
        NpcSetting.ShowDistance = NPCDistance.Value;
        NpcSetting.HideDistance = NPCDistance.Value;
        RF5FixPlugin.Log.LogInfo($"NPC Distance set to {NpcSetting.ShowDistance}");

        // Unity update rate
        // TODO: Replace this with camera movement interpolation?
        switch (PhysicsUpdateRate.Value) // Set update rate to screen refresh rate
        {
            case 0:
                Time.fixedDeltaTime = (float)1 / Screen.currentResolution.refreshRate;
                RF5FixPlugin.Log.LogInfo($"fixedDeltaTime set to 1 / {Screen.currentResolution.refreshRate} = {Time.fixedDeltaTime}");
                break;
            case > DefaultPhysicsUpdateRate:
                Time.fixedDeltaTime = (float)1 / PhysicsUpdateRate.Value;
                RF5FixPlugin.Log.LogInfo($"fixedDeltaTime set to 1 / {PhysicsUpdateRate.Value} = {Time.fixedDeltaTime}");
                break;
        }
    }

    // Sun & Moon | Shadow Resolution
    [HarmonyPatch(typeof(Funly.SkyStudio.OrbitingBody), nameof(Funly.SkyStudio.OrbitingBody.LayoutOribit))]
    [HarmonyPostfix]
    internal static void AdjustSunMoonLight(Funly.SkyStudio.OrbitingBody __instance)
    {
        __instance.BodyLight.shadowCustomResolution = ShadowResolution.Value;
    }

    // RealtimeBakeLight | Shadow Resolution
    [HarmonyPatch(typeof(RealtimeBakeLight), nameof(RealtimeBakeLight.Start))]
    [HarmonyPostfix]
    internal static void AdjustLightShadow(RealtimeBakeLight __instance)
    {
        __instance.Light.shadowCustomResolution = ShadowResolution.Value;
    }
}
