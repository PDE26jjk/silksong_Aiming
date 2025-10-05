using HarmonyLib;
using InControl;
using System;
using System.Reflection;
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
    }
}
