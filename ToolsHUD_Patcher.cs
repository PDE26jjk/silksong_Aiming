using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace silksong_Aiming {
    class ToolsHUD_Patcher {
        //static FieldInfo radialImageInfo;
        static FieldInfo bindingInfo;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RadialHudIcon), "Update")]
        public static void ToolsHUDUpdate(RadialHudIcon __instance) {
            //if (!AimingManager.IsAiming) return;
            bindingInfo ??= AccessTools.Field(typeof(ToolHudIcon), "binding");
            //bindingInfo.SetValue(__instance, AttackToolBinding.Neutral);
            var binding = (AttackToolBinding)bindingInfo.GetValue(__instance);

            if (AimingManager.ShouldUpdateHUD) {

                //radialImageInfo ??= AccessTools.Field(typeof(RadialHudIcon), "radialImage");
                //var radialImage = radialImageInfo.GetValue(__instance) as Image;
                //if (radialImage) {

                if (__instance is ToolHudIcon toolHud) {
                    toolHud.UpdateDisplayInstant();
                }
                //radialImage.color = Color.white;
                //}
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ToolHudIcon), "TryGetBarColour")]
        public static bool ToolsBarColor(ToolHudIcon __instance, ref bool __result, ref Color color) {
            if (!AimingManager.IsAiming) return true;
            bindingInfo ??= AccessTools.Field(typeof(ToolHudIcon), "binding");
            var binding = (AttackToolBinding)bindingInfo.GetValue(__instance);
            if (__instance.CurrentTool) {
                if ((binding == AttackToolBinding.Up && AimingManager.AttackKeyActive == 1)
                    || (binding == AttackToolBinding.Neutral && AimingManager.AttackKeyActive == 2)
                    || (binding == AttackToolBinding.Down && AimingManager.AttackKeyActive == 3)
                    ) {
                    color = Settings.ActiveToolColor.Value;
                    __result = true;
                    return false;
                }
            }
            return true;
        }

    }
}
