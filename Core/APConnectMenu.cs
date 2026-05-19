using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MenuLib;
using MenuLib.MonoBehaviors;
using MenuLib.Structs;
using UnityEngine;

namespace RepoAP
{
	[HarmonyPatch(typeof(MainMenuOpen),"Start")]
    class APConnectMenu
    {
		[HarmonyPrefix]
        static void Prefix()
        {
			MenuAPI.AddElementToMainMenu(parent =>
			{
				//`parent` in this scenario represents the MainMenu


				//Popup Page
				//If caching is disabled then the page should be created on a button's press
				//If caching is enabled then you should assign it to a field and only create the page if the fields null, otherwise menus will duplicate over time
				//REPOPopupPage repoPage = null;
				

				//Opens the page
				//openOnTop:
				//If true, the previous page will not be set to inactive
				//If false, the previous page will be set to inactive
				//repoPage.OpenPage(openOnTop: false);
				//Sets the padding for the scroll box mask
				

				MenuAPI.CreateREPOButton("Archipelago", () => 
				{
					MenuBuilder.BuildPopup();
				}, parent, new Vector2(145f, 22f));
				//repoPage.maskPadding = new Padding(left: 0, top: 0, right: 0, bottom: 0);
				//Closes this page
				//closePagesAddedOnTop:
				//If true, all pages added on top will close too
				//If false, only this page will close

				//Sets the padding for the scroll box mask
				//Adds an element to the page
				//repoPage.AddElement(parent => MenuAPI.crea)
				
				
			});
		}
    }

	public static class MenuBuilder
    {
		public static void BuildPopup()
		{
			Plugin.Logger.LogInfo("Building Popup");
			REPOPopupPage repoPage = MenuAPI.CreateREPOPopupPage("Archipelago", REPOPopupPage.PresetSide.Right, shouldCachePage: false, pageDimmerVisibility: true, spacing: 1.5f);
			
			repoPage.AddElement(parent => MenuAPI.CreateREPOLabel("<size=12>Only host player must be connected to AP Server.", parent, new Vector2(380f, 275f)));
			
			repoPage.AddElement(parent => MenuAPI.CreateREPOLabel(Plugin.connection.connected ? "<size=12><color=#00ad2e>Connected" : "<size=12><color=#7a000e>Not Connected", parent, new Vector2(400f, 225f)));
			
			repoPage.AddElement(parent => MenuAPI.CreateREPOInputField("Address", (string input) =>
			{
				Plugin.apAddress = input;
			}, parent, new Vector2(400f, 200f), placeholder: Plugin.apAddress));

			repoPage.AddElement(parent => MenuAPI.CreateREPOInputField("Port", (string input) =>
			{
				Plugin.apPort = input;
			}, parent, new Vector2(400f, 175f), placeholder: Plugin.apPort));

			repoPage.AddElement(parent => MenuAPI.CreateREPOInputField("Password", (string input) =>
			{
				Plugin.apPassword = input;
			}, parent, new Vector2(400f, 150f), placeholder: Plugin.apPassword));

			repoPage.AddElement(parent => MenuAPI.CreateREPOInputField("Player Slot", (string input) =>
			{
				Plugin.apSlot = input;
			}, parent, new Vector2(400f, 125f), placeholder: Plugin.apSlot));

			repoPage.AddElement(parent => MenuAPI.CreateREPOButton("Connect", () =>
			{
                _ = Plugin.connection.TryConnect(Plugin.apAddress, Int32.Parse(Plugin.apPort), Plugin.apPassword, Plugin.apSlot);
				repoPage.ClosePage(false);
				BuildConnectingPopUp();
				//BuildPopup();
			}, parent, new Vector2(378f, 25f)));

			repoPage.AddElement(parent => MenuAPI.CreateREPOButton("Close", () =>
			{
				repoPage.ClosePage(true);
			}, parent, new Vector2(590f, 25f)));
#if DEBUG
			repoPage.AddElement(parent => MenuAPI.CreateREPOButton("Delete AP Data", () =>
            {
				BuildSavesManagerPopup();
            }, parent, new Vector2(424f, 25f)));
#endif
            repoPage.OpenPage(true);

        }

