using HarmonyLib;
using UnityEngine;

namespace silksong_Aiming {
    class Boomerang_Patcher {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ToolBoomerang), "FixedUpdate")]
        public static void ToolBoomerang_FixedUpdate_post(ToolBoomerang __instance) {
            // Curve Claw and Curve Claw Upgraded
            if (!AimingManager.IsAiming) return;
            float elapsedTime = (float)AccessTools.Field(typeof(ToolBoomerang), "elapsedTime").GetValue(__instance);
            //Debug.Log("-----------------------ToolBoomerang11111111111" + elapsedTime);
            if (elapsedTime > 0.03f) return;
            Debug.Log("-----------------------ToolBoomerang");
            Vector2 position = __instance.gameObject.transform.position;
            AccessTools.Field(typeof(ToolBoomerang), "initialPosition").SetValue(__instance, position);
            AccessTools.Field(typeof(ToolBoomerang), "previousPosition").SetValue(__instance, position);
            var heroController = Main.gm.hero_ctrl;
            //float offset = heroController.transform.position.x - position.x;
            //if ((offset > 0 && heroController.cState.facingRight) || offset < 0 && !heroController.cState.facingRight) {
            //    Debug.Log("fix direction ToolBoomerang_FixedUpdate_post....................");
            //    position.x += 2 * offset;
            //    //position.x += 10;
            //    __instance.gameObject.transform.position = position;
            //    AccessTools.Field(typeof(ToolBoomerang), "initialPosition").SetValue(__instance, position);
            //}
            Vector2 dir = AimingManager.GetDirectionToMouse(position);
            float force = 15;
            Vector2 targetPosition = position + dir * force;

            AccessTools.Field(typeof(ToolBoomerang), "targetPosition").SetValue(__instance, targetPosition);
        }
        private static void fisDirection(ToolBoomerang __instance) {
            Vector2 position = __instance.gameObject.transform.position;
            var dir2mouse = AimingManager.GetDirectionToMouse(position);
            var heroController = Main.gm.hero_ctrl;
            float offset = heroController.transform.position.x - position.x;
            //Debug.Log("OnEnable...............facingRight: "+ AimingManager.FacingRightBeforeThrow);
            
            if ((offset * dir2mouse.x > 0)) {
                Debug.Log("fix direction ....................");
                position.x += 2 * offset;
                __instance.gameObject.transform.position = position;
            }

        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ToolBoomerang), "Start")]
        public static void ToolBoomerang_Start_post(ToolBoomerang __instance) {
            if (!AimingManager.IsAiming) return;
            fisDirection(__instance);
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ToolBoomerang), "OnEnable")]
        public static void ToolBoomerang_OnEnable_post(ToolBoomerang __instance) {
            if (!AimingManager.IsAiming) return;
            fisDirection(__instance);
        }

    }

}
