using HarmonyLib;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;

namespace silksong_Aiming {
    class WebshotWeaver_Patcher {
        private static float angle2mouse = 0;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActivateGameObject), "DoActivateGameObject")]
        public static void ActivateGameObject_DoActivateGameObject_post(ActivateGameObject __instance) {
            //Debug.Log(__instance.State.Name.ToString());
            if (!__instance.State.Name.ToString().Contains("WebShot W Fire")) return;
            //Debug.Log("-----------------------WebshotWeaver_Patcher111111111");
            var obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject); ;
            //PrintChildren(obj);
            //if (obj == null) return;
            //Debug.Log(obj.name.ToString());
            //if (obj.name == "SnipeShot Impact") {

            //    Debug.Log("Actions: ");

            //    foreach (var state in __instance.State.Fsm.States) {
            //        Debug.Log(state.Name);
            //        foreach (var action in state.Actions) {
            //            Debug.Log(action);
            //        }
            //    }
            //    Debug.Log("SnipeShot Impact: ");
            //    var be = obj.GetComponents<MonoBehaviour>();
            //    List<string> list = new List<string>();
            //    foreach (var item in be) {
            //        list.Add(item.GetType().Name);
            //    }

            //    Debug.Log(string.Join(",", list));
            //}
            //Debug.Log("-----------------------WebshotWeaver_Patcher");
            //if (obj.name.ToString().Contains("SnipeShot Impact")) return;
            //if (obj.name.ToString() == "SnipeShot Impact")
            //    obj.SetActive(true);

            float angle = angle2mouse;
            if (!AimingManager.IsAiming) {
                angle = 0;
            }
            Debug.Log("angle: " + angle);
            string[] toRoate = { "SnipeShot Trail", "Spit Effect W", "SnipeShot Impact", "Flash S" };
            if (toRoate.Contains(obj.name.ToString())) {
                if (angle > 90 || angle < -90) {
                    angle += 180;
                }
                obj.transform.SetRotation2D(angle);
            }
            //if (obj.name.ToString() == "Spit Effect W")
            //    obj.SetActive(false);
            //Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            //Transform transform = obj.transform;
            //Vector3 position = transform.position;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RayCast2dV2), "DoRaycast")]
        public static bool RayCast2dV2_DoRaycast_pre(RayCast2dV2 __instance) {
            if (!AimingManager.IsAiming) return true;

            if (!__instance.State.Name.ToString().Contains("Snipe Ray")) return true;
            //Debug.Log("-----------------------RayCast2dV2_DoRaycast_pre111111111");
            Debug.Log(__instance.State.Name.ToString());
            //Debug.Log(__instance.maxDepth);
            __instance.debug = true;
            //__instance.debugColor = Color.red;
            var _trans = AccessTools.Field(typeof(RayCast2dV2), "_trans").GetValue(__instance) as Transform;
            Vector2 position = __instance.fromPosition.Value;
            if (_trans != null) {
                position.x += _trans.position.x;
                position.y += _trans.position.y;
            }
            AimingManager.RefreshMousePosition();
            var dir2mouse = AimingManager.GetDirectionToMouse(position);
            //__instance.direction = dir2mouse;
            angle2mouse = AimingManager.GetAngleToMouse(position);
            float num = float.PositiveInfinity;
            if (__instance.distance.Value > 0f) {
                num = __instance.distance.Value;
            }
            Vector2 normalized = dir2mouse;
            Vector3 vector = _trans.TransformDirection(new Vector3(dir2mouse.x, dir2mouse.y, 0f));
            normalized.x = vector.x;
            normalized.y = vector.y;
            RaycastHit2D raycastHit2D;
            if (__instance.ignoreTriggers.Value) {
                Helper.IsRayHittingNoTriggers(position, normalized, num, ActionHelpers.LayerArrayToLayerMask(__instance.layerMask, __instance.invertMask.Value), out raycastHit2D);
            }
            else {
                raycastHit2D = Helper.Raycast2D(position, normalized, num, ActionHelpers.LayerArrayToLayerMask(__instance.layerMask, __instance.invertMask.Value));
            }
            if (raycastHit2D.collider != null && __instance.ignoreTriggers.Value && raycastHit2D.collider.isTrigger) {
                raycastHit2D = default(RaycastHit2D);
            }
            bool flag = raycastHit2D.collider != null;
            PlayMakerUnity2d.RecordLastRaycastHitInfo(__instance.Fsm, raycastHit2D);
            __instance.storeDidHit.Value = flag;
            if (flag) {
                __instance.storeHitObject.Value = raycastHit2D.collider.gameObject;
                __instance.storeHitPoint.Value = raycastHit2D.point;
                __instance.storeHitNormal.Value = raycastHit2D.normal;
                __instance.storeHitDistance.Value = raycastHit2D.fraction;
                __instance.storeDistance.Value = raycastHit2D.distance;
                //Debug.Log("HitEvent");
                __instance.Fsm.Event(__instance.hitEvent);
            }
            else {
                //Debug.Log("noHitEvent");
                __instance.storeHitPoint.Value = position + dir2mouse * 30;
                __instance.storeDistance.Value = 30;
                __instance.Fsm.Event(__instance.hitEvent);
            }

            //Debug.Log("-----------------------RayCast2dV2_DoRaycast_pre");
            return false;
        }
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(SpawnObjectFromGlobalPool), "OnEnter")]
        //public static void SpawnObjectFromGlobalPool_OnEnter_pre(SpawnObjectFromGlobalPool __instance) {
        //    if (!AimingManager.IsAiming) return;
        //    if (!__instance.State.Name.ToString().Contains("WebShot W Fire")) return;
        //    //Debug.Log("-----------------------SpawnObjectFromGlobalPool111111111");
        //    //__instance.debugColor = Color.red;
        //    var obj = __instance.storeObject.Value;
        //    //PrintChildren(obj);
        //    //obj.transform.SetRotation2D(angle2mouse);
        //    //Debug.Log(angle2mouse);
        //    //Debug.Log("-----------------------SpawnObjectFromGlobalPool");
        //}
        public static void PrintChildren(GameObject obj, int depth = 0) {
            string indent = new string(' ', depth * 2);
            Debug.Log($"{indent}{obj.name}");
            foreach (Transform child in obj.transform) {
                PrintChildren(child.gameObject, depth + 1);
            }
        }
    }
    class WebshotForge_Patcher {
        public static void PrintChildren(GameObject obj, int depth = 0) {
            string indent = new string(' ', depth * 2);
            Debug.Log($"{indent}{obj.name}");
            var be = obj.GetComponents<MonoBehaviour>();
            List<string> list = new List<string>();
            foreach (var item in be) {
                list.Add(item.GetType().Name);
            }
            if (list.Count > 0) {
                Debug.Log(string.Join(",", list));
            }
            foreach (Transform child in obj.transform) {
                PrintChildren(child.gameObject, depth + 1);
            }
        }
        //private static float angle2mouse = 0;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActivateGameObject), "DoActivateGameObject")]
        public static void ActivateGameObject_DoActivateGameObject_post(ActivateGameObject __instance) {
            if (!AimingManager.IsAiming) return;
            //Debug.Log(__instance.State.Name.ToString());
            if (!__instance.State.Name.ToString().Contains("WebShot F Fire")) return;
            //Debug.Log("-----------------------WebShot F Fire111111111");
            var obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject); ;
            WebshotWeaver_Patcher.PrintChildren(obj);
            //if (obj == null) return;
            //Debug.Log(obj.name.ToString());
            //if (obj.name == "SnipeShot Impact") {


            //    Debug.Log(string.Join(",", list));
            //}
            //Debug.Log("-----------------------WebshotWeaver_Patcher");
            //if (obj.name.ToString().Contains("SnipeShot Impact")) return;
            //if (obj.name.ToString() == "SnipeShot Impact")
            //    obj.SetActive(true);
            //float angle = angle2mouse;
            //Debug.Log("angle: " + angle);
            Vector3 position = obj.transform.position;
            var dir2mouse = AimingManager.GetDirectionToMouse(position);
            var angle2mouse = AimingManager.GetAngleToMouse(position);
            float angle = angle2mouse;
            Debug.Log("angle: " + angle);
            string[] toRoate = { "Spit Effect S", "Flash S" };
            if (toRoate.Contains(obj.name.ToString())) {
                if (angle > 90 || angle < -90) {
                    angle += 180;
                }
                obj.transform.SetRotation2D(angle);
            }
            //if (obj.name.ToString() == "Spit Effect W")
            //    obj.SetActive(false);
            //Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            //Transform transform = obj.transform;
            //Vector3 position = transform.position;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpawnObjectFromGlobalPool), "OnEnter")]
        public static void SpawnObjectFromGlobalPool_OnEnter_pre(SpawnObjectFromGlobalPool __instance) {
            if (!AimingManager.IsAiming) return;
            if (!__instance.State.Name.ToString().Contains("WebShot F Fire")) return;
            //Debug.Log("-----------------------SpawnObjectFromGlobalPool111111111");
            //__instance.debugColor = Color.red;
            var obj = __instance.storeObject.Value;
            if (obj.name.Contains("WebShot Bullet")) {
                //obj.SetActive(false);
                PrintChildren(obj);
                var fsm = obj.GetComponent<PlayMakerFSM>().Fsm;
                foreach (var state in fsm.States) {
                    Debug.Log(state.Name);
                    foreach (var action in state.Actions) {
                        Debug.Log(action);
                    }
                }

                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                Debug.Log(rb.linearVelocity);
                //rb.linearVelocity = new  Vector2(0, 1);
                obj.transform.SetRotation2D(90);
            }
            //obj.transform.SetRotation2D(angle2mouse);
            //Debug.Log(angle2mouse);
            //Debug.Log("-----------------------SpawnObjectFromGlobalPool");
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SetVelocityAsAngle), "DoSetVelocity")]
        public static void SetVelocityAsAngle_DoSetVelocity_pre(SetVelocityAsAngle __instance) {
            if (!AimingManager.IsAiming) return;
            //Debug.Log("----------------------- SetVelocityAsAngle11111111");

            string stateName = __instance.State.Name.ToString();
            if (!stateName.Equals("WebShot F Fire") && !stateName.Equals("WebShot A Fire")) return;
            //Debug.Log("----------------------- SetVelocityAsAngle: " + stateName);
            float origin_angle = __instance.angle.Value;

            Transform transform = __instance.gameObject.GameObject.Value.transform;
            Vector3 position = transform.position;
            //position.y += 0.3f;
            //if (origin_angle < 90) {
            //    position.x += 0;
            //}
            //else {
            //    position.x += 1.5f;
            //}
            //transform.position = position;
            AimingManager.RefreshMousePosition();
            float angle2mouse = AimingManager.GetAngleToMouse(position);
            __instance.angle.Value = angle2mouse;
            //float angle = origin_angle;
            //float offset = 0;
            //if (origin_angle < 90) {
            //    angle += angle2mouse - offset;
            //}
            //else {
            //    angle -= 180 - angle2mouse - offset;
            //}
            //__instance.angle.Value = angle;

            //if (stateName.Equals("WebShot A Fire")) {
            //    __instance.angle.Value = angle2mouse;
            //}

            //Debug.Log(__instance.angle.Value + ": " + angle2mouse + ": " + origin_angle);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RayCast2dV2), "DoRaycast")]
        public static bool RayCast2dV2_DoRaycast_pre(RayCast2dV2 __instance) {
            if (!AimingManager.IsAiming) return true;
            return true;
            if (!__instance.State.Name.ToString().Contains("WebShot F Fire")) return true;
            Debug.Log("-----------------------RayCast2dV2_DoRaycast_pre111111111");
            Debug.Log(__instance.State.Name.ToString());
            //Debug.Log(__instance.maxDepth);
            __instance.debug = true;
            //__instance.debugColor = Color.red;
            var _trans = AccessTools.Field(typeof(RayCast2dV2), "_trans").GetValue(__instance) as Transform;
            Vector2 position = __instance.fromPosition.Value;
            if (_trans != null) {
                position.x += _trans.position.x;
                position.y += _trans.position.y;
            }
            AimingManager.RefreshMousePosition();
            var dir2mouse = AimingManager.GetDirectionToMouse(position);
            Debug.Log(__instance.direction);
            __instance.direction = dir2mouse;
            //var angle2mouse = AimingManager.GetAngleToMouse(position);
            //float num = float.PositiveInfinity;
            //if (__instance.distance.Value > 0f) {
            //    num = __instance.distance.Value;
            //}
            //Vector2 normalized = __instance.direction.Value.normalized;
            //Vector3 vector = _trans.TransformDirection(new Vector3(__instance.direction.Value.x, __instance.direction.Value.y, 0f));
            //normalized.x = vector.x;
            //normalized.y = vector.y;
            //RaycastHit2D raycastHit2D;
            //if (__instance.ignoreTriggers.Value) {
            //    Helper.IsRayHittingNoTriggers(position, normalized, num, ActionHelpers.LayerArrayToLayerMask(__instance.layerMask, __instance.invertMask.Value), out raycastHit2D);
            //}
            //else {
            //    raycastHit2D = Helper.Raycast2D(position, normalized, num, ActionHelpers.LayerArrayToLayerMask(__instance.layerMask, __instance.invertMask.Value));
            //}
            //if (raycastHit2D.collider != null && __instance.ignoreTriggers.Value && raycastHit2D.collider.isTrigger) {
            //    raycastHit2D = default(RaycastHit2D);
            //}
            //bool flag = raycastHit2D.collider != null;
            //PlayMakerUnity2d.RecordLastRaycastHitInfo(__instance.Fsm, raycastHit2D);
            //__instance.storeDidHit.Value = flag;
            //if (flag) {
            //    __instance.storeHitObject.Value = raycastHit2D.collider.gameObject;
            //    __instance.storeHitPoint.Value = raycastHit2D.point;
            //    __instance.storeHitNormal.Value = raycastHit2D.normal;
            //    __instance.storeHitDistance.Value = raycastHit2D.fraction;
            //    __instance.storeDistance.Value = raycastHit2D.distance;
            //    Debug.Log("HitEvent");
            //    __instance.Fsm.Event(__instance.hitEvent);
            //}
            //else {
            //    Debug.Log("noHitEvent");
            //    __instance.storeHitPoint.Value = position + dir2mouse * 30;
            //    __instance.storeDistance.Value = 30;
            //    __instance.Fsm.Event(__instance.hitEvent);
            //}

            Debug.Log("-----------------------RayCast2dV2_DoRaycast_pre");
            return true;
        }
    }
}
