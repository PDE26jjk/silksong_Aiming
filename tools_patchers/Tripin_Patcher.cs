using HarmonyLib;
using UnityEngine;
using HutongGames.PlayMaker.Actions;

namespace silksong_Aiming {
    class Tripin_Patcher {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SetVelocityAsAngle), "DoSetVelocity")]
        public static void SetVelocityAsAngle_DoSetVelocity_pre(SetVelocityAsAngle __instance) {
            if (!AimingManager.IsAiming) return;
            //Debug.Log("-----------------------Tripin_Patcher11111111");
            //Debug.Log(__instance.State.Name.ToString());
            if (!__instance.State.Name.ToString().Contains("TriPin")) return;
            //Debug.Log("-----------------------Tripin_Patcher");
            float origin_angle = __instance.angle.Value;

            //Debug.Log(origin_angle);
            Transform transform = __instance.gameObject.GameObject.Value.transform;
            Vector3 position = transform.position;
            position.y += 0.3f;
            if (origin_angle < 90) {
                position.x += 0;
            }
            else {
                position.x += 1.5f;
            }
            transform.position = position;
            float angle2mouse = AimingManager.GetAngleToMouse(position);
            float offset = 12;
            if (origin_angle < 90) {
                __instance.angle.Value += angle2mouse - offset;
            }
            else {
                __instance.angle.Value -= 180 - angle2mouse - offset;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SetRotation), "DoSetRotation")]
        public static void SetRotation_DoSetRotation_post(SetRotation __instance) {
            if (!AimingManager.IsAiming) return;
            //Debug.Log("-----------------------Tripin_Patcher11111111");
            //Debug.Log(__instance.State.Name.ToString());
            if (!__instance.State.Name.ToString().Contains("TriPin")) return;

            Transform transform = __instance.gameObject.GameObject.Value.transform;
            Vector2 position = transform.position;
            position.x -= 0.2f;
            if(Main.hero.cState.wallSliding && Main.hero.cState.facingRight) {
                position.x -= 1;
            }
            transform.position = position;
            //DebugLineRenderer.DrawLine(position + Vector2.up * 5, position + Vector2.up * -5, Color.green, 2);
            //float angle2mouse = AimingManager.GetAngleToMouse(position);
            Rigidbody2D rb = transform.GetComponent<Rigidbody2D>();
            var v = rb.linearVelocity.normalized;
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            if (v.x < 0) {
                angle += 180;
            }
            var localScale = transform.localScale;
            localScale.x = Mathf.Sign(v.x);
            transform.localScale = localScale;
            transform.SetRotation2D(0);
            transform.SetLocalRotation2D(angle);
        }

    }
}
