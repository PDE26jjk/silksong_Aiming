using HarmonyLib;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using System.Linq;
using System;

namespace silksong_Aiming {
    class SilkSpear_Patcher {
        static GameObject SpearObj;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HeroControllerMethods), "OnEnter")]
        public static void FlipAndRotation(HeroControllerMethods __instance) {
            string stateName = __instance.State.Name.ToString();
            if (!stateName.Contains("throw Flip?")) return;
            var hero = Main.hero;
            try {
                SpearObj.transform.SetLocalRotation2D(0);
            }
            catch (Exception e) {
                SpearObj = hero.transform.Find("Special Attacks").Find("Needle Throw").gameObject;
            }
            if (!AimingManager.IsAiming) return;
            

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
            SpearObj.transform.SetRotation2D(angle);
        }
    }
}
