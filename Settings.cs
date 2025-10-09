using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
namespace silksong_Aiming {
    public class Settings {

        #region Configuration Properties
        private static Dictionary<string, Dictionary<string, string>> Sections = new() {
            ["Global"] = new(){
                { "EN", "0 Global" },
                { "ZH", "0 全局" }
            },
            ["Mouse"] = new(){
                { "EN", "1 Mouse" },
                { "ZH", "1 键鼠" }
            },
            ["Controller"] = new(){
                { "EN", "2 Controller" },
                { "ZH", "2 手柄" }
            },
            ["Other"] = new() {
                { "EN","3 Other" },
                { "ZH","3 其他" }
            },
            ["Warning"] = new() {
                { "EN","4 Warning feature" },
                { "ZH","4 有风险功能" }
            },

        };

        public enum ControllerChangeBindModeEnum {
            [System.ComponentModel.Description("L3 + DPad")]
            L3DPad,
            [System.ComponentModel.Description("L3 + Left Joystick")]
            L3LeftJoystick,
            [System.ComponentModel.Description("DPad Only")]
            DPad,
        }

        // ===== Aiming Settings =====

        #region Global
        [Section("Global")]
        [Description("EN", "Aiming Enabled", "Whether aiming is enabled")]
        [Description("ZH", "启用瞄准", "是否启用瞄准模式")]
        [DefaultValue(true)]
        public static ConfigEntry<bool> AimingEnabled { get; private set; }

        [Section("Global")]
        [Description("EN", "Replace Attack Key", "Replace the default attack key binding")]
        [Description("ZH", "替换攻击键", "替换默认的攻击键绑定")]
        [DefaultValue(true)]
        public static ConfigEntry<bool> ReplaceAttackKey { get; private set; }

        [Section("Global")]
        [Description("EN", "Up Slash Angle", "Angle for upward slashing attacks")]
        [Description("ZH", "上劈角度", "向上劈砍攻击的角度")]
        [DefaultValue(30)]
        [Range(0f, 90f)]
        public static ConfigEntry<float> UpSlashAngle { get; private set; }

        [Section("Global")]
        [Description("EN", "Down Slash Angle", "Angle for downward slashing attacks")]
        [Description("ZH", "下劈角度", "向下劈砍攻击的角度")]
        [DefaultValue(45)]
        [Range(0f, 90f)]
        public static ConfigEntry<float> DownSlashAngle { get; private set; }

        #endregion

        #region Mouse
        [Section("Mouse")]
        [Description("EN", "Toggle Aiming Key", "Key to toggle aiming mode")]
        [Description("ZH", "切换瞄准键", "切换瞄准模式的按键")]
        [DefaultValue(KeyCode.Z)]
        public static ConfigEntry<KeyboardShortcut> ToggleAimingKey { get; private set; }

        [Section("Mouse")]
        [Description("EN", "Switch To Up Skill Key", "Key to switch to the top skill")]
        [Description("ZH", "切换到上技能键", "切换到顶部技能的按键")]
        [DefaultValue(KeyCode.Alpha1)]
        public static ConfigEntry<KeyboardShortcut> SwitchToUpSkillKey { get; private set; }

        [Section("Mouse")]
        [Description("EN", "Switch To Middle Skill Key", "Key to switch to the middle skill")]
        [Description("ZH", "切换到中技能键", "切换到中间技能的按键")]
        [DefaultValue(KeyCode.Alpha2)]
        public static ConfigEntry<KeyboardShortcut> SwitchToMiddleSkillKey { get; private set; }

        [Section("Mouse")]
        [Description("EN", "Switch To Down Skill Key", "Key to switch to the bottom skill")]
        [Description("ZH", "切换到下技能键", "切换到底部技能的按键")]
        [DefaultValue(KeyCode.Alpha3)]
        public static ConfigEntry<KeyboardShortcut> SwitchToDownSkillKey { get; private set; }

        [Section("Mouse")]
        [Description("EN", "Switch To Attack Key", "Key to switch to attack mode")]
        [Description("ZH", "切换到攻击键", "切换到攻击模式的按键")]
        [DefaultValue(KeyCode.Alpha4)]
        public static ConfigEntry<KeyboardShortcut> SwitchToAttackKey { get; private set; }


