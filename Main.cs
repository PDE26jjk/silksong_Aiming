using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using InControl;

namespace silksong_Aiming {
    [BepInPlugin("4DDE5A40-E435-E161-9E6D-4D384476C74D", "Aiming", "0.0.1")]
    public class Main : BaseUnityPlugin {
        private ConsoleController consoleController;
        private InputController mouseCircleRenderer;
        private static GameObject consoleObject;
        private static GameObject aimingObject;
        public static GameManager gm => _gm != null ? _gm : (_gm = GameManager.instance);
        public static GameManager _gm;
        private ILogListener logListener;
        private bool found = false;
        private void Awake() {
            Settings.Read();
#if DEBUG
            Logger.LogInfo("Console Mod Initialized! Press ~ to open console.");
            //Invoke(nameof(CreateConsoleIfNeeded), 0.5f);
            CreateConsoleIfNeeded();
            Harmony.CreateAndPatchAll(typeof(AtStart));
#endif
            CreateAimingControllerIfNeeded();
            Harmony.CreateAndPatchAll(typeof(HeroController_ThrowTool_Patcher));
            Harmony.CreateAndPatchAll(typeof(HeroInput_Patcher));
            Harmony.CreateAndPatchAll(typeof(HitEnemy_Patcher));
            Harmony.CreateAndPatchAll(typeof(AimingPatcher));
            Harmony.CreateAndPatchAll(typeof(SetStateLogger));
            Harmony.CreateAndPatchAll(typeof(Tripin_Patcher));
            Harmony.CreateAndPatchAll(typeof(Fisherpin_Patcher));
            Harmony.CreateAndPatchAll(typeof(Boomerang_Patcher));
            Harmony.CreateAndPatchAll(typeof(Conch_Patcher));
            Harmony.CreateAndPatchAll(typeof(RosaryCannon_Patcher));
            Harmony.CreateAndPatchAll(typeof(WebshotWeaver_Patcher));
            Harmony.CreateAndPatchAll(typeof(WebshotForge_Patcher));
#if DEBUG
            Invoke(nameof(TryStartGame), 0.5f);
#endif
            AimingManager.IsAiming = Settings.GetBool("AimingEnabledAtStart", true);
        }

        private void TryStartGame() {
            if (gm) {
                var obj = gm.gameObject;
                Debug.Log(obj);
                gm.LoadGameFromUI(2);
            }
            else {
                //Debug.Log("No gameManager");
                Invoke(nameof(TryStartGame), 0.5f);
            }
        }

        private void CreateConsoleIfNeeded() {
            if (consoleObject == null) {
                consoleObject = new GameObject("__ConsoleController");
                DontDestroyOnLoad(consoleObject);
                if (consoleObject != null) {
                    if (consoleObject.GetComponent<ConsoleController>() == null) {
                        consoleController = consoleObject.AddComponent<ConsoleController>();
                    }
                }
            }
            if (logListener == null) {
                logListener = new ConsoleLogListener(consoleController);
                BepInEx.Logging.Logger.Listeners.Add(logListener);
            }
        }
        private void CreateAimingControllerIfNeeded() {
            if (aimingObject == null) {
                aimingObject = new GameObject("__AimingObject");
                DontDestroyOnLoad(aimingObject);
                if (aimingObject != null) {
                    if (aimingObject.GetComponent<InputController>() == null) {
                        mouseCircleRenderer = aimingObject.AddComponent<InputController>();
                    }
                }
            }
        }

#if DEBUG
        private void Update() {

            // ~
            if (Input.GetKeyDown(KeyCode.BackQuote)) {
                CreateConsoleIfNeeded();

                if (consoleController != null) {
                    consoleController.ToggleConsole();
                }
            }
        }
#endif
    }
    // https://github.com/Bigfootmech/Silksong_Skipintro/blob/master/Mod.cs
    [HarmonyPatch(typeof(StartManager), "Start")]
    public class AtStart {
        [HarmonyPostfix]
        static void Postfix(StartManager __instance,
            IEnumerator __result,
            ref AsyncOperation ___loadop) // , ref float ___FADE_SPEED
        {
            __instance.gameObject.GetComponent<Animator>().speed = 99999;
        }
    }

