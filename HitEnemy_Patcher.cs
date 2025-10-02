using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace silksong_Aiming {
    class HitEnemy_Patcher {
        static bool silkGainWhenAimingToolHit;
        static bool hasGetSetting = false;
        static List<string> toolsToAddSilk; 
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HealthManager), "TakeDamage")]
        public static void HealthManager_TakeDamage_pre(HealthManager __instance, HitInstance hitInstance) {
            //Debug.Log("HealthManager_TakeDamage_pre---------");
            //Debug.Log(__instance.gameObject.name);
            //Debug.Log(hitInstance.AttackType);
            //Debug.Log(hitInstance.RepresentingTool.name);
            if (!hasGetSetting) {
                silkGainWhenAimingToolHit = Settings.GetBool("SilkGainWhenAimingToolHit", false);
                toolsToAddSilk = Settings.GetStringList("ToolsToAddSilk");
                hasGetSetting = true;
            }
            if (!silkGainWhenAimingToolHit) return;
            if (hitInstance.RepresentingTool == null) return;
            string toolName = hitInstance.RepresentingTool.name;
            if (hitInstance.AttackType == AttackTypes.Generic) {
                if (toolsToAddSilk.Contains(toolName)) {
                    HeroController.instance.SilkGain();
                }
            }
            else
            if (hitInstance.AttackType == AttackTypes.Heavy) {
                if (toolsToAddSilk.Contains(toolName)) {
                    HeroController.instance.SilkGain();
                }
            }
        }
    }
}
