using HarmonyLib;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using System.Linq;
using System;
using System.Collections;

namespace silksong_Aiming {
    class SilkSpear_Patcher {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActivateGameObject), "DoActivateGameObject")]
        public static void t1(ActivateGameObject __instance) {
            string stateName = __instance.State.Name.ToString();
            var obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject); ;
            if (!obj) return;
            //Debug.Log("ActivateGameObject: " + stateName + ":" + obj.name);
            if (stateName == "Cancel" && obj.name == "Followup Slash") {
                //foreach (var state in __instance.State.Fsm.States) {
                //    Debug.Log(state.Name);
                //    foreach (var _trans in state.Transitions) {
                //        //FsmState _state = _trans.ToFsmState;
                //        Debug.Log("    " + _trans.EventName + "-->" + _trans.ToFsmState.Name);
                //    }
                //}
            }
            string[] tohandle = { "Harpoon Breaker", "Harpoon Breaker Extend" };
            if (tohandle.Contains(obj.name)) {

            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tk2dPlayAnimation), "DoPlayAnimation")]
        public static void t1(Tk2dPlayAnimation __instance) {
            string stateName = __instance.State.Name.ToString();
            //if (!__instance.State.Name.ToString().Contains("Throw")) return;

            GameObject obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject);
            var spriteAnimator = obj.GetComponent<tk2dSpriteAnimator>();
            Debug.Log("Tk2dPlayAnimation-------------: " + stateName);
            Debug.Log(__instance.clipName);
            if (__instance.clipName.ToString() == "NeedleThrow Throwing") {
                //var hero = Main.hero;
                //var dir = AimingManager.GetDirectionToMouse(hero.transform.position);
                //var angle = AimingManager.GetAngleToMouse(hero.transform.position);
                //bool facingRight = hero.cState.facingRight;
                ////if (!.cState.wallSliding) {

                //if (Mathf.Sign(dir.x * (facingRight ? 1 : -1)) < 0) {
                //    hero.FlipSprite();
                //}
                //}
            }

        }
        static GameObject SpearObj;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HeroControllerMethods), "OnEnter")]
        public static void FlipAndRotation(HeroControllerMethods __instance) {
            string stateName = __instance.State.Name.ToString();
            if (!__instance.State.Name.ToString().Contains("throw Flip?")) return;
            var hero = Main.hero;

            var dir = AimingManager.GetDirectionToMouse(hero.transform.position);
            var angle = AimingManager.GetAngleToMouse(hero.transform.position);
            bool facingRight = hero.cState.facingRight;
            //if (!.cState.wallSliding) {

            if (Mathf.Sign(dir.x * (facingRight ? 1 : -1)) < 0) {
                hero.FlipSprite();
            }
            if (dir.x < 0) {
                angle += 180;
            }
            try {
                SpearObj.transform.SetRotation2D(0);
            }
            catch (Exception e) {
                SpearObj = hero.transform.Find("Special Attacks").Find("Needle Throw").gameObject;
            }
            SpearObj.transform.SetRotation2D(angle);
        }
    }
    class SilkCharge_Patcher {
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(ActivateGameObject), "DoActivateGameObject")]
        //public static void t1(ActivateGameObject __instance) {
        //    string stateName = __instance.State.Name.ToString();
        //    var obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject); ;
        //    if (!obj) return;
        //    Debug.Log("ActivateGameObject: " + stateName + ":" + obj.name);
        //    if (stateName == "Silk Charge Start" && obj.name == "Silk Charge DashBurst") {
        //    }
        //    if (tohandle.Contains(obj.name)) {

        //    }
        //}
        static Vector2 Dir;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GetHeroCState), "OnEnter")]
        public static void FlipAndRotation(HeroControllerMethods __instance) {
            string stateName = __instance.State.Name.ToString();
            if (!__instance.State.Name.ToString().Contains("Charge Flip?")) return;
            var hero = Main.hero;
            Dir = AimingManager.GetDirectionToMouse(hero.transform.position);
            var angle = AimingManager.GetAngleToMouse(hero.transform.position);
            bool facingRight = hero.cState.facingRight;
            //if (!.cState.wallSliding) {

            if (Mathf.Sign(Dir.x * (facingRight ? 1 : -1)) < 0) {
                hero.FlipSprite();
            }
            //if (dir.x < 0) {
            //    angle += 180;
            //}
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tk2dPlayAnimationWithEvents), "DoPlayAnimationWithEvents")]
        public static void catch_begin(Tk2dPlayAnimationWithEvents __instance) {
            string stateName = __instance.State.Name.ToString();
            if (!stateName.Contains("Silk Charge")) return;
            GameObject obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject);
            var spriteAnimator = obj.GetComponent<tk2dSpriteAnimator>();
            Debug.Log("Tk2dPlayAnimationWithEvents-------------: " + stateName);
            Debug.Log(__instance.clipName);
            var hero = Main.hero;
            var angle = AimingManager.GetAngleToMouse(hero.transform.position);
            if (Dir.x < 0) {
                angle += 180;
            }
            if (stateName == "Silk Charge Start" && __instance.clipName.ToString() == "Silk Charge") {
                spriteAnimator.AnimationChanged += SpriteAnimator_AnimationChanged;
                spriteAnimator.AnimationCompletedEvent += SpriteAnimator_AnimationCompletedEvent;
                obj.transform.SetLocalRotation2D(angle);
            }
            //Debug.Log(__instance.animationCompleteEvent.Name);
        }

        private static void SpriteAnimator_AnimationCompletedEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip) {
            animator.AnimationCompletedEvent -= SpriteAnimator_AnimationCompletedEvent;
            Debug.Log("SpriteAnimator_AnimationCompletedEvent " + clip.name);
            var hero = Main.hero;
            //hero.transform.localRotation = Quaternion.identity;
        }

        private static void SpriteAnimator_AnimationChanged(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip previousClip, tk2dSpriteAnimationClip newClip) {
            Debug.Log("SpriteAnimator_AnimationChanged " + newClip.name);
            animator.AnimationChanged -= SpriteAnimator_AnimationChanged;
            var hero = Main.hero;
            hero.StartCoroutine(resetRotation(hero));
        }
        private static IEnumerator resetRotation(HeroController hero) {
            yield return new WaitForSeconds(0.05f);
            hero.transform.localRotation = Quaternion.identity;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(SetVelocity2d), "OnEnter")]
        public static void Rotation(SetVelocity2d __instance) {
            string stateName = __instance.State.Name.ToString();
            if (!stateName.Contains("Silk Charge Start")) return;
            var hero = Main.hero;
            __instance.everyFrame = true;
            //var dir = AimingManager.GetDirectionToMouse(hero.transform.position);
            Debug.Log("Charge SetVelocity2d: OnEnter " + __instance.vector);
            __instance.vector = Dir * __instance.vector.Value.magnitude;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SetVelocity2d), "DoSetVelocity")]
        public static void Rotation2(SetVelocity2d __instance) {
            string stateName = __instance.State.Name.ToString();
            if (!stateName.Contains("Silk Charge Start")) return;
            var hero = Main.hero;
            __instance.everyFrame = true;
            //var dir = AimingManager.GetDirectionToMouse(hero.transform.position);
            var angle = AimingManager.GetAngleToMouse(hero.transform.position);
            GameObject obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject);
            if (Dir.x < 0) {
                angle += 180;
            }
            obj.transform.SetRotation2D(angle);

            Debug.Log("Charge SetVelocity2d: " + __instance.vector);
        }
    }
}
