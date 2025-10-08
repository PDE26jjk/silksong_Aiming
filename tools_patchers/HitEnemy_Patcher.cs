using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace silksong_Aiming {
    class HitEnemy_Patcher {
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
                var getStringList = (string value) => {
                    return value.Split(',')
                                .Select(s => s.Trim())
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList();
                };
                toolsToAddSilk = getStringList(Settings.ToolsToAddSilk.Value);
                Settings.ToolsToAddSilk.SettingChanged += (sender, e) => {
                    toolsToAddSilk = getStringList(Settings.ToolsToAddSilk.Value);
                };
                hasGetSetting = true;
            }
            if (!Settings.SilkGainWhenAimingToolHit.Value) return;
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
