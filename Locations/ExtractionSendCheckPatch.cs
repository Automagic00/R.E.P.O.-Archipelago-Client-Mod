using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace RepoAP
{
	/// <summary>
	/// Send checks to AP server based on items getting Extracted
	/// </summary>
	/// 

	class ExtractSendCheck
    {
		static int totalHaul;
		public static void Send(FieldInfo totalHaulField)
        {
			totalHaul = (int)totalHaulField.GetValue(RoundDirector.instance);

			//Only Run if singleplayer or host machine
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				if (RoundDirector.instance.dollarHaulList.Count == 0)
				{
					return;
				}
				foreach (var valuableObject in RoundDirector.instance.dollarHaulList)
				{
					Debug.Log($"Extracting {valuableObject.name}");
					if (valuableObject && valuableObject.GetComponent<PhysGrabObject>())
					{
						totalHaulField.SetValue(RoundDirector.instance, totalHaul + (int)valuableObject.GetComponent<ValuableObject>().dollarValueCurrent);

						//If extracted item is a pelly, send a corresponding check
						if (valuableObject.name.Contains("Pelly"))
						{
							Plugin.connection.ActivateCheck(LocationData.PellyNameToID(valuableObject.name + RunManager.instance.levelCurrent.name));
							APSave.AddPellyGathered(valuableObject.name + RunManager.instance.levelCurrent.name);
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
			}
		}
    }


    [HarmonyPatch(typeof(ExtractionPoint))]
    class ExtractionSendCheckPatch
    {

		static FieldInfo field = AccessTools.Field(typeof(RoundDirector), "totalHaul");
		static int totalHaul;

        [HarmonyPrefix, HarmonyPatch("DestroyAllPhysObjectsInHaulList")]
        static void ExtractAllPatch()
        {
			//Exit it not connected to an AP Server
			if (Plugin.connection == null)
            {
				return;
            }

			ExtractSendCheck.Send(field);
		}
		[HarmonyPrefix, HarmonyPatch("DestroyTheFirstPhysObjectsInHaulList")]
		static void ExtractFirstPatch()
		{
			//Exit it not connected to an AP Server
			if (Plugin.connection == null)
			{
				return;
			}

			ExtractSendCheck.Send(field);
		}
	}
}
