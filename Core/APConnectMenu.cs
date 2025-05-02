using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuLib;
using MenuLib.MonoBehaviors;
using UnityEngine;
using HarmonyLib;
using MenuLib.Structs;
using System.Reflection;

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
				}, parent, new Vector2(145f, 27f));
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
			Debug.LogWarning("Building Popup");
			REPOPopupPage repoPage = MenuAPI.CreateREPOPopupPage("Archipelago", REPOPopupPage.PresetSide.Right, shouldCachePage: false, pageDimmerVisibility: true, spacing: 1.5f);
			Debug.LogWarning("1");
			repoPage.AddElement(parent => MenuAPI.CreateREPOLabel("<size=12>Only host player must be connected to AP Server.", parent, new Vector2(380f, 275f)));
			Debug.LogWarning("2");
			repoPage.AddElement(parent => MenuAPI.CreateREPOLabel(Plugin.connection.session != null ? "<size=12><color=#00ad2e>Connected" : "<size=12><color=#7a000e>Not Connected", parent, new Vector2(400f, 225f)));
			Debug.LogWarning("3");
			repoPage.AddElement(parent => MenuAPI.CreateREPOInputField("Address", (string input) =>
			{
				Plugin.apAdress = input;
			}, parent, new Vector2(400f, 200f), placeholder: Plugin.apAdress));
			Debug.LogWarning("4");
			repoPage.AddElement(parent => MenuAPI.CreateREPOInputField("Port", (string input) =>
			{
				Plugin.apPort = input;
			}, parent, new Vector2(400f, 175f), placeholder: Plugin.apPort));
			Debug.LogWarning("5");
			repoPage.AddElement(parent => MenuAPI.CreateREPOInputField("Password", (string input) =>
			{
				Plugin.apPassword = input;
			}, parent, new Vector2(400f, 150f), placeholder: Plugin.apPassword));
			Debug.LogWarning("6");
			repoPage.AddElement(parent => MenuAPI.CreateREPOInputField("Player Slot", (string input) =>
			{
				Plugin.apSlot = input;
			}, parent, new Vector2(400f, 125f), placeholder: Plugin.apSlot));
			Debug.LogWarning("7");
			repoPage.AddElement(parent => MenuAPI.CreateREPOButton("Connect", () =>
			{
				Plugin.connection.TryConnect(Plugin.apAdress, Int32.Parse(Plugin.apPort), Plugin.apPassword, Plugin.apSlot);
				repoPage.ClosePage(false);
				BuildPopup();
			}, parent, new Vector2(378f, 25f)));
			Debug.LogWarning("8");
			repoPage.AddElement(parent => MenuAPI.CreateREPOButton("Close", () =>
			{
				repoPage.ClosePage(true);
			}, parent, new Vector2(590f, 25f)));
			Debug.LogWarning("9");
			repoPage.OpenPage(true);
			Debug.LogWarning("10");
		}
	}
}
