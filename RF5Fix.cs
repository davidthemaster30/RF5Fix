using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;

namespace RF5Fix;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInProcess(GAME_PROCESS)]
public class RF5FixPlugin : BasePlugin
{
    #region PluginInfo
    private const string PLUGIN_GUID = "RF5Fix";
    private const string PLUGIN_NAME = "RF5Fix";
    private const string PLUGIN_VERSION = "0.2.0";
    private const string GAME_PROCESS = "Rune Factory 5.exe";
    #endregion

    internal static readonly new ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("RF5Fix");

    public override void Load()
    {
        Log.LogInfo($"Plugin {PLUGIN_NAME} is loaded!");

        GeneralPatches.LoadConfig(Config);
        MiscellaneousPatch.LoadConfig(Config);
        FOVPatch.LoadConfig(Config);
        ControllerPatch.LoadConfig(Config);
        CustomResolutionPatch.LoadConfig(Config);
        UltrawidePatches.LoadConfig(Config);

        Harmony.CreateAndPatchAll(typeof(CustomResolutionPatch));
        Harmony.CreateAndPatchAll(typeof(UltrawidePatches));
        Harmony.CreateAndPatchAll(typeof(GeneralPatches.IntroSkipPatch));
        Harmony.CreateAndPatchAll(typeof(GeneralPatches.HatchingPatch));
        Harmony.CreateAndPatchAll(typeof(GeneralPatches.CampRenderTexturePatch));
        Harmony.CreateAndPatchAll(typeof(FOVPatch));
        Harmony.CreateAndPatchAll(typeof(ControllerPatch));
        Harmony.CreateAndPatchAll(typeof(MiscellaneousPatch));
    }
}
