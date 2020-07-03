using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NewEssentials.API;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Managers
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class TPAManager : ITPAManager, IAsyncDisposable
    {
        private readonly Dictionary<ulong, HashSet<ulong>> m_OpenRequests = new Dictionary<ulong, HashSet<ulong>>();
        private IStringLocalizer m_StringLocalizer;

        public TPAManager()
        {
            Provider.onEnemyConnected += AddPlayer;
            Provider.onEnemyDisconnected += RemovePlayer;
        }

        public void SetLocalizer(IStringLocalizer stringLocalizer)
        {
            m_StringLocalizer = stringLocalizer;
        }

        public bool IsRequestOpen(ulong recipient, ulong requester)
        {
            return m_OpenRequests[recipient].Contains(requester);
        }

        public void OpenNewRequest(ulong recipient, ulong requester, int requestLifetime)
        {
            m_OpenRequests[recipient].Add(requester);
            
            var thread = new Thread(async () => await RequestExpirationThread(recipient, requester, requestLifetime));
            thread.Start();
        }

        private async Task RequestExpirationThread(ulong recipient, ulong requester, int lifetime)
        {
            Thread.Sleep(lifetime);
            m_OpenRequests[recipient].Remove(requester);

            SteamPlayer player = PlayerTool.getSteamPlayer(requester);
            if (player == null)
                return;

            await UniTask.SwitchToMainThread();
            ChatManager.serverSendMessage(m_StringLocalizer["tpa:expired", new {Recipient = player.playerID.characterName}], Color.red, toPlayer: player);
        }

        public async ValueTask DisposeAsync()
        {
            Provider.onEnemyConnected -= AddPlayer;
            Provider.onEnemyDisconnected -= RemovePlayer;
            await Task.Yield();
        }

        private void AddPlayer(SteamPlayer newPlayer) =>
            m_OpenRequests.Add(newPlayer.playerID.steamID.m_SteamID, new HashSet<ulong>());

        private void RemovePlayer(SteamPlayer gonePlayer) =>
            m_OpenRequests.Remove(gonePlayer.playerID.steamID.m_SteamID);
    }
}