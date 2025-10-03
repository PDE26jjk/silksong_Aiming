using HarmonyLib;
using UnityEngine;
using HutongGames.PlayMaker.Actions;

namespace silksong_Aiming {
    class Fisherpin_Patcher {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpawnProjectileV2), "OnEnter")]
        public static void SpawnProjectileV2_OnEnter_post(SpawnProjectileV2 __instance) {
            if (!AimingManager.IsAiming) return;
            //Debug.Log(__instance.State.Name);
            //Debug.Log(__instance.Owner);
            //Debug.Log(__instance.State.Name.ToString());
            if (!__instance.State.Name.ToString().Contains("Fisherpin")) return;
            //Debug.Log("---------------------Fisherpin_Patcher");
            var obj = __instance.StoreSpawned.Value;
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
            var dir2mouse = AimingManager.GetDirectionToMouse(position);
            rb.linearVelocity = dir2mouse * v.magnitude * 1.5f;
            IProjectile component2 = obj.GetComponent<IProjectile>();
            if (component2 != null) {
                component2.VelocityWasSet();
            }
        }

    }
}
