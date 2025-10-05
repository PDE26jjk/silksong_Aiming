using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace silksong_Aiming {
    class ToolsHUD_Patcher {
        static FieldInfo radialImageInfo;
        static FieldInfo bindingInfo;
        static Color ActiveColor = Color.black;

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
                    if(ActiveColor == Color.black) { 
                        ActiveColor = Settings.GetColor("ActiveToolColor", new Color32(234, 204, 128, 255));
                    }
                    color = ActiveColor;
                    __result = true;
                    return false;
                }
            }
            return true;
        }

    }
}
