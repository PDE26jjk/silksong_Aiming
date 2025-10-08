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
            ["Other"] = new() {
                { "EN","Other" },
                { "ZH","其他" }
            },
            ["Aiming"] = new(){
                { "EN", "Aiming" },
                { "ZH", "瞄准" }
            },
            ["Visual"] = new(){
                { "EN", "Visual" },
                { "ZH", "视觉" }
            },
        };


        // ===== Aiming Settings =====

        [Section("Aiming")]
        [Description("EN", "Aiming Enabled", "Whether aiming is enabled")]
        [Description("ZH", "启用瞄准", "是否启用瞄准模式")]
        [DefaultValue(true)]
        public static ConfigEntry<bool> AimingEnabled { get; private set; }

        [Section("Aiming")]
        [Description("EN", "Toggle Aiming Key", "Key to toggle aiming mode")]
        [Description("ZH", "切换瞄准键", "切换瞄准模式的按键")]
        [DefaultValue(KeyCode.Z)]
        public static ConfigEntry<KeyboardShortcut> ToggleAimingKey { get; private set; }

        // ===== Skill Key Bindings =====
        [Section("SkillKeys")]
        [Description("EN", "Switch To Up Skill Key", "Key to switch to the top skill")]
        [Description("ZH", "切换到上技能键", "切换到顶部技能的按键")]
        [DefaultValue(KeyCode.Alpha1)]
        public static ConfigEntry<KeyboardShortcut> SwitchToUpSkillKey { get; private set; }

        [Section("SkillKeys")]
        [Description("EN", "Switch To Middle Skill Key", "Key to switch to the middle skill")]
        [Description("ZH", "切换到中技能键", "切换到中间技能的按键")]
        [DefaultValue(KeyCode.Alpha2)]
        public static ConfigEntry<KeyboardShortcut> SwitchToMiddleSkillKey { get; private set; }

        [Section("SkillKeys")]
        [Description("EN", "Switch To Down Skill Key", "Key to switch to the bottom skill")]
        [Description("ZH", "切换到下技能键", "切换到底部技能的按键")]
        [DefaultValue(KeyCode.Alpha3)]
        public static ConfigEntry<KeyboardShortcut> SwitchToDownSkillKey { get; private set; }

        [Section("SkillKeys")]
        [Description("EN", "Switch To Attack Key", "Key to switch to attack mode")]
        [Description("ZH", "切换到攻击键", "切换到攻击模式的按键")]
        [DefaultValue(KeyCode.Alpha4)]
        public static ConfigEntry<KeyboardShortcut> SwitchToAttackKey { get; private set; }

        // ===== Visual Settings =====
        [Section("Visual")]
        [Description("EN", "Crosshair Size", "Size of the aiming crosshair")]
        [Description("ZH", "准星大小", "瞄准准星的大小")]
        [DefaultValue(0.1f)]
        [Range(0.01f, 1.0f)]
        public static ConfigEntry<float> CrosshairSize { get; private set; }

        [Section("Visual")]
        [Description("EN", "Crosshair Color", "Color of the aiming crosshair")]
        [Description("ZH", "准星颜色", "瞄准准星的颜色")]
        [DefaultValue("#66ccff")]
        public static ConfigEntry<Color> CrosshairColor { get; private set; }

        [Section("Visual")]
        [Description("EN", "Use Crosshair Image", "Use custom crosshair image")]
        [Description("ZH", "使用准星图片", "启用自定义准星图片")]
        [DefaultValue(true)]
        public static ConfigEntry<bool> UseCrosshairImage { get; private set; }

        [Section("Visual")]
        [Description("EN", "CrosshairI Image File", "Path of crosshairI Image File")]
        [Description("ZH", "准星图片路径", "准星图片路径")]
        [DefaultValue("")]
        public static ConfigEntry<string> CrosshairImageFile { get; private set; }

        [Section("Visual")]
        [Description("EN", "Crosshair Alpha", "Transparency of the aiming crosshair")]
        [Description("ZH", "准星透明度", "瞄准准星的透明度")]
        [DefaultValue(1.0f)]
        [Range(0.0f, 1.0f)]
        public static ConfigEntry<float> CrosshairAlpha { get; private set; }
        [Section("Visual")]
        [Description("EN", "Crosshair Vibration", "Enable crosshair vibration effect")]
        [Description("ZH", "准星震动", "启用准星震动效果")]
        [DefaultValue(false)]
        public static ConfigEntry<bool> CrosshairVibration { get; private set; }

        // ===== Gain Silk Settings =====
        [Section("SilkGain")]
        [Description("EN", "Silk Gain When Aiming Tool Hit", "Gain silk when hitting with aiming tools")]
        [Description("ZH", "瞄准工具命中获得丝绸", "使用瞄准工具命中时获得丝绸")]
        [DefaultValue(true)]
        public static ConfigEntry<bool> SilkGainWhenAimingToolHit { get; private set; }

        [Section("SilkGain")]
        [Description("EN", "Tools To Add Silk", "List of tools that add silk when hitting")]
        [Description("ZH", "获得丝绸的工具", "命中时获得丝绸的工具列表")]
        [DefaultValue("Straight Pin, Tri Pin, Harpoon, Curve Claws, Curve Claws Upgraded, Shakra Ring, Conch Drill, WebShot Forge, WebShot Architect, WebShot Weaver")]
        public static ConfigEntry<string> ToolsToAddSilk { get; private set; }

        // ===== Other Settings =====
        [Section("Other")]
        [Description("EN", "Replace Attack Key", "Replace the default attack key binding")]
        [Description("ZH", "替换攻击键", "替换默认的攻击键绑定")]
        [DefaultValue(true)]
        public static ConfigEntry<bool> ReplaceAttackKey { get; private set; }

        [Section("Other")]
        [Description("EN", "Use DPad To Change Tools", "Use DPad for changing tools")]
        [Description("ZH", "使用方向键切换工具", "使用方向键切换工具")]
        [DefaultValue(true)]
        public static ConfigEntry<bool> UseDPadToChangeTools { get; private set; }

        [Section("Other")]
        [Description("EN", "Use Harpoon Dash Aiming", "Enable aiming for harpoon dash")]
        [Description("ZH", "使用鱼叉冲刺瞄准", "启用鱼叉冲刺的瞄准功能")]
        [DefaultValue(false)]
        public static ConfigEntry<bool> UseHarpoonDashAiming { get; private set; }

        [Section("Other")]
        [Description("EN", "Active Tool Color", "Color for the currently active tool")]
        [Description("ZH", "活动工具颜色", "当前活动工具的颜色")]
        [DefaultValue("#EACC80")]
        public static ConfigEntry<Color> ActiveToolColor { get; private set; }

        #endregion

        public static void Initialize(ConfigFile config, string lang) {
            // 获取所有配置属性
            var configProperties = typeof(Settings)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(ConfigEntry<>))
                .ToList();
            foreach (var property in configProperties) {
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

                // 创建配置描述
                ConfigDescription configDesc;
                if (rangeAttr != null) {
                    configDesc = new ConfigDescription(
                        description,
                        new AcceptableValueRange<float>(rangeAttr.Min, rangeAttr.Max)
                    );
                }
                else {
                    configDesc = new ConfigDescription(description);
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