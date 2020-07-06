using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NewEssentials.API;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using SDG.Unturned;

namespace NewEssentials.Managers
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class PrivateMessageManager : IPrivateMessageManager, IAsyncDisposable
    {
        private readonly Dictionary<ulong, ulong> m_LastMessage;

        public PrivateMessageManager()
        {
            m_LastMessage = new Dictionary<ulong, ulong>();
            Provider.onEnemyDisconnected += RemovePlayer;
        }
        
        public void RecordLastMessager(ulong recipient, ulong sender)
        {
            if (m_LastMessage.ContainsKey(recipient))
                m_LastMessage[recipient] = sender;
            else
                m_LastMessage.Add(recipient, sender);
        }

        public ulong? GetLastMessager(ulong recipient)
        {
            if (!m_LastMessage.ContainsKey(recipient))
                return null;

            return m_LastMessage[recipient];
        }

        public async ValueTask DisposeAsync()
        {
            Provider.onEnemyDisconnected -= RemovePlayer;
            await Task.Yield();
        }

        private void RemovePlayer(SteamPlayer gonePlayer)
        {
            ulong gonePlayerSteamID = gonePlayer.playerID.steamID.m_SteamID;

            if (m_LastMessage.ContainsKey(gonePlayerSteamID))
                m_LastMessage.Remove(gonePlayerSteamID);
        }
    }
}