		public static void BuildConnectingPopUp()
        {
			REPOPopupPage repoPage = MenuAPI.CreateREPOPopupPage("Connecting to Server...", shouldCachePage: false, presetSide: REPOPopupPage.PresetSide.Right, pageDimmerVisibility: true);

			//repoPage.AddElement(parent => MenuAPI.CreateREPOLabel("<size=12>Connecting...", parent, new Vector2(10, 10)));
			Plugin.connection.connectingPage = repoPage;
			repoPage.OpenPage(true);
		}

#if DEBUG
		public static void BuildSavesManagerPopup()
		{
            REPOPopupPage saveManagerPage = MenuAPI.CreateREPOPopupPage("Save Manager", shouldCachePage: false, presetSide: REPOPopupPage.PresetSide.Right, pageDimmerVisibility: true);
			saveManagerPage.AddElement(parent => MenuAPI.CreateREPOButton("Delete Current AP Save", () =>
            {
				if (!Plugin.connection.connected) MenuManager.instance.PagePopUp("Not Connected", UnityEngine.Color.red, "Not connected to AP server. Please connect before erasing data.", "OK", true);
				else DeleteSavesPopup();
            }, parent, new Vector2(378f, 240f)));

            saveManagerPage.AddElement(parent => MenuAPI.CreateREPOButton("Delete ALL AP Saves", () =>
            {
				DeleteSavesPopup(true);
            }, parent, new Vector2(378f, 160f)));

            saveManagerPage.AddElement(parent => MenuAPI.CreateREPOButton("Close", () =>
            {
                saveManagerPage.ClosePage(true);
            }, parent, new Vector2(590f, 25f)));

            saveManagerPage.OpenPage(true);
        }

		public static void DeleteSavesPopup(bool deleteAll = false)
		{
			string resultString = "";

            REPOPopupPage warningPage = MenuAPI.CreateREPOPopupPage($"Erase {(deleteAll ? "All" : "Current")} Multiworld Data",
            shouldCachePage: false, pageDimmerVisibility: true);

            warningPage.AddElement(parent => MenuAPI.CreateREPOLabel($"<size=12>Are you SURE you want to delete local data for {(deleteAll ? "ALL multiworlds" : "the current multiworld slot")}? " +
                $"{(Plugin.connection.connected ? "Proceeding will disconnect you from the multiworld. " : "")}This action is irreversable." +
				$"{(deleteAll ? " Do not continue unless you have been told to or know what you are doing!" : "")}", parent, new Vector2(380f, 275f)));

            warningPage.AddElement(parent => MenuAPI.CreateREPOButton("Absolutely", () =>
			{
				string apSavePath = string.Concat(Application.persistentDataPath, "/archipelago/saves");

				if (!Directory.Exists(apSavePath))
				{
					resultString += "No saves to delete!";
					return;
				}

                Plugin.connection.TryDisconnect();

                if (deleteAll) 
				{ 
					string[] allSaves = Directory.GetFiles(apSavePath);
					int numSavesDeleted = 0;

					foreach (string file in allSaves)
					{
						try
						{
							File.Delete(file);
							numSavesDeleted++;
						}
						catch (Exception ex)
						{
                            Plugin.Logger.LogWarning($"Unable to delete file {file.Substring(apSavePath.Length, file.Length - apSavePath.Length - 1)}. {ex}");
						}
					}
					int numSavesRemaining = allSaves.Length - numSavesDeleted;

					if (allSaves.Length == 0) resultString += "No saves to delete!";
                    else resultString += $"Deleted: {numSavesDeleted}{(numSavesRemaining > 0 ? "\nUnable to delete: " + numSavesRemaining : "")}";
                }
				else
				{
					string filePath = string.Concat(apSavePath, APSave.fileName);
                    try
                    {
                        File.Delete(filePath);
						resultString += $"Successfully deleted {APSave.fileName}.";
                    }
                    catch (Exception ex)
                    {
                        Plugin.Logger.LogWarning($"Unable to delete file {APSave.fileName}. {ex}");
						resultString += $"Unable to delete current multiworld data.";
                    }
                }
				warningPage.ClosePage(false);
                MenuManager.instance.PagePopUp("Result", UnityEngine.Color.white, resultString, "OK", true);
            }, parent, new Vector2(378f, 25f)));

            warningPage.AddElement(parent => MenuAPI.CreateREPOButton("Maybe Not", () =>
            {
                warningPage.ClosePage(true);
            }, parent, new Vector2(590f, 25f)));

            warningPage.OpenPage(true);
        }
#endif
    }
}
