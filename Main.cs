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
    [BepInPlugin("com.PDE26jjk.Aiming", "Aiming", "1.0.0")]
    public class Main : BaseUnityPlugin {
        private ConsoleController consoleController;
        private InputController mouseCircleRenderer;
        private static GameObject consoleObject;
        private static GameObject aimingObject;
        public static GameManager gm => _gm != null ? _gm : (_gm = GameManager.instance);
        public static HeroController hero => gm != null ? gm.hero_ctrl : null;
        public static GameManager _gm;
        private ILogListener logListener;
        public static Main instance => _instance;
        public static Main _instance;
        public static bool hasSetup = false;
        //private bool found = false;
        private void Awake() {
            _instance = this;
#if DEBUG
            Logger.LogInfo("Console Mod Initialized! Press ~ to open console.");
            //Invoke(nameof(CreateConsoleIfNeeded), 0.5f);
            CreateConsoleIfNeeded();
            Harmony.CreateAndPatchAll(typeof(AtStart));
#endif
            WaitForGameManager();

        }
        private void WaitForGameManager() {
            if (GameManager.SilentInstance) {
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

                Harmony.CreateAndPatchAll(typeof(HarpoonDash_Patcher));
                Harmony.CreateAndPatchAll(typeof(SilkSpear_Patcher));
                Harmony.CreateAndPatchAll(typeof(SilkCharge_Patcher));
                Harmony.CreateAndPatchAll(typeof(ToolsHUD_Patcher));
#if DEBUG
                TryStartGame();
#endif
                GetSettings();
                if (Input.GetJoystickNames().Length > 0) AimingManager.UsingJoystick = true;
                hasSetup = true;
            }
            else {
                //Debug.Log("No gameManager"); 
                Invoke(nameof(WaitForGameManager), 0.5f);
            }
        }

        private void TryStartGame() {
            gm.LoadGameFromUI(2);
        }
        private void GetSettings() {
            string lang; // DE, EN, ES, FR, IT, JA, KO, PT, RU, ZH
            TeamCherry.Localization.LocalizationProjectSettings.TryGetSavedLanguageCode(out lang);
            //Debug.Log("TryGetSavedLanguageCode-----------------------------------------------" + lang);

            Settings.Initialize(this.Config, lang);
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
        public static bool IsAiming => Settings.AimingEnabled.Value;
        internal static Vector3 MousePosW = Vector3.zero;
        internal static float LastClickTime;
        internal static int AttackKeyActive = 1;
        internal static bool UsingJoystick;
        internal static bool ReplaceAttackKey => Settings.ReplaceAttackKey.Value;
        internal static bool ShouldUpdateHUD;
        internal static bool DefaultJoystickDir;
        internal static bool OnWallWhenGetDir;
        public static bool UseAttackKeyOverlay() {
            return IsAiming && ReplaceAttackKey;
        }

        public static Vector3 RefreshMousePosition() {
            DefaultJoystickDir = false;
            OnWallWhenGetDir = false;
            if (!UsingJoystick) {
                // 获取鼠标世界坐标
                Vector3 mousePos = Input.mousePosition;
                //Debug.Log(UnityEngine.Camera.main);
                mousePos.z = -UnityEngine.Camera.main.transform.position.z;
                MousePosW = UnityEngine.Camera.main.ScreenToWorldPoint(mousePos);
                MousePosW.z = 0; // 确保Z坐标为0
                //Debug.Log("update Mouse " + MousePosW);
            }
            // 手柄方向的偏移代替鼠标位置
            else {
                HeroController hero = Main.gm.hero_ctrl;
                var heroPos = hero.transform.position;
                var heroFacingRight = hero.cState.facingRight;
                Vector3 joystickDir = InputManager.ActiveDevice.RightStick.Vector;
                if (joystickDir.magnitude < 0.3) {
                    joystickDir = heroFacingRight ? Vector2.right : Vector2.left;
                    //Debug.Log("wallSliding " + hero.cState.wallSliding);
                    DefaultJoystickDir = true;
                    if (hero.cState.wallSliding) {
                        OnWallWhenGetDir = true;
                        joystickDir.x = -joystickDir.x;
                    }
                }
                else {
                    joystickDir.Normalize();
                }

                //Debug.Log(Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg);
                MousePosW = heroPos + joystickDir * 30;
                //Debug.Log("update Mouse UsingJoystick" + MousePosW);
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

        internal static bool UseHarpoonDashAiming() {
            return IsAiming && Settings.UseHarpoonDashAiming.Value;
        }
        internal static bool UseSilkChargeAiming() {
            return IsAiming && Settings.UseSilkChargeAiming.Value;
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
    public class SetStateLogger {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayMakerFSM), "SendEvent")]
        public static void SendEvent(PlayMakerFSM __instance, string eventName) {
            //Debug.Log($"[FSM事件] {__instance.name} 事件: {eventName}");
        }

    }
}
