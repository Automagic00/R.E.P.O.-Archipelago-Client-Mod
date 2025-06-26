using HarmonyLib;
using RepoAP.Core;

namespace RepoAP
{
    [HarmonyPatch(typeof(PunManager), "Start")]
    class DisplayCompletionStatusPatch
    {
        static void Postfix()
        {
            if (RunManager.instance.levelCurrent.name.Contains("Menu"))
            {
                return;
            }

            string status;
            bool complete = APSave.CheckCompletion(out status);
            HandleAPTruckScreenMessages.TruckScreenChatPatch.AddMessage("AP", status);
        }
    }
}
