using HarmonyLib;
using HutongGames.PlayMaker;
using InControl;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace silksong_Aiming {
    public class InputController : MonoBehaviour {
        public float radius = 0.1f;
        public Color color = Color.red;
        public float width = 0.05f;
        public int segments = 32;

        private LineRenderer lineRenderer;
        private bool UseDPadToChangeTools;

        void Start() {
            // 创建 LineRenderer
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.positionCount = segments + 1;
            lineRenderer.loop = true;

            // 设置材质
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            color = Settings.GetColor("CrosshairColor", color);
            radius = Settings.GetFloat("CrosshairSize", radius);
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            ToggleAimingKey = Settings.GetKeyCode("ToggleAimingKey", KeyCode.Z);
            SwitchToUpSkillKey = Settings.GetKeyCode("SwitchToUpSkillKey", KeyCode.Alpha1);
            SwitchToMiddleSkillKey = Settings.GetKeyCode("SwitchToMiddleSkillKey", KeyCode.Alpha2);
            SwitchToDownSkillKey = Settings.GetKeyCode("SwitchToDownSkillKey", KeyCode.Alpha3);
            SwitchToAttackKey = Settings.GetKeyCode("SwitchToAttackKey", KeyCode.Alpha4);

            AimingManager.ReplaceAttackKey = Settings.GetBool("ReplaceAttackKey", true);
            AimingManager._UseHarpoonDashAiming = Settings.GetBool("UseHarpoonDashAiming", false);
            UseDPadToChangeTools = Settings.GetBool("UseDPadToChangeTools", true);

        }
        private KeyCode ToggleAimingKey;
        private KeyCode SwitchToUpSkillKey;
        private KeyCode SwitchToMiddleSkillKey;
        private KeyCode SwitchToDownSkillKey;
        private KeyCode SwitchToAttackKey;

        private bool useTimeSlow = false;
        static bool lastIsAiming;
        static int lastAttackKeyActive;

        void Update() {
            lineRenderer.enabled = AimingManager.IsAiming && !AimingManager.UsingJoystick;

            if (!UnityEngine.Camera.main) { return; }
            if (!Main.gm || !Main.gm.hero_ctrl || Main.gm.hero_ctrl.IsPaused()) {
                return;
            }
            GetInput();
            AimingManager.ShouldUpdateHUD = (lastIsAiming != AimingManager.IsAiming || lastAttackKeyActive != AimingManager.AttackKeyActive);
            lastIsAiming = AimingManager.IsAiming;
            lastAttackKeyActive = AimingManager.AttackKeyActive;

            if (!AimingManager.IsAiming) {
                return;
            }
            // 获取鼠标世界位置
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            // 更新圆的位置
            DrawCircle(mouseWorldPos);
        }
        public void GetInput() {

            if (!Main.gm || !Main.gm.hero_ctrl || Main.gm.hero_ctrl.IsPaused()) {
                return;
            }
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Alpha6)) {
                useTimeSlow = !useTimeSlow;
            }
            if (useTimeSlow) {
                Time.timeScale = 0.2f;
            }
            else {
                Time.timeScale = 1.0f;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5)) {
                var hero = Main.gm.hero_ctrl;
                if (hero == null) { return; }
                //hero.harpoonDashFSM.SendEventSafe("DO MOVE");
                string activeStateName = hero.harpoonDashFSM.ActiveStateName;

                //toolEventTarget.SendEvent(usage.FsmEventName);
                var event1 = FsmEvent.GetFsmEvent("DO MOVE");
                var fsm = hero.harpoonDashFSM.Fsm;
                //fsm.ProcessEvent(event1);
                if (!fsm.Started) {
                    fsm.Start();
                }
                FsmExecutionStack.PushFsm(fsm);
                //Debug.Log(fsm.ActiveState.Transitions.Length);
                Debug.Log("active :" + fsm.ActiveState.Name);
                foreach (var state in fsm.States) {
                    Debug.Log(state.Name);
                    foreach (var _trans in state.Transitions) {
                        //FsmState _state = _trans.ToFsmState;
                        Debug.Log("    " + _trans.EventName + "-->" + _trans.ToFsmState.Name);
                    }
                }
                //foreach (var trans in fsm.ActiveState.Transitions) {
                //    if (trans.FsmEvent == event1) {
                //        FsmState state = trans.ToFsmState;
                //        Debug.Log(state.Name);
                //        fsm.SwitchState(state);
                //        Debug.Log(state.Transitions.Length);
                //        //state = state.Transitions[1].ToFsmState;
                //        Debug.Log(state.Name);
                //        //AccessTools.Field(typeof(Fsm), "activeState").SetValue(fsm, state);
                //        //var EnterStateMethod = AccessTools.Method(typeof(Fsm), "EnterState");
                //        //EnterStateMethod.Invoke(fsm, new object[] { state });
                //        //fsm.StateChanged(state);
                //        state.OnEnter();
                //        AccessTools.Field(typeof(FsmState), "active").SetValue(state, true);
                //        //state.Actions = new FsmStateAction[0];
                //        Debug.Log(state.Transitions.Length);

                //        foreach (var _trans in state.Transitions) {
                //            //FsmState _state = _trans.ToFsmState;
                //            Debug.Log(_trans.EventName + "-->" + _trans.ToFsmState.Name);
                //        }
                //        //state.OnExit();
                //        //fsm.SwitchState(state);
                //    }
                //}

                FsmExecutionStack.PopFsm();

                //Debug.Log("hero state:");
                //Debug.Log(hero.harpoonDashFSM.ActiveStateName);
                //Debug.Log(hero.fsm_brollyControl.ActiveStateName);
                //Debug.Log(hero.fsm_fallTrail.ActiveStateName);
                //Debug.Log("all state:");
                //foreach (var state in hero.harpoonDashFSM.FsmStates)
                //{
                //    Debug.Log("state: "+state.Name);
                //    foreach (var action in state.Actions)
                //    {
                //        Debug.Log(action);
                //    }

                //}
            }
            if (Input.GetKeyDown(KeyCode.Alpha7)) {
                var hero = Main.gm.hero_ctrl;
                Debug.Log("harpoonDashFSM : " + hero.harpoonDashFSM.ActiveStateName);
                Debug.Log("Action : " + hero.harpoonDashFSM.Fsm.ActiveState.ActiveAction);
            }

