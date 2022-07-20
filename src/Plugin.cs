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

        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

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
            STACK
        }

        public Plugin()
        {
            _enabled = Config.Bind("Options", "Enabled", true, "You can disable the mod quickly by editing this value to false.");
            _useColor = Config.Bind("Options", "Color", true, "When true this will format the Value text to yellow.");
            _useStack = Config.Bind("Options", "Stack", true, "When true this will multiply the value of items by the item stack size.");
            _displayType = Config.Bind("Options", "Display", DisplayType.SELL, "Determines what type of information to display.");
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

        private void Patch()
        {
            harmony.PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} has loaded!");
            Logger.LogInfo($"Version: {PluginInfo.PLUGIN_VERSION}");
        }

        bool CheckConfig(ConfigType configType)
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
                default:
                    configValue = true;
                    break;
            }

            return configValue;
        }

        DisplayType CheckDisplay()
        {
            DisplayType displayType;
            displayType = _displayType.Value;
            return displayType;
        }

        [HarmonyPatch(typeof(Inventory), "fillHoverDescription")]
        class Tooltip_Patch
        {
            [HarmonyPostfix]
            static void setDescriptionPatch(ref Inventory __instance, ref InventorySlot rollOverSlot)
            {
                Plugin plugin = Plugin.Instance;
                InventoryItem item = rollOverSlot.itemInSlot;

                int value;
                int price;

                if (!item.isStackable || !plugin.CheckConfig(ConfigType.STACK) || item.isATool || item.isPowerTool || item.hasFuel)
                {
                    value = item.value;
                }
                else
                {
                    value = item.value * rollOverSlot.stack;
                }

                price = value * 2;

                if(plugin.CheckDisplay() == DisplayType.BUY)
                {
                    value = price;
                }

                string desc = item.getItemDescription(__instance.getInvItemId(item));

                if (plugin.CheckDisplay() != DisplayType.BOTH)
                {
                    if (plugin.CheckConfig(ConfigType.COLOR))
                    {
                        __instance.InvDescriptionText.text = desc + $"\n{UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + value.ToString("n0"))}";
                    }
                    else
                    {
                        __instance.InvDescriptionText.text = desc + $"\n <sprite=11>{value}";
                    }
                }
                else
                {
                    if (plugin.CheckConfig(ConfigType.COLOR))
                    {
                        __instance.InvDescriptionText.text = desc + $"\n{UIAnimationManager.manage.moneyAmountColorTag(" [Buy] " + "<sprite=11>"  + price.ToString("n0"))}" +
                            $"{UIAnimationManager.manage.moneyAmountColorTag(" [Sell] " +"<sprite=11>" + value.ToString("n0"))}";
                    }
                    else
                    {
                        __instance.InvDescriptionText.text = desc  +  $"\n" + " [Buy] " + "<sprite=11>" + price + " [Sell] " + $"<sprite=11>{value}";
                    }
                }

                value = 0;
            }

        }
    }
}
