using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NewEssentials.API.Players;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using SDG.Unturned;

namespace NewEssentials.Players
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class GodManager : IGodManager, IAsyncDisposable
    {
        private readonly HashSet<ulong> m_Gods;
        
        //TODO: Add some harmony patches to prevent all damage e.g infection/dehydration/suffocation since Nelly selectively uses this event
        public GodManager()
        {
            m_Gods = new HashSet<ulong>();
            DamageTool.damagePlayerRequested += onDamagePlayerRequested;
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

        private void onDamagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            if (m_Gods.Contains(parameters.player.channel.owner.playerID.steamID.m_SteamID))
                shouldAllow = false;
        }

        public async ValueTask DisposeAsync()
        {
            DamageTool.damagePlayerRequested -= onDamagePlayerRequested;
        }
    }
}