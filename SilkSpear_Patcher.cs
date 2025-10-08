using HarmonyLib;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using System.Linq;
using System;

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

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(Tk2dPlayAnimation), "DoPlayAnimation")]
        //public static void t1(Tk2dPlayAnimation __instance) {
        //    string stateName = __instance.State.Name.ToString();
        //    //if (!__instance.State.Name.ToString().Contains("Throw")) return;

        //    GameObject obj = __instance.Fsm.GetOwnerDefaultTarget(__instance.gameObject);
        //    var spriteAnimator = obj.GetComponent<tk2dSpriteAnimator>();
        //    //Debug.Log("Tk2dPlayAnimation-------------: " + stateName);
        //    //Debug.Log(__instance.clipName);
        //    if (__instance.clipName.ToString() == "NeedleThrow Throwing") {
        //        //var hero = Main.hero;
        //        //var dir = AimingManager.GetDirectionToMouse(hero.transform.position);
        //        //var angle = AimingManager.GetAngleToMouse(hero.transform.position);
        //        //bool facingRight = hero.cState.facingRight;
        //        ////if (!.cState.wallSliding) {

        //        //if (Mathf.Sign(dir.x * (facingRight ? 1 : -1)) < 0) {
        //        //    hero.FlipSprite();
        //        //}
        //        //}
        //    }

        //}
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

            if ((Mathf.Sign(dir.x * (facingRight ? 1 : -1)) < 0) && !hero.cState.wallSliding) {
                hero.FlipSprite();
            }
            if (dir.x < 0) {
                angle += 180;
            }
            if (hero.cState.wallSliding) {
                angle = -angle;
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
}
