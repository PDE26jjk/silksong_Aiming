using BepInEx.Configuration;
using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using InControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace silksong_Aiming {
    public class InputController : MonoBehaviour {
        public float radius = 0.1f;
        public Color color = Color.red;
        public float width = 0.05f;
        public int segments = 64;

        private LineRenderer lineRenderer;
        private SpriteRenderer textureRenderer;

        public void waitForMain() {
            if (Main.hasSetup) {
                // 键盘快捷键配置
                ToggleAimingKey = Settings.ToggleAimingKey.Value.MainKey;
                SwitchToUpSkillKey = Settings.SwitchToUpSkillKey.Value.MainKey;
                SwitchToMiddleSkillKey = Settings.SwitchToMiddleSkillKey.Value.MainKey;
                SwitchToDownSkillKey = Settings.SwitchToDownSkillKey.Value.MainKey;
                SwitchToAttackKey = Settings.SwitchToAttackKey.Value.MainKey;

                TryLoadCursorFromFile(Settings.CrosshairImageFile.Value);

                RegisterConfigCallbacks();
            }
            else {
                Invoke(nameof(waitForMain), 0.5f);
            }

        }
        void Awake() {
            Invoke(nameof(waitForMain), 0.5f);
            CreateCircleTexture();
            CreateDirTexture(ref dirTexture);
        }

        void Start() {
            // 创建 LineRenderer
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.positionCount = segments + 1;
            lineRenderer.loop = true;

            // 设置材质
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

            textureRenderer = gameObject.AddComponent<SpriteRenderer>();
            textureRenderer.sortingOrder = lineRenderer.sortingOrder;
            Debug.Log("lineRenderer.sortingOrder---------" + lineRenderer.sortingOrder);
            textureRenderer.enabled = false;
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
                TryLoadCursorFromFile(Settings.CrosshairImageFile.Value);
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
        private Texture2D cursorTexture;
        private Texture2D circleTexture;
        private Texture2D dirTexture;
        // 创建圆环纹理
        private void CreateCircleTexture() {
            int size = 256; // 纹理尺寸
            circleTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);

            // 设置透明背景
            Color transparent = new Color(0, 0, 0, 0);
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    circleTexture.SetPixel(x, y, transparent);
                }
            }

            // 绘制圆环
            Vector2 center = new Vector2(size / 2, size / 2);
            float radius = size / 2 - 5; // 留出边缘
            float thickness = 50; // 圆环厚度

            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist > radius - thickness && dist < radius) {
                        circleTexture.SetPixel(x, y, Color.white);
                    }
                }
            }

            circleTexture.Apply();
        }
        private void CreateDirTexture(ref Texture2D tex) {
            int size = 256; // 纹理尺寸
            tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
            float d = -0.1f;        // 两个圆心的距离
            float ra = 0.7f;       // 大圆半径
            float rb = 0.67f;
            float he = 0.4f;
            float raE = 0.07f;
            float rbE = 0.0f;
            float edgeSmoothness = 0.01f;
            // 计算缩放因子，使 SDF 在纹理空间中工作
            float scale = size / 2.0f;
            Vector2 center = new Vector2(size / 2, size / 2);

            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    // 将像素坐标转换为 SDF 空间
                    Vector2 p = new Vector2(
                        (x - center.x) / scale,
                        (y - center.y) / scale
                    );
                    // 计算有符号距离
                    float distance = sdMoon(p, d, ra, rb);
                    Vector2 p2 = p;
                    p2.x -= rb - 0.1f;
                    distance = Mathf.Min(distance, sdEgg(p2, he, raE, rbE));
                    // 根据距离设置颜色
                    float alpha = 1.0f - Mathf.Clamp01((distance + edgeSmoothness) / (2.0f * edgeSmoothness));
                    var moonColor = Color.white;
                    Color color = new Color(moonColor.r, moonColor.g, moonColor.b, alpha);
                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
        }
        // https://iquilezles.org/articles/distfunctions2d/
        private float sdMoon(Vector2 p, float d, float ra, float rb) {
            p.y = Mathf.Abs(p.y);
            float a = (ra * ra - rb * rb + d * d) / (2.0f * d);
            float b = Mathf.Sqrt(Mathf.Max(ra * ra - a * a, 0.0f));
            if (d * (p.x * b - p.y * a) > d * d * Mathf.Max(b - p.y, 0.0f))
                return Vector2.Distance(p, new Vector2(a, b));
            return Mathf.Max(
                Vector2.Distance(p, Vector2.zero) - ra,
                -(Vector2.Distance(p, new Vector2(d, 0)) - rb)
            );
        }
        private float sdEgg(Vector2 p, float he, float ra, float rb) {
            float ce = 0.5f * (he * he - (ra - rb) * (ra - rb)) / (ra - rb);
            p.y = Mathf.Abs(p.y);
            if (p.x < 0.0f) return Vector2.Distance(p, Vector2.zero) - ra;
            if (p.x * ce - p.y * he > he * ce)
                return Vector2.Distance(new Vector2(p.y, p.x - he), Vector2.zero) - rb;
            return Vector2.Distance(new Vector2(p.y + ce, p.x), Vector2.zero) - (ce + ra);
        }
        void OnGUI() {
            if (Settings.CrosshairVibration.Value || AimingManager.UsingJoystick) return;
            if (!Main.hero || Main.hero.IsPaused()) return;
            Color originalColor = GUI.color;
            Vector3 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;

            if (Settings.UseCrosshairImage.Value && cursorTexture) {
                GUI.color = new Color(1, 1, 1, Settings.CrosshairAlpha.Value);

                Vector2 imageSize = new Vector2(cursorTexture.width, cursorTexture.height) * Settings.CrosshairSize.Value * 10;

                // 绘制自定义光标
                GUI.DrawTexture(
                    new Rect(mousePos.x - imageSize.x / 2, mousePos.y - imageSize.y / 2, imageSize.x, imageSize.y),
                    cursorTexture
                );
            }
            else if (circleTexture) {
                Color c = Settings.CrosshairColor.Value;
                c.a = Settings.CrosshairAlpha.Value;
                GUI.color = c;

                Vector2 imageSize = new Vector2(1, 1) * Settings.CrosshairSize.Value * 900;

                // 绘制自定义光标
                GUI.DrawTexture(
                    new Rect(mousePos.x - imageSize.x / 2, mousePos.y - imageSize.y / 2, imageSize.x, imageSize.y),
                    circleTexture
                );
            }

            // 恢复原始颜色
            GUI.color = originalColor;
        }
        private bool TryLoadCursorFromFile(string filePath) {
            try {
                if (!filePath.StartsWith("http") && !filePath.StartsWith("\\"))
                    if (!File.Exists(filePath)) {
                        Debug.LogError($"文件不存在: {filePath}");
                        return false;
                    }
                StartCoroutine(LoadTextureRoutine(filePath, (img) => {
                    cursorTexture = img;
                    Debug.Log("cursor texture: " + cursorTexture.width + ":" + cursorTexture.height);
                    textureRenderer.sprite = Sprite.Create(
                        img,
                        new Rect(0, 0, img.width, img.height),
                        new Vector2(0.5f, 0.5f), // 中心锚点
                        100 // 每单位像素数
                    );
                }));
                return true;

            }
            catch (System.Exception ex) {
                Debug.LogError($"加载光标文件失败: {ex.Message}");
                return false;
            }
        }
        private IEnumerator LoadTextureRoutine(string filePath, System.Action<Texture2D> callback) {
            string url = filePath;
            if (!filePath.StartsWith("http"))
                url = "file://" + filePath.Replace("\\", "/");

            using (WWW www = new WWW(url)) {
                yield return www;

                if (string.IsNullOrEmpty(www.error)) {
                    callback?.Invoke(www.texture);
                }
                else {
                    Debug.LogError($"加载失败: {www.error}");
                    callback?.Invoke(null);
                }
            }
        }

        private GameObject JoystickDirObj;
        private SpriteRenderer JoystickDirRenderer;
        void Update() {
            if (!GameManager.SilentInstance || !Main.hero) return;
            try {
                JoystickDirRenderer.transform.parent.transform.ToString();
                Vector3 joystickDir = InputManager.ActiveDevice.RightStick.Vector;
                if (AimingManager.IsAiming && joystickDir.magnitude > 0.3 && Settings.UseDirectionIndicator.Value) {
                    JoystickDirRenderer.enabled = true;
                    float angle = Mathf.Atan2(joystickDir.y, joystickDir.x) * Mathf.Rad2Deg;
                    if (Main.hero.cState.facingRight) {
                        angle = 180 - angle;
                    }
                    JoystickDirObj.transform.SetLocalRotation2D(angle);
                    JoystickDirRenderer.material.color = new Color(1, 1, 1, Settings.DirectionAlpha.Value);
                    JoystickDirRenderer.sortingOrder = Settings.DirectionRenderOrder.Value;
                }
                else {
                    JoystickDirRenderer.enabled = false;
                    JoystickDirObj.transform.SetLocalRotation2D(90);
                }
            }
            catch (System.Exception) {
                JoystickDirObj = new GameObject("__JoystickDir");
                JoystickDirObj.transform.SetParent(Main.hero.transform);
                JoystickDirRenderer = JoystickDirObj.AddComponent<SpriteRenderer>();
                JoystickDirRenderer.sprite = Sprite.Create(
                        dirTexture,
                        new Rect(0, 0, dirTexture.width, dirTexture.height),
                        new Vector2(0.5f, 0.5f),
                        100
                    );
                JoystickDirObj.transform.localPosition = Vector3.zero;
                JoystickDirObj.transform.localScale = Vector3.one * 2;
                //Debug.Log("__JoystickDir" + dirTexture.width);
                return;
            }
            color = Settings.CrosshairColor.Value;
            color.a = Settings.CrosshairAlpha.Value;
            radius = Settings.CrosshairSize.Value;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            textureRenderer.enabled = AimingManager.IsAiming && !AimingManager.UsingJoystick && Settings.CrosshairVibration.Value && Settings.UseCrosshairImage.Value && textureRenderer.sprite;
            lineRenderer.enabled = AimingManager.IsAiming && !AimingManager.UsingJoystick && Settings.CrosshairVibration.Value && !textureRenderer.enabled;


            //Debug.Log(" textureRenderer.enabled " + (bool)(textureRenderer.sprite));
            //Debug.Log(" lineRenderer.enabled " + lineRenderer.enabled);

            if (!UnityEngine.Camera.main) { return; }
            if (!GameManager.SilentInstance || !Main.gm.hero_ctrl || Main.gm.hero_ctrl.IsPaused()) {
                return;
            }
            GetInput();
            AimingManager.ShouldUpdateHUD = (lastIsAiming != AimingManager.IsAiming || lastAttackKeyActive != AimingManager.AttackKeyActive);
            lastIsAiming = AimingManager.IsAiming;
            lastAttackKeyActive = AimingManager.AttackKeyActive;

            if (!AimingManager.IsAiming) {
                return;
            }
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            if (lineRenderer.enabled) {
                // 更新圆的位置
                DrawCircle(mouseWorldPos);
            }
            else if (textureRenderer.enabled) {
                transform.position = mouseWorldPos;
                float scale = Settings.CrosshairSize.Value * 3;
                transform.localScale = new Vector3(scale, scale, 1);
                textureRenderer.material.color = new Color(1, 1, 1, Settings.CrosshairAlpha.Value);
            }
        }
        public void GetInput() {

            if (!GameManager.SilentInstance || !Main.gm.hero_ctrl || Main.gm.hero_ctrl.IsPaused()) {
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
                // 获取所有活动精灵渲染器
                //SpriteRenderer[] allSpriteRenderers = FindObjectsOfType<SpriteRenderer>(true);
                //List<string> activeSprites = new List<string>();
                //string TargetSpriteName = "Silk_Recharge_glow0003";
                //var sprite = Resources.Load<Sprite>(TargetSpriteName);
                //Debug.Log(sprite);

                Vector2 pos = JoystickDirRenderer.transform.position;
                Debug.Log(" JoystickDir.transform.position :" + pos);
                DebugLineRenderer.DrawLine(pos + Vector2.up * 5, pos + Vector2.up * -5, Color.green, 2);
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
            //else
            ////if (Input.GetKeyDown(KeyCode.Joystick1Button9)) {
            //if (InputManager.ActiveDevice.LeftStickButton.WasPressed) {
            //    if (!AimingManager.UsingJoystick && AimingManager.IsAiming) {
            //        AimingManager.UsingJoystick = true;
            //    }
            //    else {
            //        Settings.AimingEnabled.Value = !AimingManager.IsAiming;
            //    }
            //}
            Vector3 joystickDir = InputManager.ActiveDevice.RightStick.Vector;
            if (joystickDir.magnitude >= 0.5 && !AimingManager.UsingJoystick && AimingManager.IsAiming) {
                AimingManager.UsingJoystick = true;
            }
            if (!AimingManager.IsAiming) { return; }
            if (Input.GetKeyDown(SwitchToUpSkillKey)) {
                AimingManager.AttackKeyActive = 1;
            }
            else if (Input.GetKeyDown(SwitchToMiddleSkillKey)) {
                AimingManager.AttackKeyActive = 2;
            }
            else if (Input.GetKeyDown(SwitchToDownSkillKey)) {
                AimingManager.AttackKeyActive = 3;
            }
            else if (Input.GetKeyDown(SwitchToAttackKey)) {
                AimingManager.AttackKeyActive = 4;
            }

            Settings.ControllerChangeBindModeEnum mode = Settings.ControllerChangeBindMode.Value;
            if (mode != Settings.ControllerChangeBindModeEnum.L3LeftJoystick) {
                if ((mode == Settings.ControllerChangeBindModeEnum.L3DPad && InputManager.ActiveDevice.LeftStickButton.IsPressed)
                        || mode == Settings.ControllerChangeBindModeEnum.DPad) {
                    if (InputManager.ActiveDevice.DPadUp.WasPressed) {
                        AimingManager.AttackKeyActive = 1;
                    }
                    else if (InputManager.ActiveDevice.DPadRight.WasPressed) {
                        AimingManager.AttackKeyActive = 2;
                    }
                    else if (InputManager.ActiveDevice.DPadDown.WasPressed) {
                        AimingManager.AttackKeyActive = 3;
                    }
                    else if (InputManager.ActiveDevice.DPadLeft.WasPressed) {
                        AimingManager.AttackKeyActive = 4;
                    }
                }
            }
            else if (InputManager.ActiveDevice.LeftStickButton.WasPressed) {
                joystickDir = InputManager.ActiveDevice.LeftStick.Vector;
                if (joystickDir.magnitude > 0.3) {
                    var angle = InputManager.ActiveDevice.LeftStick.Angle;
                    //AimingManager.AttackKeyActive = joystickDir
                    AimingManager.AttackKeyActive = Mathf.CeilToInt((angle + 45) % 360) / 90 + 1;
                    Debug.Log("joystick angle: " + Mathf.CeilToInt((angle + 45) % 360) / 90 + 1);
                }
            }
        }

        Vector3 GetMouseWorldPosition() {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = UnityEngine.Camera.main.nearClipPlane + 0.1f;
            return UnityEngine.Camera.main.ScreenToWorldPoint(mousePos);
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
