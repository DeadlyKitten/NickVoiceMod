using HarmonyLib;
using Nick;
using VoiceMod.Controllers;

namespace VoiceMod.Patches
{
    [HarmonyPatch(typeof(GameAgent), "Initiate")]
    class GameAgent_Initiate
    {
        static void Postfix(GameAgent __instance, bool recycled)
        {
            if(!__instance.IsItem && !recycled)
            {
                __instance.gameObject.AddComponent<VoiceController>();
            }
        }
    }
}
