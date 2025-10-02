using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace silksong_Aiming {
    class Conch_Patcher {
        private static Vector2 Dir = Vector2.zero;
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SetDesiredProjectileVelocity), "OnEnter")]
        public static void SetDesiredProjectileVelocity_OnEnter_post(SetDesiredProjectileVelocity __instance) {
            if (!AimingManager.IsAiming) return;
            //Debug.Log("-----------------------Conch_Patcher111111111");
            string[] toPatch = { "UR", "LR", "DR", "DL" };
            string stateName = __instance.State.Name.ToString();
            if (!toPatch.Contains(stateName)) {
                return;
            }

            Debug.Log(__instance.State.Name.ToString());
            //Vector2 oriVel = __instance.DesiredVelocity.Value;

            //foreach (var state in __instance.State.Fsm.States) {
            //    Debug.Log(state.Name);
            //    foreach (var action in state.Actions) {
            //        Debug.Log(action);
            //    }

            //}
            GameObject obj = __instance.Target.GetSafe(__instance);
            if (obj) {
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                var v = rb.linearVelocity;
                //Debug.Log(oriVel);
                //Debug.Log(oriVel.magnitude);
                //Debug.Log(v);
                //if (!__instance.State.Name.ToString().Contains("Conch")) return;
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = -UnityEngine.Camera.main.transform.position.z;
                Vector3 mouseWorldPos = UnityEngine.Camera.main.ScreenToWorldPoint(mousePos);
                mouseWorldPos.z = 0;
                AimingManager.MousePosW = mouseWorldPos;
                Vector2 dir = AimingManager.GetDirectionToMouse(obj.transform.position);
                if (Time.time - AimingManager.LastClickTime < 0.03f) {
                    //Debug.Log("aaaaaaaaaaaa" + AimingManager.LastClickTime);
                }
                else {
                    //dir.x *= Mathf.Sign(dir.x * v.x);
                    //dir.y *= Mathf.Sign(dir.y * v.y);
                    //Debug.Log("bbbbbbbbbbbbb" + Time.time);
                    return;
                }
                //    if (previousNormal.magnitude > 0) {
                //        dir.x *= Mathf.Sign(dir.x * oriVel.x);
                //        dir.y *= Mathf.Sign(dir.y * oriVel.y);
                //    }

                //var conchProjectileCollision = obj.GetComponent<ConchProjectileCollision>();
                //if (conchProjectileCollision) {
                //   var previousNormalInfo = AccessTools.Field(typeof(ConchProjectileCollision), "previousNormal");
                //    Vector2 previousNormal = (Vector2)previousNormalInfo.GetValue(conchProjectileCollision);
                //   Debug.Log( AccessTools.Field(typeof(ConchProjectileCollision), "direction").GetValue(conchProjectileCollision));
                //   Debug.Log( AccessTools.Field(typeof(ConchProjectileCollision), "isActive").GetValue(conchProjectileCollision));
                //    previousNormalInfo.SetValue(conchProjectileCollision, -dir);
                //}
                rb.linearVelocity = dir * 35;
                __instance.DesiredVelocity = rb.linearVelocity;
                Dir = dir;
                //var conchProjectileCollision = obj.GetComponent<ConchProjectileCollision>();
                //conchProjectileCollision.SetDirection(dir);
                //Debug.Log(__instance.DesiredVelocity.Value);
                //safe.transform.SetLocalRotation2D(AimingManager.GetAngleToMouse(safe.transform.position));
                //Debug.Log("-----------------------Conch_Patcher");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SetRotation), "DoSetRotation")]
        public static void SetRotation_DoSetRotation_post(SetRotation __instance) {
            if (!AimingManager.IsAiming) return;
            GameObject obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject);
            if (!obj || obj.name != "Sprite") {
                return;
            }
            if (Time.time - AimingManager.LastClickTime < 0.03f) {
                //Debug.Log("aaaaaaaaaaaa" + AimingManager.LastClickTime);
                //Debug.Log("-----------------------DoSetRotation11111111");
                //Debug.Log(__instance.State.Name.ToString());

                Transform transform = obj.transform;
                //Vector3 position = transform.position;
                float angle = Mathf.Atan2(Dir.y, Dir.x) * Mathf.Rad2Deg;
                transform.SetRotation2D(angle);
                DamageEnemies component = obj.GetComponent<DamageEnemies>();
                if (component != null) {
                    component.SetDirection(angle);
                }
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ConchProjectileHelperAction), "OnEnter")]
        public static void ConchProjectileHelperAction_OnEnter_pre(ConchProjectileHelperAction __instance) {
            if (!AimingManager.IsAiming) return;
            if (Time.time - AimingManager.LastClickTime < 0.038f) {
                //Debug.Log("aaaaaaaaaaaa" + AimingManager.LastClickTime);
                Debug.Log("-----------------------ConchProjectileHelperAction_OnEnter_pre");
                Debug.Log(Dir);
                Vector2 ori_dir = __instance.direction.Value;
                Debug.Log(ori_dir);
                //Debug.Log(__instance.State.Name.ToString());
                var conchProjectileCollision = __instance.target.GetSafe<ConchProjectileCollision>(__instance);
                Debug.Log(conchProjectileCollision.transform.GetScaleX());
                if (ori_dir == Dir) {
                    Debug.Log("same");
                    return;
                }
                if (conchProjectileCollision != null) {
                    // TODO: 螺角反射的方向怎么修改？
                    //if (Dir.x * ori_dir.x > 0) {
                    //    conchProjectileCollision.transform.SetScaleX(-1);
                    //    conchProjectileCollision.transform.SetScaleY(-1);
                    //}
                    //else {
                    //    conchProjectileCollision.transform.SetScaleX(1);
                    //    conchProjectileCollision.transform.SetScaleY(1);
                    //}
                }
                __instance.direction = Dir;

            }
        }

    }

}
