using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using HarmonyLib;

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
        private readonly ConfigEntry<int> _nexusID;

        private readonly Harmony _harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        //private static readonly bool AlreadyPatched = Harmony.GetPatchInfo((MethodBase)AccessTools.Method(typeof(Inventory), "fillHoverDescription", (Type[])null, (Type[])null)) != null;

        public enum DisplayType
        {
            Sell,
            Buy,
            Both
        }

        public enum ConfigType
        {
            Enabled,
            Color,
            Stack,
            Commerce
        }

        public Plugin()
        {
            _enabled = Config.Bind("Options", "Enabled", true, "You can disable the mod quickly by editing this value to false.");
            _useColor = Config.Bind("Options", "Color", true, "When true this will format the Value text to yellow.");
            _useStack = Config.Bind("Options", "Stack", true, "When true this will multiply the value of items by the item stack size.");
            _displayType = Config.Bind("Options", "Display", DisplayType.Sell, "Determines what type of information to display.");
            _useCommerce = Config.Bind("Options", "Commerce", true, "When true this will calculate the value using commerce licence level");
            _hotKey = Config.Bind<KeyCode>("General", "HotKey", KeyCode.LeftControl, "The Unity key-bind that will toggle display of stack price. disable with KeyCode.None");
            _nexusID = Config.Bind("TRTools", "NexusID", 1, "Don't alter this value as it is used by TRTools.");
        }

        private void Awake()
        {
            Instance = this;

            if (!CheckConfig(ConfigType.Enabled))
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
            _harmony.PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} has loaded!");
            Logger.LogInfo($"Version: {PluginInfo.PLUGIN_VERSION}");
        }

        public bool CheckConfig(ConfigType configType)
        {
            bool configValue;

            switch (configType)
            {
                case ConfigType.Enabled:
                    configValue = _enabled.Value;
                    break;
                case ConfigType.Color:
                    configValue = _useColor.Value;
                    break;
                case ConfigType.Stack:
                    configValue = _useStack.Value;
                    break;
                case ConfigType.Commerce:
                    configValue =_useCommerce.Value;
                    break;
                default:
                    configValue = true;
                    break;
            }
            
            Logger.LogInfo($"Checking {configType}.");
            return configValue;
        }

        public DisplayType CheckDisplay()
        {
            var displayType = _displayType.Value;
            return displayType;
        }
        
    }
}
