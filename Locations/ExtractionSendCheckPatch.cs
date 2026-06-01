using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RepoAP;
using UnityEngine;

namespace RepoAP
{
	/// <summary>
	/// Send checks to AP server based on items getting Extracted
	/// </summary>
	/// 

	class ExtractSendCheck
    {
		//static int totalHaul;
	    private static void CheckValuable(GameObject valuableObject)
	    {
            Plugin.Logger.LogDebug($"Extracting {valuableObject.name}");
            if (valuableObject && valuableObject.GetComponent<PhysGrabObject>())
            {
               //totalHaulField.SetValue(RoundDirector.instance, totalHaul + (int)valuableObject.GetComponent<ValuableObject>().dollarValueCurrent);

               //If extracted item is a pelly, send a corresponding check
               if (valuableObject.name.Contains("Pelly"))
               {
                  Plugin.connection.ActivateCheck(LocationData.PellyNameToID(valuableObject.name + RunManager.instance.levelCurrent.name));
                  APSave.AddPellyGathered(RunManager.instance.levelCurrent.name+valuableObject.name);
               }
               else if (valuableObject.name.Contains("Soul"))
               {
                  long id = LocationData.MonsterSoulNameToID(valuableObject.name);
                  if (0 != LocationData.RemoveBaseId(id))
                  {
                     Plugin.connection.ActivateCheck(id);
                     APSave.AddMonsterSoulGathered(valuableObject.name);
                  }
               }
               else if (valuableObject.name.Contains("Valuable"))
               {
                  long id = LocationData.ValuableNameToID(valuableObject.name);
                  if (0 != LocationData.RemoveBaseId(id))
                  {
                     Plugin.connection.ActivateCheck(id);
                     APSave.AddValuableGathered(valuableObject.name);
                  }

               }
            }
        }

		public static void Send()
        {
			//totalHaul = (int)totalHaulField.GetValue(RoundDirector.instance);

			//Only Run if singleplayer or host machine
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
                if (RoundDirector.instance.dollarHaulList.Count > 0)
                {
                    foreach (var valuableObject in RoundDirector.instance.dollarHaulList)
                    {
                        CheckValuable(valuableObject);
                    }
                }
            }
		}

	    public static void SendFirst()
	    {
            //totalHaul = (int)totalHaulField.GetValue(RoundDirector.instance);

            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (RoundDirector.instance.dollarHaulList.Count > 0)
                {
                    CheckValuable(RoundDirector.instance.dollarHaulList[0]);
                }
            }
        }
    }


    [HarmonyPatch(typeof(ExtractionPoint))]
    class ExtractionSendCheckPatch
    {
        [HarmonyPrefix, HarmonyPatch("DestroyAllPhysObjectsInHaulList")]
        static void ExtractAllPatch()
        {
			//Exit if not connected to an AP Server
			if (Plugin.connection == null)
            {
				return;
            }

			ExtractSendCheck.Send();
            VehicleInventoryManager.Send();

        }
		[HarmonyPrefix, HarmonyPatch("DestroyTheFirstPhysObjectsInHaulList")]
		static void ExtractFirstPatch()
		{
			//Exit if not connected to an AP Server
			if (Plugin.connection == null)
			{
				return;
			}

			ExtractSendCheck.SendFirst();
		}
        [HarmonyPostfix, HarmonyPatch("DestroyAllPhysObjectsInHaulList")]
        static void ExtractAllSyncWithClientsPatch()
        {
            //Exit if not connected to an AP Server
            if (Plugin.connection == null)
            {
                return;
            }

            Plugin.customRPCManager.CallSyncSlotDataWithClientsRpc(Plugin.customRPCManagerObject);
        }
    }
}

[HarmonyPatch(typeof(ItemValuableBox))]
class VehicleInventoryManager
{

    internal static Dictionary<ItemValuableBox, List<string>> valuableBoxContents = [];

    public static void Send()
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

        foreach (var valuableBox in RoundDirector.instance.valuableBoxHaulList)
        {
            if (valuableBoxContents.TryGetValue(valuableBox, out List<string> namesInBox))
            {
                foreach (string valuableName in namesInBox)
                {
                    CheckValuable(valuableName);
                }
                valuableBoxContents[valuableBox].Clear();
            }
        }
    }

    private static void CheckValuable(string valuableName)
    {
        Plugin.Logger.LogDebug($"Extracting {valuableName}");

        //totalHaulField.SetValue(RoundDirector.instance, totalHaul + (int)valuableObject.GetComponent<ValuableObject>().dollarValueCurrent);

        //If extracted item is a pelly, send a corresponding check
        if (valuableName.Contains("Pelly"))
        {
            Plugin.connection.ActivateCheck(LocationData.PellyNameToID(valuableName + RunManager.instance.levelCurrent.name));
            APSave.AddPellyGathered(RunManager.instance.levelCurrent.name + valuableName);
        }
        else if (valuableName.Contains("Soul"))
        {
            long id = LocationData.MonsterSoulNameToID(valuableName);
            if (0 != LocationData.RemoveBaseId(id))
            {
                Plugin.connection.ActivateCheck(id);
                APSave.AddMonsterSoulGathered(valuableName);
            }
        }
        else if (valuableName.Contains("Valuable"))
        {
            long id = LocationData.ValuableNameToID(valuableName);
            if (0 != LocationData.RemoveBaseId(id))
            {
                Plugin.connection.ActivateCheck(id);
                APSave.AddValuableGathered(valuableName);
            }

        }

    }


    [HarmonyPostfix, HarmonyPatch(nameof(ItemValuableBox.Start))]
    public static void RegisterVehicleInventory(ItemValuableBox __instance)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer() || valuableBoxContents.ContainsKey(__instance)) return;
        valuableBoxContents[__instance] = [];
    }

    [HarmonyPrefix, HarmonyPatch(nameof(ItemValuableBox.StartAbsorbLocal))]
    public static void AddValuableNameToHauler(ItemValuableBox __instance, ref PhysGrabObject target)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
        valuableBoxContents[__instance].Add(target.GetComponent<ValuableObject>().gameObject.name);
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ItemValuableBox.OnDestroy))]
    public static void RemoveValuableBoxWhenDestroyed(ItemValuableBox __instance)
    {
        valuableBoxContents.Remove(__instance);
    }
    [HarmonyPostfix, HarmonyPatch(nameof(ItemValuableBox.OnDisable))]
    public static void RemoveValuableBoxWhenDisabled(ItemValuableBox __instance)
    {
        valuableBoxContents.Remove(__instance);
    }

}