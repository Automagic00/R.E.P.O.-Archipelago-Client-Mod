using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace RepoAP.Core
{
    [HarmonyPatch(typeof(TutorialDirector), "Awake")]
    internal class TutorialPatches
    {
        static readonly TutorialDirector.TutorialPage lureTrapTutorial = new()
        {
            pageName = "LureTrap",
            video = null,
            text = "A Monster Lure trap is active! Hide or Fight!",
            focusText = "Lure Trap Active!",
            dummyText = "Good luck!"
        };
        static readonly TutorialDirector.TutorialPage auditTrapTutorial = new()
        {
            pageName = "AuditTrap",
            video = null,
            text = "You've been audited! Goodbye, money!",
            focusText = "You've been audited!",
            dummyText = "Goodbye, money!"
        };
        /*static readonly TutorialDirector.TutorialPage deathlinkTutorial = new()
        {
            pageName = "DeathLink",
            video = null,
            text = "A companion has died, which means you did too (sort of)!",
            focusText = "Death link triggered!",
            dummyText = "deathlink"
        };*/

        [HarmonyPostfix]
        public static void CreateAPTutorialPages(TutorialDirector __instance)
        {
            if (!__instance.tutorialPages.Contains(lureTrapTutorial))
                __instance.tutorialPages.Add(lureTrapTutorial);
            if (!__instance.tutorialPages.Contains(auditTrapTutorial))
                __instance.tutorialPages.Add(auditTrapTutorial);
            /*if (!__instance.tutorialPages.Contains(deathlinkTutorial))
                __instance.tutorialPages.Add(deathlinkTutorial);*/
        }
    }
}
