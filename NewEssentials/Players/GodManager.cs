using Microsoft.Extensions.DependencyInjection;
using NewEssentials.API.Players;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.Unturned.Players.Life.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewEssentials.Players
{
    [PluginServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class GodManager : IGodManager
    {
        private readonly HashSet<ulong> m_Gods;
        
        //TODO: Add some harmony patches to prevent all damage e.g infection/dehydration/suffocation since Nelly selectively uses this event
        public GodManager(NewEssentials plugin,
            IEventBus eventBus)
        {
            m_Gods = new HashSet<ulong>();

            eventBus.Subscribe(plugin, (EventCallback<UnturnedPlayerDamagingEvent>)OnPlayerDamaging);
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

        private Task OnPlayerDamaging(IServiceProvider serviceProvider, object sender, UnturnedPlayerDamagingEvent @event)
        {
            if (m_Gods.Contains(@event.Player.SteamId.m_SteamID))
                @event.IsCancelled = true;

            return Task.CompletedTask;
        }
    }
}