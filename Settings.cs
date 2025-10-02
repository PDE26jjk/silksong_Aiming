using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class Settings {
    private readonly string SettingFile = Path.Combine(Paths.ConfigPath, "Aiming.cfg");
    private static Dictionary<string, string> data = new Dictionary<string, string>();

    public static Settings Read() {
        var ret = new Settings();
        if (!File.Exists(ret.SettingFile)) {
            Debug.LogError($"Error reading config: Aiming Config File not found. Creating default config...");
            File.WriteAllText(ret.SettingFile, defaultSettingString);
        }
        ReadFile(ret.SettingFile);
        return ret;
    }

    private static void ReadFile(string filePath) {
        try {
            var lines = File.ReadAllLines(filePath);
            data.Clear();

            foreach (var line in lines) {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#")) continue;

                var parts = trimmed.Split(new[] { '=' }, 2);
                if (parts.Length == 2) {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    data[key] = value;
                }
            }
        }
        catch (Exception ex) {
            Debug.LogError($"Error reading config file: {ex.Message}");
        }
    }

    public static string Get(string key, string defaultValue = "") {
        return data.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public static int GetInt(string key, int defaultValue = 0) {
        return int.TryParse(Get(key), out int result) ? result : defaultValue;
    }

    public static float GetFloat(string key, float defaultValue = 0f) {
        return float.TryParse(Get(key), out float result) ? result : defaultValue;
    }

    public static bool GetBool(string key, bool defaultValue = false) {
        return bool.TryParse(Get(key), out bool result) ? result : defaultValue;
    }
    public static KeyCode GetKeyCode(string key, KeyCode defaultValue = KeyCode.None) {
        string value = Get(key);
        if (string.IsNullOrEmpty(value)) return defaultValue;

        try {
            // 尝试直接解析
            if (Enum.TryParse(value, true, out KeyCode result)) {
                return result;
            }

            // 处理特殊键名
            return value switch {
                "LeftMouse" => KeyCode.Mouse0,
                "RightMouse" => KeyCode.Mouse1,
                "MiddleMouse" => KeyCode.Mouse2,
                "Mouse4" => KeyCode.Mouse3,
                "Mouse5" => KeyCode.Mouse4,
                "Mouse6" => KeyCode.Mouse5,
                "Mouse7" => KeyCode.Mouse6,
                "Backspace" => KeyCode.Backspace,
                "Tab" => KeyCode.Tab,
                "Enter" => KeyCode.Return,
                "Esc" => KeyCode.Escape,
                "Space" => KeyCode.Space,
                "PageUp" => KeyCode.PageUp,
                "PageDown" => KeyCode.PageDown,
                "End" => KeyCode.End,
                "Home" => KeyCode.Home,
                "LeftArrow" => KeyCode.LeftArrow,
                "UpArrow" => KeyCode.UpArrow,
                "RightArrow" => KeyCode.RightArrow,
                "DownArrow" => KeyCode.DownArrow,
                "Insert" => KeyCode.Insert,
                "Delete" => KeyCode.Delete,
                "F1" => KeyCode.F1,
                "F2" => KeyCode.F2,
                "F3" => KeyCode.F3,
                "F4" => KeyCode.F4,
                "F5" => KeyCode.F5,
                "F6" => KeyCode.F6,
                "F7" => KeyCode.F7,
                "F8" => KeyCode.F8,
                "F9" => KeyCode.F9,
                "F10" => KeyCode.F10,
                "F11" => KeyCode.F11,
                "F12" => KeyCode.F12,
                "NumPad0" => KeyCode.Keypad0,
                "NumPad1" => KeyCode.Keypad1,
                "NumPad2" => KeyCode.Keypad2,
                "NumPad3" => KeyCode.Keypad3,
                "NumPad4" => KeyCode.Keypad4,
                "NumPad5" => KeyCode.Keypad5,
                "NumPad6" => KeyCode.Keypad6,
                "NumPad7" => KeyCode.Keypad7,
                "NumPad8" => KeyCode.Keypad8,
                "NumPad9" => KeyCode.Keypad9,
                "NumPad*" => KeyCode.KeypadMultiply,
                "NumPad+" => KeyCode.KeypadPlus,
                "NumPad-" => KeyCode.KeypadMinus,
                "NumPad." => KeyCode.KeypadPeriod,
                "NumPad/" => KeyCode.KeypadDivide,
                "NumPadEnter" => KeyCode.KeypadEnter,
                "LeftShift" => KeyCode.LeftShift,
                "RightShift" => KeyCode.RightShift,
                "LeftCtrl" => KeyCode.LeftControl,
                "RightCtrl" => KeyCode.RightControl,
                "LeftAlt" => KeyCode.LeftAlt,
                "RightAlt" => KeyCode.RightAlt,
                _ => defaultValue
            };
        }
        catch {
            return defaultValue;
        }
    }
    public static List<string> GetStringList(string key, List<string> defaultValue = null) {
        defaultValue ??= new List<string>();

        if (!data.TryGetValue(key, out var value)) {
            return defaultValue;
        }

        try {
            // 分割字符串并移除空格
            return value.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
        catch {
            return defaultValue;
        }
    }
    public static Color GetColor(string key, Color defaultValue)
    {
        string value = Get(key);
        if (string.IsNullOrEmpty(value)) return defaultValue;
        
        try
        {
            // 尝试解析十六进制颜色
            if (ColorUtility.TryParseHtmlString(value, out Color color))
            {
                return color;
            }
            
            // 尝试解析RGBA格式
            if (value.Contains(","))
            {
                string[] parts = value.Split(',');
                if (parts.Length >= 3)
                {
                    float r = float.Parse(parts[0].Trim());
                    float g = float.Parse(parts[1].Trim());
                    float b = float.Parse(parts[2].Trim());
                    float a = parts.Length > 3 ? float.Parse(parts[3].Trim()) : 1f;
                    
                    return new Color(r, g, b, a);
                }
            }
            
            // 尝试解析颜色名称
            if (ColorNameToValue.TryGetValue(value.ToLower(), out Color namedColor))
            {
                return namedColor;
            }
            
            return defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }
        private static readonly Dictionary<string, Color> ColorNameToValue = new Dictionary<string, Color>
    {
        {"red", Color.red},
        {"green", Color.green},
        {"blue", Color.blue},
        {"yellow", Color.yellow},
        {"cyan", Color.cyan},
        {"magenta", Color.magenta},
        {"white", Color.white},
        {"black", Color.black},
        {"gray", Color.gray},
        {"grey", Color.grey},
        {"clear", Color.clear},
        {"orange", new Color(1f, 0.5f, 0f)},
        {"purple", new Color(0.5f, 0f, 0.5f)},
        {"pink", new Color(1f, 0.75f, 0.8f)},
        {"brown", new Color(0.6f, 0.4f, 0.2f)},
        {"gold", new Color(1f, 0.84f, 0f)},
        {"silver", new Color(0.75f, 0.75f, 0.75f)}
    };
    private static string defaultSettingString = @"
# Aiming Configuration
# Format: key = value

# Aiming settings
ToggleAimingKey=Z
AimingEnabledAtStart = true

# Switch to Skill Key bindings
SwitchToUpSkillKey=Alpha1
SwitchToMiddleSkillKey=Alpha2
SwitchToDownSkillKey=Alpha3
SwitchToAttackKey=Alpha4

# Visual settings
CrosshairSize = 0.1
CrosshairColor = red

# Gain silk settings
SilkGainWhenAimingToolHit=true
ToolsToAddSilk=Straight Pin, Tri Pin, Harpoon, Curve Claws, Curve Claws Upgraded, Shakra Ring, Conch Drill, WebShot Forge, WebShot Architect,WebShot Weaver

";
}