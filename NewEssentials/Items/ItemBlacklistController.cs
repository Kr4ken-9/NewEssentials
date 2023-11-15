using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using SDG.Unturned;
using NotImplementedException = System.NotImplementedException;
using Priority = OpenMod.API.Prioritization.Priority;

namespace NewEssentials.Items;

[ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
public class ItemBlacklistController
{
    public ItemBlacklistController(IEventBus bus)
    {
        ItemManager.onTakeItemRequested += OnTakeItemRequested;
    }

    private void OnTakeItemRequested(Player player, byte x, byte y, uint instanceid, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemdata, ref bool shouldallow)
    {
        
    }
}

[HarmonyPatch(typeof(ItemManager))]
public class ItemsPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ItemManager.ReceiveTakeItemRequest))]
    private static bool askInfect(ItemManager __instance)
    {
        return !OnStatUpdating?.Invoke(__instance) ?? true;
    }
}