using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace RF5Fix;

[HarmonyPatch]
internal static class FOVPatch
{
    private const string FOVSection = "FOV Adjustment";
    private const int MaxDegrees = 360;
    public static ConfigEntry<bool> FOVOverride;
    public static ConfigEntry<float> AdditionalFOV;
    private static bool farmFOVHasRun = false;
    private static bool trackingFOVHasRun = false;

    private static float OriginalInDoorFov = 0f;
    private static float OriginalOutDoorminFov = 0f;
    private static float OriginalDangeonminFov = 0f;

    private static readonly ConfigDescription AdditionalFOVDescription = new ConfigDescription(
        "Set additional FOV in degrees. This does not adjust FOV in cutscenes.",
        new AcceptableValueRange<float>(0f, 90f));

    public static void LoadConfig(ConfigFile Config)
    {
        // True by default to enable Vert+ for narrow aspect ratios.
        FOVOverride = Config.Bind(FOVSection, nameof(FOVOverride), true, $"Set to true to enable adjustment of the FOV. {Environment.NewLine}It will also adjust the FOV to be Vert+ if your aspect ratio is narrower than 16:9.");
        FOVOverride.SettingChanged += OnSettingChanged;
        AdditionalFOV = Config.Bind(FOVSection, nameof(AdditionalFOV), 0f, AdditionalFOVDescription);
        AdditionalFOV.SettingChanged += OnSettingChanged;
    }

    internal static void OnSettingChanged(object sender, EventArgs e)
    {
        var eventArgs = e as SettingChangedEventArgs;
        RF5FixPlugin.Log.LogInfo($"{eventArgs.ChangedSetting.Definition} Setting has changed.");

        farmFOVHasRun = false;
        trackingFOVHasRun = false;
    }

    // Adjust tracking camera FOV
    // Indoor, outdoor, dungeon
    [HarmonyPatch(typeof(PlayerTrackingCamera), nameof(PlayerTrackingCamera.Start))]
    [HarmonyPostfix]
    internal static void TrackingFOV(PlayerTrackingCamera __instance)
    {
        // Only run this once
        if (FOVOverride.Value && !trackingFOVHasRun)
        {
            var InDoor = __instance.GetSetting(Define.TrackinCameraType.InDoor);
            var OutDoor = __instance.GetSetting(Define.TrackinCameraType.OutDoor);
            var Dangeon = __instance.GetSetting(Define.TrackinCameraType.Dangeon);

            if(OriginalInDoorFov == 0f || OriginalOutDoorminFov == 0f || OriginalDangeonminFov == 0f){
                OriginalInDoorFov = InDoor.minFov;
                OriginalOutDoorminFov = OutDoor.minFov;
                OriginalDangeonminFov = Dangeon.minFov;
            }

            RF5FixPlugin.Log.LogInfo($"Tracking Camera. Current InDoor FOV = {InDoor.minFov}. Current OutDoor FOV = {OutDoor.minFov}. Current Dangeon FOV = {Dangeon.minFov}");

            // Vert+ FOV
            if (AspectRatio.ScreenRatioIsSmallerThenDefault)
            {
                InDoor.minFov = Mathf.Floor(Mathf.Atan(Mathf.Tan(OriginalInDoorFov * Mathf.PI / MaxDegrees) / AspectRatio.ScreenAspectRatio * AspectRatio.DefaultAspectRatio) * MaxDegrees / Mathf.PI);
                OutDoor.minFov = Mathf.Floor(Mathf.Atan(Mathf.Tan(OriginalOutDoorminFov* Mathf.PI / MaxDegrees) / AspectRatio.ScreenAspectRatio * AspectRatio.DefaultAspectRatio) * MaxDegrees / Mathf.PI);
                Dangeon.minFov = Mathf.Floor(Mathf.Atan(Mathf.Tan(OriginalDangeonminFov * Mathf.PI / MaxDegrees) / AspectRatio.ScreenAspectRatio * AspectRatio.DefaultAspectRatio) * MaxDegrees / Mathf.PI);
            }
            // Add FOV
            if (AdditionalFOV.Value > 0f)
            {
                InDoor.minFov += AdditionalFOV.Value;
                OutDoor.minFov += AdditionalFOV.Value;
                Dangeon.minFov += AdditionalFOV.Value;
            }

            RF5FixPlugin.Log.LogInfo($"Tracking Camera: New InDoor FOV = {InDoor.minFov}. New OutDoor FOV = {OutDoor.minFov}. New Dangeon FOV = {Dangeon.minFov}");
            trackingFOVHasRun = true;
        }
    }

