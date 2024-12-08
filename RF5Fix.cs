using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;

namespace RF5Fix;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess(GAME_PROCESS)]
public class RF5FixPlugin : BasePlugin
{
    private const string GAME_PROCESS = "Rune Factory 5.exe";

    internal static readonly new ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("RF5Fix");

    internal void LoadConfig()
    {
        GeneralPatches.LoadConfig(Config);
        MiscellaneousPatch.LoadConfig(Config);
        FOVPatch.LoadConfig(Config);
        ControllerPatch.LoadConfig(Config);
        CustomResolutionPatch.LoadConfig(Config);
        UltrawidePatches.LoadConfig(Config);
    }

    public override void Load()
    {
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} is loading!");

        LoadConfig()

        Harmony.CreateAndPatchAll(typeof(CustomResolutionPatch));
        Harmony.CreateAndPatchAll(typeof(UltrawidePatches));
        Harmony.CreateAndPatchAll(typeof(GeneralPatches.IntroSkipPatch));
        Harmony.CreateAndPatchAll(typeof(GeneralPatches.HatchingPatch));
        Harmony.CreateAndPatchAll(typeof(GeneralPatches.CampRenderTexturePatch));
        Harmony.CreateAndPatchAll(typeof(FOVPatch));
        Harmony.CreateAndPatchAll(typeof(ControllerPatch));
        Harmony.CreateAndPatchAll(typeof(MiscellaneousPatch));

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }
}
