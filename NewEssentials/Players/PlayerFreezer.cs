using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NewEssentials.API.Players;
using NewEssentials.Extensions;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.Core.Helpers;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Players
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class PlayerFreezer : IPlayerFreezer, IAsyncDisposable
    {
        private readonly Dictionary<ulong, Vector3> m_FrozenPlayers;
        private bool m_ServiceRunning;
        
        public PlayerFreezer()
        {
            m_FrozenPlayers = new Dictionary<ulong, Vector3>();
            m_ServiceRunning = true;
            AsyncHelper.Schedule("Freeze Update", () => FreezeUpdate().AsTask());
            Provider.onEnemyDisconnected += RemovePlayer;
        }

        public void FreezePlayer(ulong player, Vector3 position) => m_FrozenPlayers.Add(player, position);
        public void UnfreezePlayer(ulong player) => m_FrozenPlayers.Remove(player);
        public bool IsPlayerFrozen(ulong player) => m_FrozenPlayers.ContainsKey(player);

        public async ValueTask DisposeAsync()
        {
            Provider.onEnemyDisconnected -= RemovePlayer;
            m_ServiceRunning = false;
        }

        private async UniTask FreezeUpdate()
        {
            while (m_ServiceRunning)
            {
                if (m_FrozenPlayers.Count > 0)
                {
                    ulong[] frozenPlayers = m_FrozenPlayers.Keys.ToArray();

                    for (int i = frozenPlayers.Length - 1; i >= 0; i--)
                    {
                        SteamPlayer player = PlayerTool.getSteamPlayer(frozenPlayers[i]);
                        if (player == null)
                        {
                            UnfreezePlayer(frozenPlayers[i]);
                            continue;
                        }

                        await player.player.TeleportToLocationUnsafeAsync(m_FrozenPlayers[frozenPlayers[i]]);
                    }
                }

                await UniTask.DelayFrame(1, PlayerLoopTiming.FixedUpdate);
            }
        }

        private void RemovePlayer(SteamPlayer gonePlayer)
        {
            ulong steamID = gonePlayer.playerID.steamID.m_SteamID;
            if (IsPlayerFrozen(steamID))
                UnfreezePlayer(steamID);
        }
    }
}