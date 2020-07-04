using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<ulong, List<ulong>> m_OpenRequests;
        private IStringLocalizer m_StringLocalizer;

        public TPAManager()
        {
            m_OpenRequests = new Dictionary<ulong, List<ulong>>();
            Provider.onEnemyConnected += AddPlayer;
            Provider.onEnemyDisconnected += RemovePlayer;
        }

        public void SetLocalizer(IStringLocalizer stringLocalizer)
        {
            m_StringLocalizer = stringLocalizer;
        }

        public bool IsRequestOpen(ulong recipient) => m_OpenRequests[recipient].Count > 0;

        public bool IsRequestOpen(ulong recipient, ulong requester) => m_OpenRequests[recipient].Contains(requester);

        public void OpenNewRequest(ulong recipient, ulong requester, int requestLifetime)
        {
            m_OpenRequests[recipient].Add(requester);
            
            var thread = new Thread(async () => await RequestExpirationThread(recipient, requester, requestLifetime));
            thread.Start();
        }

        public ulong AcceptRequest(ulong recipient)
        {
            ulong requester = m_OpenRequests[recipient].First();
            m_OpenRequests[recipient].Remove(requester);

            return requester;
        }

        public void AcceptRequest(ulong recipient, ulong requester) => m_OpenRequests[recipient].Remove(requester);

        private async Task RequestExpirationThread(ulong recipient, ulong requester, int lifetime)
        {
            Thread.Sleep(lifetime);

            if (!m_OpenRequests[recipient].Contains(requester))
                return;
            
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
            m_OpenRequests.Add(newPlayer.playerID.steamID.m_SteamID, new List<ulong>());

        private void RemovePlayer(SteamPlayer gonePlayer) =>
            m_OpenRequests.Remove(gonePlayer.playerID.steamID.m_SteamID);
    }
}