        [Section("Mouse")]
        [Description("EN", "Crosshair Size", "Size of the aiming crosshair")]
        [Description("ZH", "准星大小", "瞄准准星的大小")]
        [DefaultValue(0.1f)]
        [Range(0.01f, 1.0f)]
        public static ConfigEntry<float> CrosshairSize { get; private set; }

        [Section("Mouse")]
        [Description("EN", "Crosshair Color", "Color of the aiming crosshair")]
        [Description("ZH", "准星颜色", "瞄准准星的颜色")]
        [DefaultValue("#66ccff")]
        public static ConfigEntry<Color> CrosshairColor { get; private set; }

        [Section("Mouse")]
        [Description("EN", "Crosshair Alpha", "Transparency of the aiming crosshair")]
        [Description("ZH", "准星透明度", "瞄准准星的透明度")]
        [DefaultValue(1.0f)]
        [Range(0.0f, 1.0f)]
        public static ConfigEntry<float> CrosshairAlpha { get; private set; }

        [Section("Mouse")]
        [Description("EN", "Use Crosshair Image", "Use custom crosshair image")]
        [Description("ZH", "使用准星图片", "启用自定义准星图片")]
        [DefaultValue(true)]
        public static ConfigEntry<bool> UseCrosshairImage { get; private set; }

        [Section("Mouse")]
        [Description("EN", "CrosshairI Image File", "Path of crosshairI Image File")]
        [Description("ZH", "准星图片路径", "准星图片路径")]
        [DefaultValue("")]
        public static ConfigEntry<string> CrosshairImageFile { get; private set; }

        [Section("Mouse")]
        [Description("EN", "Crosshair Vibration", "Enable crosshair vibration effect")]
        [Description("ZH", "准星震动", "启用准星震动效果")]
        [DefaultValue(false)]
        public static ConfigEntry<bool> CrosshairVibration { get; private set; }
        #endregion

        #region Controller
        [Section("Controller")]
        [Description("EN", "Switching Tools Mode", "Method of Switching Tools on Controller")]
        [Description("ZH", "切换工具的方式", "手柄切换工具的方式")]
        [DefaultValue(ControllerChangeBindModeEnum.DPad)]
        public static ConfigEntry<ControllerChangeBindModeEnum> ControllerChangeBindMode { get; private set; }

        [Section("Controller")]
        [Description("EN", "Enable Direction Indicator", "Show direction indicator when the right joystick is active")]
        [Description("ZH", "使用方向指示器", "在右摇杆起作用时显示方向指示器")]
        [DefaultValue(true)]
        public static ConfigEntry<bool> UseDirectionIndicator { get; private set; }

        [Section("Controller")]
        [Description("EN", "Direction Indicator Alpha", "Transparency of the direction indicator")]
        [Description("ZH", "方向指示器透明度", "方向指示器的透明度")]
        [DefaultValue(1.0f)]
        [Range(0.0f, 1.0f)]
        public static ConfigEntry<float> DirectionAlpha { get; private set; }

        [Section("Controller")]
        [Description("EN", "Direction Indicator Render Order", "Render order of the direction indicator")]
        [Description("ZH", "方向指示器渲染层级", "方向指示器的渲染层级")]
        [DefaultValue(0)]
        public static ConfigEntry<int> DirectionRenderOrder { get; private set; }
        #endregion

        #region Other

        [Section("Other")]
        [Description("EN", "Active Tool Color", "Color for the currently active tool HUD")]
        [Description("ZH", "活动工具颜色", "当前活动工具的HUD颜色")]
        [DefaultValue("#EACC80")]
        public static ConfigEntry<Color> ActiveToolColor { get; private set; }


        [Section("Other")]
        [Description("EN", "Silk Gain When Aiming Tool Hit", "Gain silk when hitting with aiming tools")]
        [Description("ZH", "瞄准工具命中获得丝绸", "使用瞄准工具命中时获得丝绸")]
        [DefaultValue(true)]
        public static ConfigEntry<bool> SilkGainWhenAimingToolHit { get; private set; }

