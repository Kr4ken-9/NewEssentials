using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using NewEssentials.API.Players;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Players.Life.Events;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Priority = OpenMod.API.Prioritization.Priority;

namespace NewEssentials.Players
{
    [PluginServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class GodManager : IGodManager, IDisposable
    {
        private readonly HashSet<ulong> m_Gods;
        private readonly IDisposable m_EventListenerDamaging;

        public GodManager(NewEssentials plugin, IEventBus eventBus)
        {
            m_Gods = new HashSet<ulong>();

            m_EventListenerDamaging = eventBus.Subscribe<UnturnedPlayerDamagingEvent>(plugin, OnPlayerDamaging);
            Patches.OnStatUpdating += Patches_OnStatUpdating;
        }

        public bool ToggleGod(ulong steamID)
        {
            if (m_Gods.Contains(steamID))
            {
                m_Gods.Remove(steamID);
                return false;
            }

            m_Gods.Add(steamID);
            return true;
        }

        private bool Patches_OnStatUpdating(PlayerLife player)
        {
            return m_Gods.Contains(player.channel.owner.playerID.steamID.m_SteamID);
        }

        private Task OnPlayerDamaging(IServiceProvider serviceProvider, object sender, UnturnedPlayerDamagingEvent @event)
        {
            @event.IsCancelled = m_Gods.Contains(@event.Player.SteamId.m_SteamID);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            m_EventListenerDamaging?.Dispose();
            Patches.OnStatUpdating -= Patches_OnStatUpdating;
        }

        [HarmonyPatch(typeof(PlayerLife))]
        private static class Patches
        {
            // true -> cancel event
            public delegate bool StatUpdating(PlayerLife player);
            public static event StatUpdating OnStatUpdating;

            [HarmonyPrefix]
            [HarmonyPatch(nameof(PlayerLife.askStarve))]
            private static bool askStarve(PlayerLife __instance)
            {
                return !OnStatUpdating?.Invoke(__instance) ?? true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(nameof(PlayerLife.askDehydrate))]
            private static bool askDehydrate(PlayerLife __instance)
            {
                return !OnStatUpdating?.Invoke(__instance) ?? true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(nameof(PlayerLife.askInfect))]
            private static bool askInfect(PlayerLife __instance)
            {
                return !OnStatUpdating?.Invoke(__instance) ?? true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(nameof(PlayerLife.serverSetBleeding))]
            private static bool serverSetBleeding(PlayerLife __instance, bool newBleeding)
            {
                return !(newBleeding && (OnStatUpdating?.Invoke(__instance) ?? false));
            }

            [HarmonyPrefix]
            [HarmonyPatch(nameof(PlayerLife.serverSetLegsBroken))]
            private static bool serverSetLegsBroken(PlayerLife __instance, bool newLegsBroken)
            {
                return !(newLegsBroken && (OnStatUpdating?.Invoke(__instance) ?? false));
            }
        }
    }
}