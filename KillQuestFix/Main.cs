using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;

namespace KillQuestFix
{
    [BepInPlugin("Aidanamite.KillQuestFix", "KillQuestFix", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        internal static Assembly modAssembly = Assembly.GetExecutingAssembly();
        internal static string modName = $"{modAssembly.GetName().Name}";
        internal static string modDir = $"{Environment.CurrentDirectory}\\BepInEx\\{modName}";

        void Awake()
        {
            new Harmony($"com.Aidanamite.{modName}").PatchAll(modAssembly);
            Logger.LogInfo($"{modName} has loaded");
        }
    }

    [HarmonyPatch(typeof(HeroMerchant),"CompleteQuest")]
    class Patch_CompleteQuest
    {
        public static ActiveQuest current;
        public static void Prefix(ActiveQuest activeQuest) => current = activeQuest;
        public static void Postfix() => current = null;
    }

    [HarmonyPatch(typeof(ShopManager), "RemoveItems")]
    class Patch_RemoveItems
    {
        public static bool Prefix(ItemMaster item, int quantity)
        {
            if (Patch_CompleteQuest.current != null && item.name != Patch_CompleteQuest.current.quest.target && HeroMerchant.Instance.activeQuests.Exists((x) => x.quest.killQuestTarget == item.name && x != Patch_CompleteQuest.current))
            {
                if (Patch_CompleteQuest.current.failed)
                    return false;
                quantity = Patch_CompleteQuest.current.quest.quantity;
            }
            return true;
        }
    }
}