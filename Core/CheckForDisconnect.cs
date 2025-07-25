﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace RepoAP
{
    [HarmonyPatch(typeof(PlayerController),"Update")]
    class CheckForDisconnect
    {
        [HarmonyPostfix]
        static void CheckDC()
        {
            //If player is in a gameplay level and not connected
            if (!RunManager.instance.levelCurrent.name.Contains("Menu") && !RunManager.instance.levelCurrent.name.Contains("Splash") && !Plugin.connection.connected)
            {
                if (Plugin.reconnectTask == null)
                {
                    Debug.Log("Disconnected from AP Server");
                    Plugin.reconnectTask = Plugin.connection.ClientDisconnected();
                }
                else if (Plugin.reconnectTask.Status == TaskStatus.RanToCompletion)
                {
                    Plugin.reconnectTask = null;
                }
            }
        }
    }

}
