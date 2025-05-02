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

    [HarmonyPatch(typeof(ExtractionPoint), "DestroyTheFirstPhysObjectsInHaulList")]
    class ExtractionSendCheckPatch
    {

		static FieldInfo field = AccessTools.Field(typeof(RoundDirector), "totalHaul");
		static int totalHaul;

        [HarmonyPrefix]
        static bool ExtractPatch()
        {
			//Exit it not connected to an AP Server
			if (Plugin.connection == null)
            {
				return true;
            }

			totalHaul = (int)field.GetValue(RoundDirector.instance);

			//Only Run if singleplayer or host machine
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				if (RoundDirector.instance.dollarHaulList.Count == 0)
				{
					return false;
				}
				if (RoundDirector.instance.dollarHaulList[0] && RoundDirector.instance.dollarHaulList[0].GetComponent<PhysGrabObject>())
				{
					field.SetValue(RoundDirector.instance, totalHaul + (int)RoundDirector.instance.dollarHaulList[0].GetComponent<ValuableObject>().dollarValueCurrent);

					//If extracted item is a pelly, send a corresponding check
					if (RoundDirector.instance.dollarHaulList[0].name.Contains("Pelly"))
                    {
						Plugin.connection.ActivateCheck(LocationData.PellyNameToID( RoundDirector.instance.dollarHaulList[0].name + RunManager.instance.levelCurrent.name));
						APSave.AddPellyGathered(RoundDirector.instance.dollarHaulList[0].name + RunManager.instance.levelCurrent.name);
                    }


					RoundDirector.instance.dollarHaulList[0].GetComponent<PhysGrabObject>().DestroyPhysGrabObject();
					RoundDirector.instance.dollarHaulList.RemoveAt(0);
				}
			}
			return false;
		}
    }
}
