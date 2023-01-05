using HarmonyLib;
using UnityEngine;

namespace ValueTooltip.Patches
{
    [HarmonyPatch(typeof(Inventory), "fillHoverDescription")]
    class CheckIfStackable
    {
        Plugin plugin = Plugin.Instance;

        [HarmonyPostfix]
        [HarmonyPriority(1)]
        [HarmonyBefore("spicy.museumtooltip")]
        static void PostfixFillHoverDescription(ref Inventory __instance, ref InventorySlot rollOverSlot)
        {
            Plugin plugin = Plugin.Instance;
            InventoryItem item = rollOverSlot.itemInSlot;
            string desc = GetDescription(item);
            int value = CalculateValues(rollOverSlot);
            int price = value * 2;
                       
            if (plugin.CheckDisplay() == Plugin.DisplayType.Buy)
            {
                value = price;
            }

            __instance.InvDescriptionText.text = desc + GetDisplayString(value,price);
            
            value = 0;
        }

        public static int CalculateValues(InventorySlot slot)
        {
            Plugin plugin = Plugin.Instance;

            int value = 0;
            InventoryItem item = slot.itemInSlot;

            if (!item.isStackable || !plugin.CheckConfig(Plugin.ConfigType.Stack) || item.isATool || item.isPowerTool || item.hasFuel)
            {
                value = item.value;
            }
            else
            {
                value = item.value * slot.stack;
            }

            if(plugin.CheckConfig(Plugin.ConfigType.Commerce) && (plugin.CheckDisplay() != Plugin.DisplayType.Buy))
            {
                value += Mathf.RoundToInt((float) value / 20f * (float) LicenceManager.manage.allLicences[8].getCurrentLevel());
            }

            return value;
        }

        public static string GetDescription(InventoryItem item)
        {
            Inventory inv = Inventory.inv;
            string desc = item.getItemDescription(inv.getInvItemId(item));
            return desc;
        }

        public static string GetDisplayString(int value, int price)
        {
            Plugin plugin = Plugin.Instance;
            string result;

            if (plugin.CheckDisplay() != Plugin.DisplayType.Both)
            {
                if((plugin.CheckConfig(Plugin.ConfigType.Color)))
                {
                    result = $"\n{UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + value.ToString("n0"))}";
                }
                else
                {
                    result =  $"\n <sprite=11>{value}";
                }               
            }
            else
            {
                if (plugin.CheckConfig(Plugin.ConfigType.Color))
                {
                    result = $"\n{UIAnimationManager.manage.moneyAmountColorTag(" [Buy] " + "<sprite=11>" + price.ToString("n0"))}" +
                        $"{UIAnimationManager.manage.moneyAmountColorTag(" [Sell] " + "<sprite=11>" + value.ToString("n0"))}";
                }
                else
                {
                   result = $"\n" + " [Buy] " + "<sprite=11>" + price + " [Sell] " + $"<sprite=11>{value}";
                }
            }
                                      
            return result;
        }

    }
}
