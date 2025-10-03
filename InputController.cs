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

        }
        private KeyCode ToggleAimingKey;
        private KeyCode SwitchToUpSkillKey;
        private KeyCode SwitchToMiddleSkillKey;
        private KeyCode SwitchToDownSkillKey;
        private KeyCode SwitchToAttackKey;
        void Update() {
            lineRenderer.enabled = AimingManager.IsAiming && !AimingManager.UsingJoystick;
            if (!UnityEngine.Camera.main) { return; }
            if (!Main.gm || !Main.gm.hero_ctrl || Main.gm.hero_ctrl.IsPaused()) {
                return;
            }
            GetInput();
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
            if (Input.GetKeyDown(ToggleAimingKey)) {
                AimingManager.IsAiming = !AimingManager.IsAiming;
                AimingManager.UsingJoystick = false;
            }
            else
            //if (Input.GetKeyDown(KeyCode.Joystick1Button9)) {
            if (InputManager.ActiveDevice.LeftStickButton.WasPressed) {
                AimingManager.IsAiming = !AimingManager.IsAiming;
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
