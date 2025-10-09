using GlobalSettings;
using HarmonyLib;
using InControl;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace silksong_Aiming {
    class HeroInput_Patcher {
        private static FieldInfo thisStateInfo;
        private static FieldInfo lastStateInfo;
        private static FieldInfo willThrowToolInfo;
        private static MethodInfo canThrowMethod;
        public static bool AttackWasReleased = true;
        private static void SetInputIsPressed(PlayerAction action, bool value = true) {
            thisStateInfo ??= AccessTools.Field(typeof(PlayerAction), "thisState");
            var thisState = (InputControlState)thisStateInfo.GetValue(action);
            thisState.State = value;
            thisStateInfo.SetValue(action, thisState);
        }
        private static void SetInputWasPressed(PlayerAction action, bool value = true) {
            SetInputIsPressed(action, true);
            lastStateInfo ??= AccessTools.Field(typeof(PlayerAction), "lastState");
            var lastState = (InputControlState)lastStateInfo.GetValue(action);
            lastState.State = !value;
            lastStateInfo.SetValue(action, lastState);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "LookForQueueInput")]
        public static void AttackKeyOverlay(HeroController __instance) {
            if (!AimingManager.UseAttackKeyOverlay()) return;
            HeroActions inputActions = Main.gm.inputHandler.inputActions;
            if (AimingManager.AttackKeyActive < 4) {
                //Debug.LogWarning("HeroController_LookForQueueInput_pre"+inputActions.Attack.WasPressed);
                //if (inputActions.Attack.WasPressed || inputActions.Attack.WasReleased) {
                //}
                if (inputActions.Attack.WasReleased) {
                    AttackWasReleased = true;
                }
                else
                if (AttackWasReleased && (inputActions.Attack.WasPressed)) {
                    AttackWasReleased = false;
                    //Debug.Log("click attack---------------------------");
                    //Debug.Log(inputActions.Attack.IsPressed);

                    SetInputWasPressed(inputActions.Attack, false);
                    //SetInputIsPressed(inputActions.Attack, false);
                    SetInputWasPressed(inputActions.QuickCast);
                    return;
                }
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "GetWillThrowTool")]
        public static bool HeroController_GetWillThrowTool_pre(ref bool __result, HeroController __instance, bool reportFailure) {
            if (!AimingManager.IsAiming || AimingManager.AttackKeyActive >= 4) return true;
            //HeroActions inputActions = Main.gm.inputHandler.inputActions;
            //if (!inputActions.Attack.IsPressed) return true;

            //Debug.Log("GetWillThrowTool......................");
            __result = false;
            willThrowToolInfo ??= AccessTools.Field(typeof(HeroController), "willThrowTool");
            ToolItem tool = null;
            AttackToolBinding attackToolBinding = 0;
            switch (AimingManager.AttackKeyActive) {
                case 1:
                    tool = ToolItemManager.GetBoundAttackTool(AttackToolBinding.Up, ToolEquippedReadSource.Active, out attackToolBinding);
                    break;
                case 2:
                    tool = ToolItemManager.GetBoundAttackTool(AttackToolBinding.Neutral, ToolEquippedReadSource.Active, out attackToolBinding);
                    break;
                case 3:
                    tool = ToolItemManager.GetBoundAttackTool(AttackToolBinding.Down, ToolEquippedReadSource.Active, out attackToolBinding);
                    break;
            }
            //Debug.Log(tool.Type);
            willThrowToolInfo.SetValue(__instance, tool);
            canThrowMethod ??= AccessTools.Method(typeof(HeroController), "CanThrowTool", new Type[] {
                    typeof(ToolItem), typeof(AttackToolBinding), typeof(bool)
                });
            bool canThrowTool = (bool)canThrowMethod.Invoke(__instance, new object[] { tool, attackToolBinding, reportFailure });
            __result = tool && canThrowTool;
            //Debug.Log(__result);
            //Debug.Log(canThrowTool);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "DoAttack")]
        public static bool PreventAttackFloating(HeroController __instance) {
            if (!AimingManager.UseAttackKeyOverlay()) return true;
            //Debug.Log("no DoAttack...............");
            HeroActions inputActions = Main.gm.inputHandler.inputActions;
            //Debug.Log(inputActions.Attack.IsPressed);
            //Debug.Log(__instance.fsm_brollyControl.ActiveStateName);
            //Debug.Log(__instance.umbrellaFSM.ActiveStateName);
            if (AimingManager.AttackKeyActive != 4 && __instance.fsm_brollyControl.ActiveStateName != "Idle") {
                return false;
            }
            return true;
        }
        static bool downAttack = false;
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "DoAttack")]
        public static bool AttackDirection(HeroController __instance) {
            if (!AimingManager.IsAiming) return true;
            if (AimingManager.UseAttackKeyOverlay() && AimingManager.AttackKeyActive != 4) return true;

            HeroActions inputActions = Main.gm.inputHandler.inputActions;

            var facingRight = __instance.cState.facingRight;
            AimingManager.RefreshMousePosition();
            var dir = AimingManager.GetDirectionToMouse(__instance.transform.position);
            var angle = AimingManager.GetAngleToMouse(__instance.transform.position);
            //Debug.Log("isSprinting: "+ __instance.cState.isSprinting);
            //if(__instance.hero_state == GlobalEnums.ActorStates.running) {
            //    __instance.hero_state = GlobalEnums.ActorStates.idle;
            //}

            //SetInputWasPressed(inputActions.Left, Dir.x < 0);
            //SetInputWasPressed(inputActions.Right, Dir.x > 0);
            //SetInputWasPressed(inputActions.Up, false);
            //SetInputWasPressed(inputActions.Down,false);
            downAttack = false;
            int angle2y = Math.Abs(((Math.Abs((int)angle) % 180)) - 90);
            if (dir.y > 0 && angle2y < Settings.UpSlashAngle.Value) {
                SetInputIsPressed(inputActions.Up, true);
                SetInputIsPressed(inputActions.Down, false);
            }
            else if (dir.y < 0 && angle2y < Settings.DownSlashAngle.Value) {
                SetInputIsPressed(inputActions.Up, false);
                SetInputIsPressed(inputActions.Down, true);
                downAttack = true;
            }
            else {
                SetInputIsPressed(inputActions.Up, false);
                SetInputIsPressed(inputActions.Down, false);
            }
            __instance.GetComponent<Rigidbody2D>().linearVelocityX = 0;
            if (!__instance.cState.wallSliding) {

                if (Mathf.Sign(dir.x * (facingRight ? 1 : -1)) < 0) {
                    //Debug.Log("aaaaaaaaaaaaaa AttackDirection FlipSprite");
                    //Debug.Log("cState.altAttack: " + __instance.cState.altAttack);
                    __instance.FlipSprite();
                }
            }
            //__instance.currentSlashDamager.
            //Debug.Log(__instance.fsm_brollyControl.ActiveStateName);
            //Debug.Log(__instance.umbrellaFSM.ActiveStateName);

            return true;
        }
        static bool preventCancelAttack = false;
        static FieldInfo downSpikeHorizontalSpeedInfo;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HeroController), "DoAttack")]
        public static void AttackDirection2(HeroController __instance) {
            if (!AimingManager.IsAiming) return;
            if (AimingManager.UseAttackKeyOverlay() && AimingManager.AttackKeyActive != 4) return;
            if (__instance.cState.wallSliding) { return; }

            //if (downAttack) return;
            HeroActions inputActions = Main.gm.inputHandler.inputActions;
            var facingRight = __instance.cState.facingRight;
            AimingManager.RefreshMousePosition();
            var Dir = AimingManager.GetDirectionToMouse(__instance.transform.position);
            //__instance.GetComponent<Rigidbody2D>().linearVelocityX = 0;

            preventCancelAttack = false;
            // FIXME SpellCrest turn slash
            if (Mathf.Sign(Dir.x * (facingRight ? 1 : -1)) < 0 && !Gameplay.SpellCrest.IsEquipped) {
                __instance.FlipSprite();
                preventCancelAttack = true;
            }
            if (downAttack) {
                downSpikeHorizontalSpeedInfo ??= __instance.GetType()
                               .GetField("downSpikeHorizontalSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
                downSpikeHorizontalSpeedInfo.SetValue(__instance, __instance.Config.DownspikeSpeed * Mathf.Sign(Dir.x));
            }
            //Debug.Log(__instance.fsm_brollyControl.ActiveStateName);
            //Debug.Log(__instance.umbrellaFSM.ActiveStateName);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NailSlashTravel), "Start")]
        public static void PlaySlashStart(NailSlashTravel __instance) {

            Vector2 pos = __instance.transform.position;
            //DebugLineRenderer.DrawLine(pos + Vector2.up * 5, pos + Vector2.up * -5, Color.green, 2);
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(NailSlash), "PlaySlash")]
        //public static bool PlaySlash(NailSlash __instance) {
        //    Type type = __instance.GetType();
        //    //var travel = type.BaseType.GetField("travel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as NailSlashTravel;
        //    var mesh = type.GetField("mesh", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as MeshRenderer;
        //    if (mesh) {
        //        mesh.enabled = true;
        //    }
        //    var anim = type.GetField("anim", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as tk2dSpriteAnimator;
        //    tk2dSpriteAnimationClip clipByName = anim.GetClipByName(__instance.animName);
        //    anim.Play(clipByName, Mathf.Epsilon, clipByName.fps * 1);

        //    Debug.Log("PlaySlash anim: " + anim);
        //    Debug.Log("PlaySlash local scale: " + __instance.transform.localScale.x);
        //    Debug.Log("PlaySlash local postion: " + __instance.transform.localPosition.x);
        //    __instance.transform.SetScaleX(-1);
        //    //anim.transform
        //    Vector2 throwPointPos = mesh.bounds.center;
        //    DebugLineRenderer.DrawLine(throwPointPos + Vector2.up * 5, throwPointPos + Vector2.up * -5, Color.green, 2);
        //    return false;
        //}
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NailSlash), "CancelAttack", new Type[] { })]
        public static bool CancelCancelAttack(NailSlash __instance) {
            if (!AimingManager.IsAiming) return true;
            if (AimingManager.UseAttackKeyOverlay() && AimingManager.AttackKeyActive != 4) return true;
            if (downAttack) { return true; }
            //Debug.Log("WitchCrest"+Gameplay.WitchCrest.IsEquipped);
            //Debug.Log("WarriorCrest"+Gameplay.WarriorCrest.IsEquipped);
            //Debug.Log("WandererCrest"+Gameplay.WandererCrest.IsEquipped);
            //Debug.Log("ToolmasterCrest"+Gameplay.ToolmasterCrest.IsEquipped);
            //Debug.Log("ReaperCrest"+Gameplay.ReaperCrest.IsEquipped);
            //Debug.Log("SpellCrest"+Gameplay.SpellCrest.IsEquipped);
            if (Gameplay.SpellCrest.IsEquipped) return true;
            //Debug.Log("CancelCancelAttack_____----");
            //if (preventCancelAttack) {
            //    preventCancelAttack = false;
            //    return false;
            //}
            return false;
        }
        //        [HarmonyPrefix]
        //[HarmonyPatch(typeof(HeroShamanRuneEffect), "Refresh", new Type[] { })]
        //public static bool t1(HeroShamanRuneEffect __instance) {
        //    if (!AimingManager.IsAiming) return true;
        //    if (AimingManager.UseAttackKeyOverlay() && AimingManager.AttackKeyActive != 4) return true;
        //     return false;
        //}
    }
}