    public class AimingManager {
        public static bool IsAiming = true;
        internal static Vector3 MousePosW = Vector3.zero;
        internal static float LastClickTime;
        internal static int AttackKeyActive = 1;
        internal static bool UsingJoystick;
        public static bool UseAttackKeyOverlay() {
            return IsAiming && !UsingJoystick;
        }

        public static Vector3 RefreshMousePosition() {
            if (!UsingJoystick) {
            // 获取鼠标世界坐标
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -UnityEngine.Camera.main.transform.position.z;
            MousePosW = UnityEngine.Camera.main.ScreenToWorldPoint(mousePos);
            MousePosW.z = 0; // 确保Z坐标为0
            }
            // 手柄方向的偏移代替鼠标位置
            else {
                var heroPos = Main.gm.hero_ctrl.transform.position;
                var heroFacingRight = Main.gm.hero_ctrl.cState.facingRight;
                Vector3 joystickDir = InputManager.ActiveDevice.RightStick.Vector;
                if(joystickDir.magnitude < 0.3) {
                    joystickDir = heroFacingRight ? Vector2.right : Vector2.left;
                }
                else {
                    joystickDir.Normalize();
                }

                //Debug.Log(Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg);
                MousePosW = heroPos + joystickDir * 30;
            }
            return MousePosW;
        }

        public static float GetAngleToMouse(Vector3 posW) {
            Vector2 throwDirection = (MousePosW - posW).normalized;
            float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
            return angle;
        }
        public static Vector2 GetDirectionToMouse(Vector3 posW) {
            return (MousePosW - posW).normalized;
        }
    }
    public class ConsoleLogListener : ILogListener {
        private readonly ConsoleController console;

        public ConsoleLogListener(ConsoleController consoleController) {
            console = consoleController;
        }

        public void LogEvent(object sender, LogEventArgs eventArgs) {
            // 将日志事件转发到控制台
            console.AddLog($"[{eventArgs.Level}] {eventArgs.Data}");
        }

        public void Dispose() { }
    }
    public class CollisionProxy : MonoBehaviour {
        private void OnCollisionEnter2D(Collision2D collision) {
            Debug.Log($": {name}----- {collision.gameObject.name}: {collision.contacts[0].point}");
            if (collision.gameObject.name.Contains("MossBone Crawler")) {
                var be = collision.gameObject.GetComponents<MonoBehaviour>();
                List<string> list = new List<string>();
                foreach (var item in be) {
                    list.Add(item.GetType().Name);
                }

                Debug.Log(string.Join(",", list));
            }
        }
    }

    public class AimingPatcher {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InputHandler), "SetCursorVisible", new Type[] { typeof(bool) })]
        public static void InputHandler_SetCursorVisible_Post(bool value) {
            if (AimingManager.IsAiming) {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

    }
    [HarmonyPatch(typeof(PlayMakerFSM), "SendEvent")]
    public class SetStateLogger {
        public static void Prefix(PlayMakerFSM __instance, string eventName) {
            //Debug.Log($"[FSM事件] {__instance.name} 事件: {eventName}");
            //if (eventName.Contains("PIN")) {
            //    // 下一帧再执行
            //    __instance.StartCoroutine(ExecuteNextFrame(__instance));
            //}
        }
        private static IEnumerator ExecuteNextFrame(PlayMakerFSM fsm) {
            // 等待一帧
            if (fsm.Fsm.ActiveState.Name.Contains("Idle")) {
                //Debug.Log($"Idle...");
                yield return new WaitForSeconds(0.01f);
                fsm.StartCoroutine(ExecuteNextFrame(fsm));
            }
            else {
                Debug.Log($"当前状态: {fsm.Fsm.ActiveState.Name} : {fsm.Fsm.ActiveState.Description}");
                yield break;
            }
        }
    }
}
