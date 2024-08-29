using HarmonyLib;
using UnityEngine;

namespace ValueTooltip.Patches
{
    [HarmonyPatch(typeof(Inventory), "fillHoverDescription")]
    class CheckIfStackable
    {
        [HarmonyPostfix]
        [HarmonyPriority(1)]
        [HarmonyBefore("spicy.museumtooltip")]
        static void PostfixFillHoverDescription(ref Inventory __instance, ref InventorySlot rollOverSlot)
        {
            Plugin plugin = Plugin.Instance;
            InventoryItem item = rollOverSlot.itemInSlot;
            string desc = GetDescription(item);
            int value = CalculateValues(rollOverSlot);
            if (value == -1) return; // don't modify the description if the item is a deed or money
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
            GiveNPC give = GiveNPC.give;

            int value = 0;
            InventoryItem item = slot.itemInSlot;

            if (item.isDeed || item.getItemId() == Inventory.Instance.moneyItem.getItemId()) return -1;
                
            if (!item.isStackable || !plugin.CheckConfig(Plugin.ConfigType.Stack) || item.isATool || item.isPowerTool || item.hasFuel)
            {
                value = item.value;
            }
            else
            {
                value = item.value * slot.stack;
            }

            if(!slot.isDisabledForGive() && give.giveWindowOpen)// Only multiply the value if the slot is unlocked
            {
                // Npc Specific Multipliers
                switch (give.giveMenuTypeOpen)
                {
                    case GiveNPC.currentlyGivingTo.SellToBugComp:
                    case GiveNPC.currentlyGivingTo.SellToFishingComp:
                    case GiveNPC.currentlyGivingTo.SellToTuckshop:
                        value = Mathf.RoundToInt((float)value * 2.5f);
                        break;
                    case GiveNPC.currentlyGivingTo.SellToTrapper:
                        value = Mathf.RoundToInt((float)value * 2f);
                        break;
                    case GiveNPC.currentlyGivingTo.SellToJimmy:
                        value = Mathf.RoundToInt((float)value * 1.5f);
                        break;
                    case GiveNPC.currentlyGivingTo.Tech:
                        value = Mathf.RoundToInt((float)value * 6f);
                        break;
                    case GiveNPC.currentlyGivingTo.Sell:
                        if (item.relic)
                        {
                            value /= 4;
                        }
                        break;
                    default:
                        break;
                }
            }
            
            if (plugin.CheckConfig(Plugin.ConfigType.Commerce) && (plugin.CheckDisplay() != Plugin.DisplayType.Buy))
            {
                value += Mathf.RoundToInt((float) value / 20f * (float) LicenceManager.manage.allLicences[8].getCurrentLevel());
            }

            return value;
        }

        public static string GetDescription(InventoryItem item)
        {
            Inventory inv = Inventory.Instance;
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
                    result = $"\n{UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + value.ToString("n0"))}";
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
                    result = $"\n{UIAnimationManager.manage.MoneyAmountColorTag(" [Buy] " + "<sprite=11>" + price.ToString("n0"))}" +
                        $"{UIAnimationManager.manage.MoneyAmountColorTag(" [Sell] " + "<sprite=11>" + value.ToString("n0"))}";
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
