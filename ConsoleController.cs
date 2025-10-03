using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace silksong_Aiming {
    public class ConsoleController : MonoBehaviour {
        private bool showConsole = false;
        private string inputBuffer = "";
        private Vector2 scrollPosition;
        private List<string> logHistory = new List<string>();
        private List<string> commandHistory = new List<string>();
        private int historyIndex = -1;
        private const int maxHistoryLines = 100;
        private bool shouldFocusInput = false;
        // GUI 样式缓存
        private GUIStyle consoleStyle;
        private GUIStyle inputStyle;
        private GUIStyle buttonStyle;

        public static float[] TempData = { 0, 0, 0, 0, 0 };
        private void OnGUI() {
            if (!showConsole) return;

            // 设置更大的字体大小
            int fontSize = 20;
            if (consoleStyle == null) {
                consoleStyle = new GUIStyle();
                consoleStyle.normal.textColor = Color.white;
                consoleStyle.fontSize = fontSize;
            }

            if (inputStyle == null) {
                inputStyle = new GUIStyle(GUI.skin.textField);
                inputStyle.fontSize = fontSize;
            }

            if (buttonStyle == null) {
                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.fontSize = fontSize;
            }

            // 控制台高度（增加到屏幕高度的40%）
            float consoleHeight = Mathf.Min(Screen.height * 0.4f, 400);

            // 控制台背景
            GUI.Box(new Rect(0, 0, Screen.width, consoleHeight), "");

            // 日志区域
            GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, consoleHeight - 45));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (string log in logHistory) {
                GUILayout.Label(log, consoleStyle);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
            Event currentEvent = Event.current;
            if (GUI.GetNameOfFocusedControl() == "ConsoleInput") {
                if (currentEvent.type == EventType.KeyDown) {
                    switch (currentEvent.keyCode) {
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            if (!string.IsNullOrEmpty(inputBuffer)) {
                                ProcessCommand(inputBuffer);
                                inputBuffer = "";
                                currentEvent.Use();
                            }
                            break;

                        case KeyCode.UpArrow:
                            if (commandHistory.Count > 0) {
                                if (historyIndex == -1) {
                                    historyIndex = commandHistory.Count - 1;
                                }
                                else if (historyIndex > 0) {
                                    historyIndex--;
                                }
                                inputBuffer = commandHistory[historyIndex];
                                currentEvent.Use();
                            }
                            break;

                        case KeyCode.DownArrow:
                            if (historyIndex != -1) {
                                if (historyIndex < commandHistory.Count - 1) {
                                    historyIndex++;
                                    inputBuffer = commandHistory[historyIndex];
                                }
                                currentEvent.Use();
                            }
                            break;

                        case KeyCode.Escape:
                            ToggleConsole();
                            currentEvent.Use();
                            break;
                    }
                }
            }

            // 输入区域
            GUILayout.BeginArea(new Rect(10, consoleHeight - 30, Screen.width - 100, 30));
            GUI.SetNextControlName("ConsoleInput");
            inputBuffer = GUILayout.TextField(inputBuffer, inputStyle);
            GUILayout.EndArea();

            // 发送按钮（更大更明显）
            if (GUI.Button(new Rect(Screen.width - 85, consoleHeight - 30, 75, 30), "Send", buttonStyle)) {
                ProcessCommand(inputBuffer);
            }


            // 自动聚焦到输入框
            if (shouldFocusInput) {
                GUI.FocusControl("ConsoleInput");
                historyIndex = -1;
                shouldFocusInput = false;
            }
        }

        public void ToggleConsole() {
            showConsole = !showConsole;
            if (showConsole) {
                scrollPosition.y = float.MaxValue; // 滚动到底部
                shouldFocusInput = true;
            }
        }

        public void AddLog(string log) {
            logHistory.Add(log);
            if (logHistory.Count > maxHistoryLines) {
                logHistory.RemoveAt(0);
            }

            // 自动滚动到底部
            scrollPosition.y = float.MaxValue;
        }

        private void ProcessCommand(string command) {
            if (string.IsNullOrWhiteSpace(command)) return;

            // 添加命令到日志（带前缀）
            AddLog("> " + command);

            // 添加到命令历史
            commandHistory.Add(command);
            historyIndex = -1; // 重置历史索引

            // 在这里处理具体命令逻辑
            if (command.Equals("clear", System.StringComparison.OrdinalIgnoreCase)) {
                logHistory.Clear();
                AddLog("Console cleared");
            }
            else if (command.Equals("help", System.StringComparison.OrdinalIgnoreCase)) {
                AddLog("Available commands:");
                AddLog("  clear - Clear console");
                AddLog("  help - Show this help");
                AddLog("  quit - Close console");
            }
            else if (command.Equals("quit", System.StringComparison.OrdinalIgnoreCase)) {
                ToggleConsole();
            }
            else if (command.Equals("o")) {

                //GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>(true);
                //List<Behaviour> allBehaviours = new List<Behaviour>();

                //StringBuilder sb = new StringBuilder();
                //foreach (GameObject go in allGameObjects) {
                //    sb.Append($"{go.name}{{");
                //    // 获取对象上所有Behaviour组件
                //    Behaviour[] behaviours = go.GetComponents<Behaviour>();
                //    foreach (Behaviour behaviour in behaviours) {
                //        sb.Append(string.Join(", ", behaviour.GetType().Name));
                //    }
                //    sb.AppendLine("}");
                //    // 打印结果 

                //}
            }
            else if (command.Equals("p")) {
                if (!Platform.Current.IsMouseSupported) {
                    return;
                }
                AimingManager.IsAiming = !AimingManager.IsAiming;
                //Debug.Log($"show mouse {AimingManager.IsAiming}");
            }
            else if (command.Equals("s")) {
                foreach (var col in FindObjectsByType<Collider2D>(FindObjectsSortMode.None)) {
                    if (!col.gameObject.GetComponent<CollisionProxy>()) {
                        col.gameObject.AddComponent<CollisionProxy>();
                    }
                }
                AddLog($"Add CollisionProxy");
            }
            else if (command.StartsWith("t ")) {
                var arges = command.Split(' ');
                if (arges.Length == 3) {
                    if (int.TryParse(arges[1], out int ind)) {
                        if (float.TryParse(arges[2], out float data)) {
                            TempData[ind] = data;
                        }
                    }
                }

            }
            else if (command.Equals("unlock")) {
                var lists = Resources.FindObjectsOfTypeAll<ToolItemList>();
                foreach (var list in lists) {
                    //Debug.Log("Unlocking tools ----------------------");
                    foreach (var tool in list) {
                        if (!tool.IsUnlocked) {
                            Debug.Log($"Unlocking {tool.name}");
                            tool.Unlock();
                        }
                    }
                }
            }
            else if (command.Equals("test")) {
                //MouseInputRenderer mouse = Main.consoleObject.GetComponent<MouseInputRenderer>();
                //Debug.Log(mouse);
            }
            else {
                AddLog($"Unknown command: {command}");
            }

            inputBuffer = "";
        }
    }
}
