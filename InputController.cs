using BepInEx.Configuration;
using HarmonyLib;
using HutongGames.PlayMaker;
using InControl;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace silksong_Aiming {
    public class InputController : MonoBehaviour {
        public float radius = 0.1f;
        public Color color = Color.red;
        public float width = 0.05f;
        public int segments = 32;

        private LineRenderer lineRenderer;

        public void waitForMain() {
            if (Main.hasSetup) {
                // 键盘快捷键配置
                ToggleAimingKey = Settings.ToggleAimingKey.Value.MainKey;
                SwitchToUpSkillKey = Settings.SwitchToUpSkillKey.Value.MainKey;
                SwitchToMiddleSkillKey = Settings.SwitchToMiddleSkillKey.Value.MainKey;
                SwitchToDownSkillKey = Settings.SwitchToDownSkillKey.Value.MainKey;
                SwitchToAttackKey = Settings.SwitchToAttackKey.Value.MainKey;

                RegisterConfigCallbacks();
            }
            else {
                Invoke(nameof(waitForMain), 0.5f);
            }

        }
        void Awake() {
            Invoke(nameof(waitForMain), 0.5f);
        }

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
        }
        private void RegisterConfigCallbacks() {
            // ===== 键盘快捷键变更回调 =====
            Settings.ToggleAimingKey.SettingChanged += (sender, args) =>
                ToggleAimingKey = Settings.ToggleAimingKey.Value.MainKey;

            Settings.SwitchToUpSkillKey.SettingChanged += (sender, args) =>
                SwitchToUpSkillKey = Settings.SwitchToUpSkillKey.Value.MainKey;

            Settings.SwitchToMiddleSkillKey.SettingChanged += (sender, args) =>
                SwitchToMiddleSkillKey = Settings.SwitchToMiddleSkillKey.Value.MainKey;

            Settings.SwitchToDownSkillKey.SettingChanged += (sender, args) =>
                SwitchToDownSkillKey = Settings.SwitchToDownSkillKey.Value.MainKey;

            Settings.SwitchToAttackKey.SettingChanged += (sender, args) =>
                SwitchToAttackKey = Settings.SwitchToAttackKey.Value.MainKey;

            Settings.CrosshairImageFile.SettingChanged += (sender, args) => {
                Texture2D texture = null;
                if (TryLoadCursorFromFile(Settings.CrosshairImageFile.Value, out texture)) {
                    cursorTexture = texture;
                }
            };

        }

        private KeyCode ToggleAimingKey;
        private KeyCode SwitchToUpSkillKey;
        private KeyCode SwitchToMiddleSkillKey;
        private KeyCode SwitchToDownSkillKey;
        private KeyCode SwitchToAttackKey;

        private bool useTimeSlow = false;
        static bool lastIsAiming;
        static int lastAttackKeyActive;
        public Texture2D cursorTexture;

        void OnGUI() {
            if (!cursorTexture) return;
            // 获取鼠标位置
            Vector3 mousePos = Input.mousePosition;

            // 转换为GUI坐标（y轴翻转）
            mousePos.y = Screen.height - mousePos.y;

            // 绘制自定义光标
            GUI.DrawTexture(
                new Rect(mousePos.x, mousePos.y, Settings.CrosshairSize.Value, Settings.CrosshairSize.Value),
                cursorTexture
            );
        }
        private bool TryLoadCursorFromFile(string filePath, out Texture2D texture) {
            texture = null;

            try {
                // 1. 检查文件是否存在
                if (!File.Exists(filePath)) {
                    Debug.LogError($"文件不存在: {filePath}");
                    return false;
                }

                //// 2. 读取文件字节
                byte[] fileData = File.ReadAllBytes(filePath);

                //// 3. 创建纹理
                texture = new Texture2D(2, 2);

                // 4. 加载图像数据
                if (!texture.LoadImage(fileData)) {
                    Debug.LogError($"无法解析图像文件: {filePath}");
                    texture = null;
                    return false;
                }

                // 5. 检查纹理尺寸
                if (texture.width <= 0 || texture.height <= 0) {
                    Debug.LogError($"无效的光标尺寸: {texture.width}x{texture.height}");
                    texture = null;
                    return false;
                }

                return true;
            }
            catch (System.Exception ex) {
                Debug.LogError($"加载光标文件失败: {ex.Message}");
                texture = null;
                return false;
            }
        }
        void Update() {
            color = Settings.CrosshairColor.Value;
            color.a = Settings.CrosshairAlpha.Value;
            radius = Settings.CrosshairSize.Value;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
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

                FsmExecutionStack.PopFsm();

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
                    Settings.AimingEnabled.Value = !AimingManager.IsAiming;
                }
            }
            else
            //if (Input.GetKeyDown(KeyCode.Joystick1Button9)) {
            if (InputManager.ActiveDevice.LeftStickButton.WasPressed) {
                if (!AimingManager.UsingJoystick && AimingManager.IsAiming) {
                    AimingManager.UsingJoystick = true;
                }
                else {
                    Settings.AimingEnabled.Value = !AimingManager.IsAiming;
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
            if (Settings.UseDPadToChangeTools.Value) {
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
