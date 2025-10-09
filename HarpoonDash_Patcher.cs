using HarmonyLib;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using System.Linq;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;

namespace silksong_Aiming {
    class HarpoonDash_Patcher {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SendEventToRegister), "OnEnter")]
        public static void BeforeHarpoonDash(SendEventToRegister __instance) {
            if (!AimingManager.IsAiming) return;
            //Debug.Log("SendEventToRegister: "+__instance.State.Name.ToString());
            if (!__instance.State.Name.ToString().Contains("Antic")) return;
            //Debug.Log("Can Do?  evenname:      " + __instance.eventName);
            var hero = Main.gm.hero_ctrl;
            AimingManager.RefreshMousePosition();
            var dir2mouse = AimingManager.GetDirectionToMouse(hero.gameObject.transform.position);
            if (hero != null) {
                // 方向不对就转身
                if ((hero.cState.facingRight ? 1 : -1) * Mathf.Sign(dir2mouse.x) < 0) {
                    hero.FlipSprite();
                }
            }            //__instance.Finish();
        }
        static bool needResetLocalRotate = false;
        public static void resetLocalRotate() {
            if (needResetLocalRotate) {
                //Debug.Log("resetLocalRotate///////////////////");
                var hero = Main.gm.hero_ctrl;
                foreach (var obj in objectsNeedToReset) {
                    obj.transform.localRotation = Quaternion.identity;
                }
                needResetLocalRotate = false;
                //hero.StartCoroutine(OffsetToZeroCoroutine());
                hero.transform.SetLocalRotation2D(0f);
                //Collider2D collider2D = hero.GetComponent<BoxCollider2D>();
                //collider2D.offset = Vector2.up * -.49f;
            }
        }
        private static IEnumerator OffsetToZeroCoroutine() {
            var transform = Main.gm.hero_ctrl.transform;
            Collider2D collider2D = transform.GetComponent<BoxCollider2D>();
            float offstTo = -.49f;
            float startOffset = collider2D.offset.y;
            float time = 0f;
            float duration = 1f;
            AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            while (time < duration) {
                time += Time.deltaTime;
                float t = easingCurve.Evaluate(time / duration);
                float newOffset = Mathf.Lerp(startOffset, offstTo, t);
                collider2D.offset = Vector2.up * newOffset;
                //Debug.Log(newOffset);
                yield return null;
            }
            collider2D.offset = Vector2.up * -.49f;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SendMessage), "DoSendMessage")]
        public static void CancelAll(SendMessage __instance) {
            if (!AimingManager.UseHarpoonDashAiming()) return;
            if (!__instance.State.Name.ToString().Contains("Cancel All")) return;
            resetLocalRotate();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActivateGameObject), "DoActivateGameObject")]
        public static void ActivateGameObject_DoActivateGameObject_post2(ActivateGameObject __instance) {
            if (!AimingManager.UseHarpoonDashAiming()) return;
            if (!__instance.State.Name.ToString().Contains("Dash")) return;
            var obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject); ;
            //Debug.Log("DoActivateGameObject Dash-------------");
            //Debug.Log(obj.name);
            string[] tohandle = { "Hornet_harpoon_dash", "Harpoon Dash Damager" };
            if (tohandle.Contains(obj.name)) {
                //obj.SetActiveChildren(false);
                WebshotWeaver_Patcher.PrintChildren(obj);
            }
        }
        static float threadLength = 10.5f;
        static Vector2 needleTarget;

        private static Type hitCheckType;
        private static Type hitTypesType;
        private static FieldInfo hitTypeField;
        private static FieldInfo hitField;

        static Vector2 DirDash;
        static float AngleDash;

        private static Vector2 getRayDirectionAngRayStartPoint(GameObject hero, ref Vector2 origin) {
            var dir = AimingManager.GetDirectionToMouse(hero.transform.position);
            Vector2 heroPos = hero.transform.position;
            var orthoDir = new Vector2(dir.y, -dir.x);
            origin = heroPos + (origin - heroPos).y * orthoDir;
            DirDash = dir;
            return dir;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HarpoonDashRayCheck), "CheckRay")]
        public static bool CheckRay_Pre(
        ref object __result,
        HarpoonDashRayCheck __instance,
        Vector2 origin,
        bool isTerrainCheck) {
            if (!AimingManager.UseHarpoonDashAiming()) return true;
            try {
                //Debug.Log("CheckRay_Pre------------");
                var resultsField = AccessTools.Field(__instance.GetType(), "results");

                // 获取字段值
                RaycastHit2D[] results = (RaycastHit2D[])resultsField.GetValue(__instance);

                // 创建 ContactFilter2D
                ContactFilter2D contactFilter = new ContactFilter2D {
                    useLayerMask = true,
                    layerMask = (isTerrainCheck ? 256 : 657408),
                    useTriggers = true
                };
                GameObject hero = __instance.Hero.GetSafe(__instance);
                Vector2 dir = getRayDirectionAngRayStartPoint(hero, ref origin);
                // 执行射线检测
                int num2 = Physics2D.Raycast(origin, dir, contactFilter, results, threadLength);

                // 获取 HitCheck 类型
                Type hitCheckType = AccessTools.Inner(__instance.GetType(), "HitCheck");
                Type hitTypesType = AccessTools.Inner(__instance.GetType(), "HitTypes");

                // 遍历检测结果
                for (int i = 0; i < Mathf.Min(num2, results.Length); i++) {
                    RaycastHit2D raycastHit2D = results[i];
                    Collider2D collider = raycastHit2D.collider;

                    if (collider.gameObject.layer == 11) // 敌人层
                    {
                        HealthManager healthManager;
                        // 使用反射调用 HitTaker.TryGetHealthManager
                        if (!HitTaker.TryGetHealthManager(collider.gameObject, out healthManager) || !healthManager.IsInvincible || !healthManager.PreventInvincibleEffect || healthManager.InvincibleFromDirection == 2 || healthManager.InvincibleFromDirection == 4 || healthManager.InvincibleFromDirection == 7) {
                            Type healthManagerType = healthManager.GetType();

                            // 检查健康管理器属性
                            PropertyInfo isInvincibleProp = AccessTools.Property(healthManagerType, "IsInvincible");
                            PropertyInfo preventInvincibleProp = AccessTools.Property(healthManagerType, "PreventInvincibleEffect");
                            PropertyInfo invincibleFromDirectionProp = AccessTools.Property(healthManagerType, "InvincibleFromDirection");

                            bool isInvincible = (bool)isInvincibleProp.GetValue(healthManager);
                            bool preventInvincible = (bool)preventInvincibleProp.GetValue(healthManager);
                            int invincibleDirection = (int)invincibleFromDirectionProp.GetValue(healthManager);

                            // 检查是否可以命中
                            if (!isInvincible || !preventInvincible ||
                                invincibleDirection == 2 || invincibleDirection == 4 || invincibleDirection == 7) {
                                // 创建 HitCheck 结果
                                __result = CreateHitCheck(hitCheckType, hitTypesType, "Enemy", raycastHit2D);
                                return false; // 跳过原始方法
                            }
                        }
                    }
                    else {
                        // 检查特殊碰撞体类型
                        if (collider.CompareTag("Bounce Pod") || collider.GetComponent<BouncePod>() || collider.GetComponent<HarpoonHook>()) {
                            __result = CreateHitCheck(hitCheckType, hitTypesType, "BouncePod", raycastHit2D);
                            return false;
                        }

                        if (collider.CompareTag("Harpoon Ring")) {
                            __result = CreateHitCheck(hitCheckType, hitTypesType, "HarpoonRing", raycastHit2D);
                            return false;
                        }

                        if (collider.gameObject.layer == 17 && collider.GetComponent<TinkEffect>() && !collider.GetComponent<TinkEffect>().noHarpoonHook) // Tink效果层
                        {
                            __result = CreateHitCheck(hitCheckType, hitTypesType, "Tinker", raycastHit2D);
                            return false;
                        }

                        if (collider.gameObject.layer == 8) // 地形层
                        {
                            __result = CreateHitCheck(hitCheckType, hitTypesType, "Terrain", raycastHit2D);
                            return false;
                        }
                    }
                }

                // 没有命中任何物体
                __result = CreateHitCheck(hitCheckType, hitTypesType, "None", default(RaycastHit2D));
                return false; // 跳过原始方法
            }
            catch (Exception ex) {
                Debug.LogError($"CheckRay 补丁错误: {ex}");
                return true; // 出错时执行原始方法
            }
        }

        private static object CreateHitCheck(Type hitCheckType, Type hitTypesType, string hitTypeName, RaycastHit2D hit) {
            // 创建 HitCheck 实例
            object hitCheck = Activator.CreateInstance(hitCheckType);

            // 设置 Hit 字段
            hitField ??= AccessTools.Field(hitCheckType, "Hit");
            hitField.SetValue(hitCheck, hit);

            // 设置 HitType 字段
            hitTypeField ??= AccessTools.Field(hitCheckType, "HitType");
            object hitTypeValue = Enum.Parse(hitTypesType, hitTypeName);
            hitTypeField.SetValue(hitCheck, hitTypeValue);

            return hitCheck;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HarpoonDashRayCheck), "CheckRay")]
        public static void CheckRay_post(HarpoonDashRayCheck __instance, ref object __result, Vector2 origin, bool isTerrainCheck) {
            if (!AimingManager.UseHarpoonDashAiming()) return;
            hitCheckType ??= AccessTools.Inner(typeof(HarpoonDashRayCheck), "HitCheck");
            hitTypesType ??= AccessTools.Inner(typeof(HarpoonDashRayCheck), "HitTypes");
            hitTypeField ??= AccessTools.Field(hitCheckType, "HitType");
            hitField ??= AccessTools.Field(hitCheckType, "Hit");
            // 获取 HitType
            object hitTypeValue = hitTypeField.GetValue(__result);
            string hitType = Enum.GetName(hitTypesType, hitTypeValue);

            // 获取 RaycastHit2D
            object hitValue = hitField.GetValue(__result);
            RaycastHit2D hit = (RaycastHit2D)hitValue;

            GameObject hero = __instance.Hero.GetSafe(__instance);
            Vector2 dir = getRayDirectionAngRayStartPoint(hero, ref origin);

            //Debug.Log($"CheckRay 返回结果:");
            //Debug.Log($"- 命中类型: {hitType}");
            ////GameObject obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.Hero);
            //if (hit.collider != null) {
            //    Debug.Log($"- 命中碰撞体: {hit.collider.name}");
            //    Debug.Log($"- 命中点: {hit.point}");
            //    Debug.Log($"- 命中距离: {hit.distance}");
            //    Debug.Log($"- 命中法线: {hit.normal}");
            //    DebugLineRenderer.DrawLine(origin, hit.point, Color.red, 1f);
            //}
            //else {
            //    Debug.Log("- 未命中任何物体");
            //DebugLineRenderer.DrawLine(origin, origin + dir * threadLength, Color.green, 1f);
            //}
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HarpoonDashRayCheck), "OnEnter")]
        public static void RayCheck(HarpoonDashRayCheck __instance) {
            if (!AimingManager.UseHarpoonDashAiming()) return;
            string stateName = __instance.State.Name.ToString();
            GameObject hero = __instance.Hero.GetSafe(__instance);
            if (__instance.StoreHitPoint.Value == Vector2.zero) {
                Vector2 origin = (Vector2)hero.transform.position + new Vector2(0, 0.05f);
                Vector2 dir = getRayDirectionAngRayStartPoint(hero, ref origin);
                needleTarget = origin + dir * threadLength;
                //DebugLineRenderer.DrawLine(origin, origin + dir * threadLength, Color.green, 1f);
            }
            else {
                needleTarget = __instance.StoreHitPoint.Value;
            }
            return;
        }

        static HashSet<GameObject> objectsNeedToReset = new();
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActivateGameObject), "DoActivateGameObject")]
        public static void SetNeedleBreakerPosition(ActivateGameObject __instance) {
            if (!AimingManager.UseHarpoonDashAiming()) return;
            string stateName = __instance.State.Name.ToString();
            var obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject); ;
            if (!obj) return;
            if (!__instance.State.Name.ToString().Contains("Throw")) return;
            string[] tohandle = { "Harpoon Breaker", "Harpoon Breaker Extend" };
            if (tohandle.Contains(obj.name)) {
                //Debug.Log(obj.name);
                AngleDash = Mathf.Atan2(DirDash.y, DirDash.x) * Mathf.Rad2Deg;
                float angle = AngleDash;
                if (DirDash.x < 0) {
                    angle += 180;
                }
                obj.transform.SetRotation2D(angle);
                objectsNeedToReset.Add(obj);
                //obj.transform.SetScaleX(Mathf.Sign(DirDash.x));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActivateGameObject), "DoActivateGameObject")]
        public static void SetNeedlePosition(ActivateGameObject __instance) {
            if (!AimingManager.UseHarpoonDashAiming()) return;
            string[] toPatch = { "Wall Needle", "Enemy Needle", "Tink Needle", "Air Needle" };
            bool needPatch = false;
            string stateName = __instance.State.Name.ToString();
            foreach (var str in toPatch) {
                if (stateName.Contains(str)) {
                    needPatch = true;
                    break;
                }
            }
            if (!needPatch) return;

            var obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject); ;
            string[] tohandle = { "Harpoon Needle" };
            if (tohandle.Contains(obj.name)) {
                //Debug.Log("SetNeedlePosition : Harpoon Needle");
                obj.transform.position = needleTarget;
                //obj.transform.eulerAngles = Dir;
                AngleDash = Mathf.Atan2(DirDash.y, DirDash.x) * Mathf.Rad2Deg;
                obj.transform.SetRotation2D(AngleDash);
                obj.transform.SetScaleX(Mathf.Sign(DirDash.x));
                objectsNeedToReset.Add(obj);
            }
        }

        static GameObject heroBox;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tk2dPlayAnimation), "DoPlayAnimation")]
        public static void t1(Tk2dPlayAnimation __instance) {
            if (!AimingManager.UseHarpoonDashAiming()) return;
            string stateName = __instance.State.Name.ToString();
            //if (!__instance.State.Name.ToString().Contains("Throw")) return;

            GameObject obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject);
            var spriteAnimator = obj.GetComponent<tk2dSpriteAnimator>();
            //Debug.Log("Tk2dPlayAnimation-------------: " + stateName);
            //Debug.Log(__instance.clipName);
            var _sprite = obj.GetComponent<tk2dSpriteAnimator>();
            var heroAnim = obj.GetComponent<IHeroAnimationController>();
            string clipName = __instance.clipName.Value;
            if (stateName.Contains("Dash") && clipName == "Harpoon Dash") {
                int angle2y = Math.Abs(((Math.Abs((int)AngleDash) % 180)) - 90);
                //Debug.Log("angle to y : " + angle2y);
                // overwrite the clip for a better visual effect.
                if (angle2y < 45 && stateName != "Ring Dash") {
                    string overwriteClipName = "Super Jump Loop";
                    tk2dSpriteAnimationClip clip = ((heroAnim != null) ? heroAnim.GetClip(overwriteClipName) : _sprite.GetClipByName(overwriteClipName));
                    _sprite.Play(clip);
                    AngleDash = Mathf.Atan2(DirDash.y, DirDash.x) * Mathf.Rad2Deg;
                    float angle = AngleDash - 90;
                    //OriginAngle = obj.transform.GetLocalRotation2D();
                    obj.transform.SetLocalRotation2D(angle); // reset at catch_end
                    Main.hero.StartCoroutine(resetLocalRotateDelay());

                    needResetLocalRotate = true;
                    if (!heroBox) {
                        foreach (Transform child in obj.transform) {
                            if (child.gameObject.name == "HeroBox") { heroBox = child.gameObject; break; }
                        }
                    }
                    heroBox.transform.SetRotation2D(0);
                    //Debug.Log("HeroBox -----------" + heroBox.transform.localRotation);
                    objectsNeedToReset.Add(heroBox);
                }
            }
            if (stateName == "Idle" || stateName == "Grab Anim") {
                resetLocalRotate();
            }

        }
        static IEnumerator resetLocalRotateDelay() {
            yield return new WaitForSeconds(0.7f);
            //Debug.Log("resetLocalRotateDelay-----");
            resetLocalRotate();
        }

        static bool isNearTarget;
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SetVelocityAsAngle), "OnEnter")]
        public static void HeroControllerMethods_DoSetVelocity_pre(SetVelocityAsAngle __instance) {
            if (!AimingManager.UseHarpoonDashAiming()) return;
            disLast = (needleTarget - (Vector2)Main.gm.hero_ctrl.transform.position).magnitude;
            isNearTarget = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SetVelocityAsAngle), "DoSetVelocity")]
        public static void SetVelocityAsAngle_Dash(SetVelocityAsAngle __instance) {
            if (!AimingManager.UseHarpoonDashAiming()) return;
            if (!__instance.State.Name.ToString().Contains("Dash")) return;

            GameObject obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject);
            //Transform transform = obj.transform;
            //__instance.angle.Value = AngleDash;
            var hero = Main.hero;
            hero.GetComponent<Rigidbody2D>().linearVelocity = DirDash * __instance.speed.Value;
            //__instance.speed.Value = 100;
        }

        static float disLast;
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ClampVelocity2D), "DoClampVelocity")]
        public static bool ClampVelocity2D_Dash(ClampVelocity2D __instance) {
            if (!__instance.State.Name.ToString().Contains("Dash")) return true;
            if (!AimingManager.UseHarpoonDashAiming()) return true;
            //return false;
            float dis = (needleTarget - (Vector2)Main.gm.hero_ctrl.transform.position).magnitude;
            float maxVel = 5;
            if (disLast - dis > 0 && dis < 2) {
                isNearTarget = true;
            }
            if (dis < 2) {
                resetLocalRotate();
            }
            disLast = dis;
            var hero = Main.hero;
            var rb = hero.GetComponent<Rigidbody2D>();
            Vector2 linearVelocity = rb.linearVelocity;
            if (isNearTarget) {
                if (linearVelocity.x < -maxVel) linearVelocity.x =  -maxVel;
                else if (linearVelocity.x > maxVel) linearVelocity.x =  maxVel;
                rb.linearVelocity = linearVelocity;
            }

            return false;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tk2dPlayAnimationWithEvents), "DoPlayAnimationWithEvents")]
        public static void catch_begin(Tk2dPlayAnimationWithEvents __instance) {
            if (!AimingManager.UseHarpoonDashAiming()) return;
            //Debug.Log(__instance.State.Name.ToString());
            string stateName = __instance.State.Name.ToString();
            if (!stateName.Contains("Catch")) return;
            GameObject obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject);
            var spriteAnimator = obj.GetComponent<tk2dSpriteAnimator>();
            //Debug.Log("Tk2dPlayAnimationWithEvents-------------: " + stateName);
            //Debug.Log(__instance.clipName);
            //Debug.Log(__instance.animationCompleteEvent.Name);

            var _sprite = obj.GetComponent<tk2dSpriteAnimator>();
            var heroAnim = obj.GetComponent<IHeroAnimationController>();
            string clipName = __instance.clipName.Value;
            tk2dSpriteAnimationClip clip = ((heroAnim != null) ? heroAnim.GetClip(clipName) : _sprite.GetClipByName(clipName));

            if (clipName == "Harpoon Catch") {

                float angle = AngleDash;
                if (DirDash.x < 0) {
                    angle += 180;
                }
                if (DirDash.y < 0) {
                    //var y = heroBox.transform.localRotation.y;
                    Collider2D collider2D = obj.GetComponent<Collider2D>();
                    //collider2D.offset += Vector2.up * 10f;
                    //Debug.Log("HeroColl Y: " + collider2D.offset);
                }

                //var be = obj.gameObject.GetComponents<MonoBehaviour>();
                //List<string> list = new List<string>();
                //foreach (var item in be) {
                //    list.Add(item.GetType().Name);
                //}

                //Debug.Log(string.Join(",", list));
                obj.transform.SetLocalRotation2D(angle);
                //heroBox.transform.SetRotation2D(0);
                needResetLocalRotate = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tk2dPlayAnimationWithEvents), "OnExit")]
        public static void catch_end(Tk2dPlayAnimationWithEvents __instance) {
            if (!AimingManager.UseHarpoonDashAiming()) return;
            string stateName = __instance.State.Name.ToString();
            if (!stateName.Contains("Catch")) return;
            GameObject obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject);
            var spriteAnimator = obj.GetComponent<tk2dSpriteAnimator>();
            //Debug.Log("Tk2dPlayAnimationWithEvents OnExit-------------: " + stateName);
            //Debug.Log(__instance.clipName);
            //Debug.Log(__instance.animationCompleteEvent.Name);

            var _sprite = obj.GetComponent<tk2dSpriteAnimator>();
            var heroAnim = obj.GetComponent<IHeroAnimationController>();
            string clipName = __instance.clipName.Value;
            tk2dSpriteAnimationClip clip = ((heroAnim != null) ? heroAnim.GetClip(clipName) : _sprite.GetClipByName(clipName));
            if (__instance.clipName.Value == "Harpoon Catch") {
                resetLocalRotate();
            }
        }

    }
}