        [Section("Other")]
        [Description("EN", "Tools To Add Silk", "List of tools that add silk when hitting")]
        [Description("ZH", "获得丝绸的工具", "命中时获得丝绸的工具列表")]
        [DefaultValue("Straight Pin, Tri Pin, Harpoon, Curve Claws, Curve Claws Upgraded, Shakra Ring, Conch Drill, WebShot Forge, WebShot Architect, WebShot Weaver")]
        public static ConfigEntry<string> ToolsToAddSilk { get; private set; }
        #endregion

        #region Warning
        [Section("Warning")]
        [Description("EN", "Enable Harpoon Dash Aiming", "Enable aiming for harpoon dash")]
        [Description("ZH", "使用飞针冲刺瞄准", "启用飞针的瞄准功能")]
        [DefaultValue(false)]
        public static ConfigEntry<bool> UseHarpoonDashAiming { get; private set; }

        [Section("Warning")]
        [Description("EN", "Enable Silk Charge Aiming", "Enable aiming for silk charge")]
        [Description("ZH", "使用丝刃镖瞄准", "启用丝刃镖的瞄准功能")]
        [DefaultValue(false)]
        public static ConfigEntry<bool> UseSilkChargeAiming { get; private set; }
        #endregion

        #endregion

        public static void Initialize(ConfigFile config, string lang) {
            // 获取所有配置属性
            var configProperties = typeof(Settings)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(ConfigEntry<>))
                .ToList();
            int order = configProperties.Count;
            foreach (var property in configProperties) {
                order--;
                // 获取配置属性
                var sectionAttr = property.GetCustomAttribute<SectionAttribute>();
                var descAttr = property.GetCustomAttributes<DescriptionAttribute>().ToList();
                var defaultValueAttr = property.GetCustomAttribute<DefaultValueAttribute>();
                var rangeAttr = property.GetCustomAttribute<RangeAttribute>();

                // 获取分类名称（本地化）
                string sectionName = sectionAttr?.Name ?? "General";
                string localizedSection = GetLocalizedSection(sectionName, lang);

                // 确定当前语言描述
                string keyName = property.Name;
                string description = "No description";

                // 根据系统语言选择描述
                var currentLang = Application.systemLanguage;
                var langDesc = descAttr.FirstOrDefault(d =>
                    d.Language == lang);

                langDesc ??= descAttr.FirstOrDefault(d =>
                    d.Language == "EN");

                if (langDesc != null) {
                    keyName = langDesc.KeyName;
                    description = langDesc.Description;
                }
                else if (descAttr.Count > 0) {
                    // 默认使用第一个描述
                    keyName = descAttr[0].KeyName;
                    description = descAttr[0].Description;
                }
                var propertyType = property.PropertyType.GetGenericArguments()[0];
                // 获取默认值
                object defaultValue = defaultValueAttr?.Value;
                if (defaultValue == null) {
                    // 尝试获取属性类型的默认值
                    defaultValue = propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;
                }

                //Debug.Log("key------"+keyName);
                // 创建配置描述
                ConfigDescription configDesc;
                if (rangeAttr != null) {
                    configDesc = new ConfigDescription(
                        description,
                        new AcceptableValueRange<float>(rangeAttr.Min, rangeAttr.Max),
                        new ConfigurationManagerAttributes { Order = order }
                    );
                }
                else {
                    configDesc = new ConfigDescription(description, null, new ConfigurationManagerAttributes { Order = order });
                }

                defaultValue = ProcessSpecialTypes(defaultValue, propertyType);
                // 创建配置项
                // 获取配置项的实际类型
                Type configType = property.PropertyType.GetGenericArguments()[0];

                // 使用反射创建配置项
                var bindMethod = typeof(ConfigFile).GetMethods()
                    .Where(m => m.Name == "Bind" && m.IsGenericMethod)
                    .Where(m => {
                        var parameters = m.GetParameters();
                        return parameters.Length == 4 &&
                               parameters[0].ParameterType == typeof(string) &&
                               parameters[1].ParameterType == typeof(string) &&
                               parameters[3].ParameterType == typeof(ConfigDescription);
                    })
                    .FirstOrDefault().MakeGenericMethod(configType);


                // 准备参数
                object[] parameters = new object[] {
                    localizedSection, // section
                    keyName,           // key
                    defaultValue,      // defaultValue
                    configDesc         // configDescription
                };

                // 调用Bind方法
                object configEntry = bindMethod.Invoke(config, parameters);

                // 设置属性值
                property.SetValue(null, configEntry);
            }
        }

