using HarmonyLib;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace RepoAP.Core
{
    public class HandleAPTruckScreenMessages
    {
        // MessageSendCustomRPC only allows nicknames of active players
        // This is a workaround where we replace the non-const string that
        // defines the Taxman's own nickname in chat with whatever we want
        // The string is reset in the postfix
        [HarmonyPatch(typeof(TruckScreenText), "MessageSendCustomRPC")]
        public class OverridePlayerNameCheckPatch
        {
            static string nicknameCacheman = "";
            static string customFormattedNickname = "";
            static void Prefix(TruckScreenText __instance)
            {
                if (customFormattedNickname != string.Empty)
                {
                    FieldInfo taxmanNameField = AccessTools.Field(typeof(TruckScreenText), "nicknameTaxman");
                    nicknameCacheman = (string)taxmanNameField.GetValue(__instance);

                    taxmanNameField.SetValue(__instance, customFormattedNickname);
                }
            }

            static void Postfix(TruckScreenText __instance)
            {
                if (nicknameCacheman != string.Empty)
                {
                    AccessTools.Field(typeof(TruckScreenText), "nicknameTaxman").SetValue(__instance, nicknameCacheman);
                }
            }

            static public void SetNickname(string nickname, string color)
            {
                customFormattedNickname = "\n\n<color=#" + color + "><b>" + nickname + ":</b></color>\n";
            }

            static public void SetFormattedNickname(string nicknameFormatted)
            {
                customFormattedNickname = $"\n\n{nicknameFormatted}\n";
            }

            static public void ResetNickname()
            {
                customFormattedNickname = string.Empty;
            }
        }

        // This class actually sends the messages
        [HarmonyPatch(typeof(TruckScreenText), "Update")]
        public class TruckScreenChatPatch
        {
            const string nickAP = "<b><color=#c97682>AR</color><color=#75c275>CH</color><color=#ca94c2>IP</color><color=#d9a07d>EL</color><color=#767ebd>AG</color><color=#eee391>O:</color></b>";

            // define static queue of incoming messages (string,string)
            private struct messageData
            {
                public messageData(string nick, string msg)
                {
                    message = msg;
                    nickname = nick;
                }
                public string message { get; set; }
                public string nickname { get; set; }
            }

            static private ConcurrentQueue<messageData> messageQueue = new ConcurrentQueue<messageData>();

            static void Prefix(TruckScreenText __instance)
            {
                if (Plugin.connection != null)
                {
                    var currentLineIndex = (int)AccessTools.Field(typeof(TruckScreenText), "currentLineIndex").GetValue(__instance);
                    var currentPageIndex = (int)AccessTools.Field(typeof(TruckScreenText), "currentPageIndex").GetValue(__instance);

                    if (currentLineIndex >= __instance.pages[currentPageIndex].textLines.Count)
                    {
                        if(messageQueue.TryDequeue(out var nextMessage))
                        {
                            OverridePlayerNameCheckPatch.SetFormattedNickname(nextMessage.nickname);
                            __instance.MessageSendCustom(String.Empty, nextMessage.message, 0);
                            OverridePlayerNameCheckPatch.ResetNickname();
                        }
                    }
                }
            }

            static public void AddMessage(string preformattedNickname, string message)
            {
                string nick;
                if (preformattedNickname == "AP") nick = nickAP;
                else nick = $"\n\n{preformattedNickname}\n";

                messageData md = new messageData(nick, message);
                messageQueue.Enqueue(md);
            }

            static public void AddMessage(string nickname, string hexColor, string message)
            {
                string nick;
                if (nickname == "AP") nick = nickAP;
                else nick = "\n\n<color=#" + hexColor + "><b>" + nickname + ":</b></color>\n";

                messageData md = new messageData(nick, message);
                messageQueue.Enqueue(md);
            }
        }
    }
}