    // Farming FOV
    [HarmonyPatch(typeof(PlayerFarmingCamera), nameof(PlayerFarmingCamera.Awake))]
    [HarmonyPrefix]
    internal static void FarmingFOV(PlayerFarmingCamera __instance)
    {
        // Only run this once
        if (FOVOverride.Value && !farmFOVHasRun)
        {
            var battleInst = BattleConst.Instance;
            RF5FixPlugin.Log.LogInfo($"PlayerFarmingCamera: Current FOV = {battleInst.FarmCamera_FOV}.");

            // Vert+ FOV
            if (AspectRatio.ScreenRatioIsSmallerThenDefault)
            {
                battleInst.FarmCamera_FOV = Mathf.Floor(Mathf.Atan(Mathf.Tan(battleInst.FarmCamera_FOV * Mathf.PI / MaxDegrees) / AspectRatio.ScreenAspectRatio * AspectRatio.DefaultAspectRatio) * MaxDegrees / Mathf.PI);
            }

            // Add FOV
            if (AdditionalFOV.Value > 0f)
            {
                battleInst.FarmCamera_FOV += AdditionalFOV.Value;
            }

            RF5FixPlugin.Log.LogInfo($"PlayerFarmingCamera: New FOV = {battleInst.FarmCamera_FOV}.");
            farmFOVHasRun = true;
        }
    }

    // FOV adjustment for every camera
    // Does not effect tracking camera and farm camera, maybe more. They are forced to a specific FOV
    // Adjust to Vert+ at narrower than 16:9
    [HarmonyPatch(typeof(Cinemachine.CinemachineVirtualCamera), nameof(Cinemachine.CinemachineVirtualCamera.OnEnable))]
    [HarmonyPostfix]
    internal static void GlobalFOV(Cinemachine.CinemachineVirtualCamera __instance)
    {
        if (FOVOverride.Value && !farmFOVHasRun)
        {
            var currLens = __instance.m_Lens;
            var currFOV = currLens.FieldOfView;

            RF5FixPlugin.Log.LogInfo($"Cinemachine VCam: Current camera name = {__instance.name}.");
            RF5FixPlugin.Log.LogInfo($"Cinemachine VCam: Current camera FOV = {currFOV}.");

            // Vert+ FOV
            if (AspectRatio.ScreenRatioIsSmallerThenDefault)
            {
                float newFOV = Mathf.Floor(Mathf.Atan(Mathf.Tan(currFOV * Mathf.PI / MaxDegrees) / AspectRatio.ScreenAspectRatio * AspectRatio.DefaultAspectRatio) * MaxDegrees / Mathf.PI);
                currLens.FieldOfView = Mathf.Clamp(newFOV, 1f, 180f);
            }

            // Add FOV for everything but cutscenes
            if (AdditionalFOV.Value > 0f && __instance.name != "CMvcamCutBuffer" && __instance.name != "CMvcamShortPlay")
            {
                currLens.FieldOfView += AdditionalFOV.Value;
                RF5FixPlugin.Log.LogInfo($"Cinemachine VCam: Cam name = {__instance.name}. Added gameplay FOV = {AdditionalFOV.Value}.");
            }

            __instance.m_Lens = currLens;
            RF5FixPlugin.Log.LogInfo($"Cinemachine VCam: New camera FOV = {__instance.m_Lens.FieldOfView}.");
        }
    }
}
