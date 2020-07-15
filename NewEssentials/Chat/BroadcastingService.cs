using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Plugins;
using OpenMod.Core.Helpers;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Chat
{
    public class BroadcastingService : IBroadcastingService
    {
        public bool IsActive { get; set; }

        private IConfiguration m_Configuration;
        private int m_BroadcastIndex;
        private IOpenModPlugin _plugin;

        public BroadcastingService(IConfiguration config, IOpenModPlugin plugin)
        {
            m_Configuration = config;
            _plugin = plugin;
            if (m_Configuration.GetValue<int>("broadcast:repeatingBroadcastInterval") > 0)
                AsyncHelper.Schedule("NewEssentials::Broadcasting", async ()  => await Broadcast(m_Configuration.GetValue<int>("broadcast:repeatingBroadcastInterval")));
        }
        
        
        private async UniTask ClearEffectCoroutine(float time)
        {
            await Task.Delay((int) time);

            foreach (SteamPlayer player in Provider.clients.Where(x => x != null))
                EffectManager.askEffectClearByID(m_Configuration.GetValue<ushort>("broadcasting:effectId"), player.playerID.steamID);

            IsActive = false;
        }
        
        public async Task StartBroadcast(int duration, string msg)
        {
            foreach (var player in Provider.clients.Where(x => x != null))
                EffectManager.sendUIEffect(m_Configuration.GetValue<ushort>("broadcasting:effectId"), 4205, player.playerID.steamID, true, msg);

            IsActive = true;

            await ClearEffectCoroutine(duration);
        }

        private async UniTask Broadcast(float time)
        {
            while (_plugin.IsComponentAlive)
            {
                await Task.Delay( (int) time);

                if (IsActive)
                    await Task.Delay((int) time);

                List<string> messages = m_Configuration.GetValue<List<string>>("broadcasting:broadcastMessages");

                string message = messages[m_BroadcastIndex];

                await StartBroadcast(m_Configuration.GetValue<int>("broadcasting:repeatingBroadcastStayTime"), message);

                if (++m_BroadcastIndex >= messages.Count)
                    m_BroadcastIndex = 0;
            }
        }
        
    }
}