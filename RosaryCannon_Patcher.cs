using HarmonyLib;
using UnityEngine;
using HutongGames.PlayMaker.Actions;

namespace silksong_Aiming {
    class RosaryCannon_Patcher {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpawnProjectileV2), "OnEnter")]
        public static void SpawnProjectileV2_OnEnter_post(SpawnProjectileV2 __instance) {
            if (!AimingManager.IsAiming) return;
            //Debug.Log(__instance.State.Name.ToString());
            if (!__instance.State.Name.ToString().Contains("Shoot")) return;
            var obj = __instance.StoreSpawned.Value;
            if (!obj.name.ToString().Contains("Geo Small Projectile")) return;
            //Debug.Log("-----------------------RosaryCannon_Patcher111111111");
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            Transform transform = obj.transform;
            Vector3 position = transform.position;
            var v = rb.linearVelocity;
            //position.y += 0.3f;
            //if (origin_angle < 90) {
            //    //position.x += 0;
            //}
            //else {
            //    position.x += 1.5f;
            //}
            transform.position = position;
            AimingManager.RefreshMousePosition();
            var dir2mouse = AimingManager.GetDirectionToMouse(position);
            rb.linearVelocity = dir2mouse * v.magnitude;
            IProjectile component2 = obj.GetComponent<IProjectile>();
            if (component2 != null) {
                component2.VelocityWasSet();
            }
            var heroController = Main.gm.hero_ctrl;
            if (heroController != null) {
                // 方向不对就转身
                if ((heroController.cState.facingRight ? 1 : -1) * Mathf.Sign(dir2mouse.x) < 0) {
                    heroController.FlipSprite();
                }
            }
            //Debug.Log("-----------------------RosaryCannon_Patcher");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ListenForToolThrow), "OnUpdate")]
        public static bool ListenForToolThrow_OnUpdate_pre(ListenForToolThrow __instance) {
            if (!AimingManager.IsAiming) return true;
            if (AimingManager.AttackKeyActive >= 4 || HeroInput_Patcher.AttackWasReleased) return true;
            if (!__instance.IsActive.IsNone && !__instance.IsActive.Value) {
                return false;
            }
            var inputActions = Main.gm.inputHandler.inputActions;
            if (Main.gm.hero_ctrl.IsPaused()) { return false; }
            var CanDo = (bool reportFailure) => !__instance.RequireToolToThrow.Value || HeroController.instance.GetWillThrowTool(reportFailure);
            if (inputActions.Attack.WasPressed && CanDo(true)) {
                __instance.Fsm.Event(__instance.WasPressed);
            }
            if (inputActions.Attack.WasReleased) {
                __instance.Fsm.Event(__instance.WasReleased);
            }
            if (inputActions.Attack.IsPressed && CanDo(false)) {
                __instance.Fsm.Event(__instance.IsPressed);
            }
            if (!inputActions.Attack.IsPressed) {
                __instance.Fsm.Event(__instance.IsNotPressed);
            }
            return false;
        }


    }
}