        private static string GetLocalizedSection(string sectionName, string langCode) {
            if (Sections.TryGetValue(sectionName, out var translations)) {
                if (translations.TryGetValue(langCode, out string localized)) {
                    return localized;
                }

                // 尝试英语作为备选
                if (translations.TryGetValue("EN", out localized)) {
                    return localized;
                }
            }

            // 返回原始名称
            return sectionName;
        }
        private static object ProcessSpecialTypes(object defaultValue, Type configType) {
            // 处理键盘快捷键类型
            if (configType == typeof(KeyboardShortcut)) {
                return ConvertToKeyboardShortcut(defaultValue);
            }

            // 处理颜色类型
            if (configType == typeof(Color)) {
                return ConvertToColor(defaultValue);
            }
            return defaultValue;
        }
        private static object ConvertToColor(object value) {
            // 如果已经是Color类型，直接返回
            if (value is Color color) {
                return color;
            }

            // 如果值是字符串，尝试解析为颜色
            if (value is string colorString) {
                return ParseColor(colorString);
            }

            // 默认返回白色
            return Color.white;
        }
        private static Color ParseColor(string colorString) {
            // 尝试解析为颜色名称
            if (ColorUtility.TryParseHtmlString(colorString, out Color color)) {
                return color;
            }

            // 尝试常见颜色名称
            switch (colorString.ToLower()) {
                case "red": return Color.red;
                case "green": return Color.green;
                case "blue": return Color.blue;
                case "yellow": return Color.yellow;
                case "cyan": return Color.cyan;
                case "magenta": return Color.magenta;
                case "white": return Color.white;
                case "black": return Color.black;
                case "gray": return Color.gray;
            }

            Debug.LogError($"Invalid color value: {colorString}");
            return Color.white;
        }
        private static object ConvertToKeyboardShortcut(object value) {
            // 如果已经是KeyboardShortcut类型，直接返回
            if (value is KeyboardShortcut shortcut) {
                return shortcut;
            }

            // 如果值是KeyCode类型，转换为KeyboardShortcut
            if (value is KeyCode keyCode) {
                return new KeyboardShortcut(keyCode);
            }

            // 如果值是字符串，尝试解析为KeyCode
            if (value is string keyString) {
                if (Enum.TryParse<KeyCode>(keyString, out KeyCode parsedKey)) {
                    return new KeyboardShortcut(parsedKey);
                }
            }

            // 默认返回空快捷键
            return new KeyboardShortcut(KeyCode.None);
        }
        private static Color HexToColor(string hex) {
            if (ColorUtility.TryParseHtmlString(hex, out Color color)) {
                return color;
            }
            Debug.LogError($"Invalid color code: {hex}");
            return Color.white;
        }

        #region Annotations

        /// 
        /// Configuration item category
        /// 
        [AttributeUsage(AttributeTargets.Property)]
        public class SectionAttribute : Attribute {
            public string Name { get; }

            public SectionAttribute(string name) {
                Name = name;
            }
        }

        /// 
        /// Multilingual description
        /// 
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
        public class DescriptionAttribute : Attribute {
            public string Language { get; }
            public string KeyName { get; }
            public string Description { get; }

            public DescriptionAttribute(string language, string keyName, string description) {
                Language = language;
                KeyName = keyName;
                Description = description;
            }
        }

        /// 
        /// Default value
        /// 
        [AttributeUsage(AttributeTargets.Property)]
        public class DefaultValueAttribute : Attribute {
            public object Value { get; }

            public DefaultValueAttribute(object value) {
                Value = value;
            }

            // Overloads to support various types
            public DefaultValueAttribute(string value) : this((object)value) { }
            public DefaultValueAttribute(float value) : this((object)value) { }
            public DefaultValueAttribute(int value) : this((object)value) { }
            public DefaultValueAttribute(bool value) : this((object)value) { }
            public DefaultValueAttribute(KeyCode value) : this((object)value) { }
        }

        /// 
        /// Value range restriction
        /// 
        [AttributeUsage(AttributeTargets.Property)]
        public class RangeAttribute : Attribute {
            public float Min { get; }
            public float Max { get; }

            public RangeAttribute(float min, float max) {
                Min = min;
                Max = max;
            }
        }
        #endregion
    }
}