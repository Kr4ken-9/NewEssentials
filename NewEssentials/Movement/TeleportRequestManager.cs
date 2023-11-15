using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Players;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.Core.Helpers;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Movement
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class TeleportRequestManager : ITeleportRequestManager, IAsyncDisposable
    {
        private readonly Dictionary<ulong, List<ulong>> m_OpenRequests;
        private IStringLocalizer m_StringLocalizer;

        public TeleportRequestManager()
        {
            m_OpenRequests = new Dictionary<ulong, List<ulong>>();

            if (Level.isLoaded)
                foreach (var player in Provider.clients)
                    m_OpenRequests.Add(player.playerID.steamID.m_SteamID, new List<ulong>());

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
            
            AsyncHelper.Schedule("NewEssentials::TPARequestExpiration", () => RequestExpirationThread(recipient, requester, requestLifetime).AsTask());
        }

        public ulong AcceptRequest(ulong recipient)
        {
            ulong requester = m_OpenRequests[recipient].First();
            m_OpenRequests[recipient].Remove(requester);

            return requester;
        }

        public void AcceptRequest(ulong recipient, ulong requester) => m_OpenRequests[recipient].Remove(requester);

        private async UniTask RequestExpirationThread(ulong recipientID, ulong requesterID, int lifetime)
        {
            await UniTask.Delay(TimeSpan.FromMilliseconds(lifetime));

            if (!m_OpenRequests[recipientID].Contains(requesterID))
                return;
            
            m_OpenRequests[recipientID].Remove(requesterID);

            SteamPlayer requester = PlayerTool.getSteamPlayer(requesterID);
            if (requester == null)
                return;

            SteamPlayer recipient = PlayerTool.getSteamPlayer(recipientID);
            if (recipient == null)
                return;

            await UniTask.SwitchToMainThread();
            ChatManager.serverSendMessage(m_StringLocalizer["tpa:expired", new {Recipient = recipient.playerID.characterName}], Color.red, toPlayer: requester, useRichTextFormatting: true);
        }

        public async ValueTask DisposeAsync()
        {
            Provider.onEnemyConnected -= AddPlayer;
            Provider.onEnemyDisconnected -= RemovePlayer;
        }

        private void AddPlayer(SteamPlayer newPlayer)
        {
            var id = newPlayer.playerID.steamID.m_SteamID;
            if (m_OpenRequests.ContainsKey(id))
                return;
            
            m_OpenRequests.Add(newPlayer.playerID.steamID.m_SteamID, new List<ulong>());
        }

        private void RemovePlayer(SteamPlayer gonePlayer) =>
            m_OpenRequests.Remove(gonePlayer.playerID.steamID.m_SteamID);
    }
}