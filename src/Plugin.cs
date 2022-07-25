using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System;

namespace ValueTooltip
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        private readonly ConfigEntry<bool> _enabled;
        private readonly ConfigEntry<bool> _useColor;
        private readonly ConfigEntry<bool> _useStack;
        private readonly ConfigEntry<DisplayType> _displayType;
        private readonly ConfigEntry<bool> _useCommerce;
        private readonly ConfigEntry<KeyCode> _hotKey;

        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        //private static readonly bool AlreadyPatched = Harmony.GetPatchInfo((MethodBase)AccessTools.Method(typeof(Inventory), "fillHoverDescription", (Type[])null, (Type[])null)) != null;

        public enum DisplayType
        {
            SELL,
            BUY,
            BOTH
        }

        public enum ConfigType
        {
            ENABLED,
            COLOR,
            STACK,
            COMMERCE
        }

        public Plugin()
        {
            _enabled = Config.Bind("Options", "Enabled", true, "You can disable the mod quickly by editing this value to false.");
            _useColor = Config.Bind("Options", "Color", true, "When true this will format the Value text to yellow.");
            _useStack = Config.Bind("Options", "Stack", true, "When true this will multiply the value of items by the item stack size.");
            _displayType = Config.Bind("Options", "Display", DisplayType.SELL, "Determines what type of information to display.");
            _useCommerce = Config.Bind("Options", "Commerce", true, "When true this will calculate the value using commerce licence level");
            _hotKey = Config.Bind<KeyCode>("General", "HotKey", KeyCode.LeftControl, "The Unity keybind that will toggle display of stack price. disable with KeyCode.None");
        }

        private void Awake()
        {
            Instance = this;

            if (!CheckConfig(ConfigType.ENABLED))
            {
                Logger.LogInfo($"Mod: {PluginInfo.PLUGIN_NAME} is disabled!");
                Logger.LogInfo("Enable it via the OctrDev.ValueTooltip.cfg file.");
                return;
            }        

            Patch();
        }

        void Update()
        {
            if(Inventory.inv.invOpen)
            {
                
                if(Input.GetKeyDown(_hotKey.Value))
                {
                    ToggleHotkey();
                }

                if(Input.GetKeyUp(_hotKey.Value))
                {
                    ToggleHotkey();
                }            

            }
        }

        private void ToggleHotkey()
        {
            _useStack.Value = !_useStack.Value;
            this.Config.Save();
        }

        private void Patch()
        {
            harmony.PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} has loaded!");
            Logger.LogInfo($"Version: {PluginInfo.PLUGIN_VERSION}");
        }

        public bool CheckConfig(ConfigType configType)
        {
            bool configValue;

            switch (configType)
            {
                case ConfigType.ENABLED:
                    configValue = _enabled.Value;
                    break;
                case ConfigType.COLOR:
                    configValue = _useColor.Value;
                    break;
                case ConfigType.STACK:
                    configValue = _useStack.Value;
                    break;
                case ConfigType.COMMERCE:
                    configValue =_useCommerce.Value;
                    break;
                default:
                    configValue = true;
                    break;
            }

            return configValue;
        }

        public DisplayType CheckDisplay()
        {
            DisplayType displayType;
            displayType = _displayType.Value;
            return displayType;
        }

   
    }
}
