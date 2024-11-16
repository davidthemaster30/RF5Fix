using HarmonyLib;

using UnityEngine;

namespace RF5Fix;

public partial class RF5Fix
{
    [HarmonyPatch]
    public class LetterboxingPatch
    {
        public static GameObject letterboxing;

        // Letterbox
        [HarmonyPatch(typeof(LetterBoxController), nameof(LetterBoxController.OnEnable))]
        [HarmonyPostfix]
        public static void LetterboxAssign(LetterBoxController __instance)
        {
            letterboxing = __instance.transform.parent.gameObject;
            Log.LogInfo($"Letterboxing assigned.");
        }

        // Enable Letterboxing
        [HarmonyPatch(typeof(CampMenuMain), nameof(CampMenuMain.StartCamp))] // Camp menu
        [HarmonyPatch(typeof(UILoader), nameof(UILoader.OpenCanvas))] // UILoader - Map, Calendar, Task Board, Movie Player, Shop etc
        [HarmonyPostfix]
        public static void EnableLetterboxing()
        {
            letterboxing.SetActive(true);
            Log.LogInfo("Enabled UI letterboxing.");
        }

        // Disable Letterboxing
        [HarmonyPatch(typeof(GameMain), nameof(GameMain.FieldLoadStart))] // Load game
        [HarmonyPatch(typeof(CampMenuMain), nameof(CampMenuMain.CloseCamp))] // Camp menu
        [HarmonyPatch(typeof(UILoader), nameof(UILoader.DoCloseCanvas))] // UILoader - Map, Calendar, Task Board, Movie Player, Shop etc
        [HarmonyPostfix]
        public static void DisableLetterboxing()
        {
            letterboxing.SetActive(false);
            Log.LogInfo("Disabled UI letterboxing.");
        }
    }
}