#endif

            if (Input.GetKeyDown(ToggleAimingKey)) {
                if (AimingManager.UsingJoystick && AimingManager.IsAiming) {
                    AimingManager.UsingJoystick = false;
                }
                else {
                    AimingManager.IsAiming = !AimingManager.IsAiming;
                }
            }
            else
            //if (Input.GetKeyDown(KeyCode.Joystick1Button9)) {
            if (InputManager.ActiveDevice.LeftStickButton.WasPressed) {
                if (!AimingManager.UsingJoystick && AimingManager.IsAiming) {
                    AimingManager.UsingJoystick = true;
                }
                else {
                    AimingManager.IsAiming = !AimingManager.IsAiming;
                }
            }
            Vector3 joystickDir = InputManager.ActiveDevice.RightStick.Vector;
            if (joystickDir.magnitude >= 0.5 && !AimingManager.UsingJoystick && AimingManager.IsAiming) {
                AimingManager.UsingJoystick = true;
            }
            if (!AimingManager.IsAiming) { return; }
            if (Input.GetKeyDown(SwitchToUpSkillKey)) {
                //Debug.Log("111111111");
                AimingManager.AttackKeyActive = 1;
            }
            else if (Input.GetKeyDown(SwitchToMiddleSkillKey)) {
                //Debug.Log("22222222222");
                AimingManager.AttackKeyActive = 2;
            }
            else if (Input.GetKeyDown(SwitchToDownSkillKey)) {
                //Debug.Log("3333333333");
                AimingManager.AttackKeyActive = 3;
            }
            else if (Input.GetKeyDown(SwitchToAttackKey)) {
                //Debug.Log("444444444");
                AimingManager.AttackKeyActive = 4;
            }
            if (UseDPadToChangeTools) {
                if (InputManager.ActiveDevice.DPadUp.WasPressed) {
                    //Debug.Log("111111111");
                    AimingManager.AttackKeyActive = 1;
                }
                else if (InputManager.ActiveDevice.DPadRight.WasPressed) {
                    //Debug.Log("22222222222");
                    AimingManager.AttackKeyActive = 2;
                }
                else if (InputManager.ActiveDevice.DPadDown.WasPressed) {
                    //Debug.Log("3333333333");
                    AimingManager.AttackKeyActive = 3;
                }
                else if (InputManager.ActiveDevice.DPadLeft.WasPressed) {
                    //Debug.Log("444444444");
                    AimingManager.AttackKeyActive = 4;
                }
            }
        }

        Vector3 GetMouseWorldPosition() {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = UnityEngine.Camera.main.nearClipPlane + 0.1f;
            return UnityEngine.Camera.main.ScreenToWorldPoint(mousePos);
            //Vector3 mousePos = Input.mousePosition;
            //mousePos.z = -UnityEngine.Camera.main.transform.position.z;
            //Vector3 MousePosW = UnityEngine.Camera.main.ScreenToWorldPoint(mousePos);
            //MousePosW.z = UnityEngine.Camera.main.nearClipPlane - 100f;
        }

        void DrawCircle(Vector3 center) {
            float angle = 0f;
            float angleIncrement = 360f / segments;

            for (int i = 0; i <= segments; i++) {
                Vector3 point = center + Quaternion.Euler(0, 0, angle) * Vector3.right * radius;
                lineRenderer.SetPosition(i, point);
                angle += angleIncrement;
            }
        }
    }
}
