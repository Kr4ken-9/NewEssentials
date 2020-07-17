using System;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NewEssentials.API.Chat;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.Core.Helpers;
using SDG.Unturned;

namespace NewEssentials.Chat
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class BroadcastingService : IBroadcastingService, IAsyncDisposable
    {
        public bool IsActive { get; set; }
        private bool m_ServiceRunning;
        private int m_BroadcastIndex;

        private ushort m_EffectID;
        private string[] m_RepeatingMessages;
        private int m_RepeatingInterval;
        private int m_RepeatingDuration;
        
        public BroadcastingService()
        {
            m_ServiceRunning = true;
            IsActive = false;
        }

        public void Configure(ushort effectID, string[] repeatingMessages, int repeatingInterval, int repeatingDuration)
        {
            m_EffectID = effectID;
            
            if (repeatingInterval <= 0)
                return;

            m_RepeatingInterval = repeatingInterval;
            m_RepeatingMessages = repeatingMessages;
            m_RepeatingDuration = repeatingDuration;

            AsyncHelper.Schedule("NewEssentials::Broadcasting", () => RepeatingBroadcast().AsTask());
        }

        private async UniTask ClearEffectCoroutine(int time)
        {
            await UniTask.Delay(time);

            await UniTask.SwitchToMainThread();

            foreach (SteamPlayer player in Provider.clients.Where(x => x != null))
                EffectManager.askEffectClearByID(m_EffectID, player.playerID.steamID);

            IsActive = false;
        }
        
        public async UniTask StartBroadcast(int duration, string msg)
        {
            await UniTask.SwitchToMainThread();

            if (Provider.clients.Count < 1)
                return;
            
            foreach (var player in Provider.clients)
                EffectManager.sendUIEffect(m_EffectID, 4205, player.playerID.steamID, true, msg);

            IsActive = true;

            await ClearEffectCoroutine(duration);
        }

        private async UniTask RepeatingBroadcast()
        {
            while (m_ServiceRunning)
            {
                await UniTask.Delay(m_RepeatingInterval);

                while (IsActive || Provider.clients.Count < 1)
                    await UniTask.Delay(m_RepeatingInterval);
                
                string message = m_RepeatingMessages[m_BroadcastIndex];

                await StartBroadcast(m_RepeatingDuration, message);
                await UniTask.Delay(m_RepeatingDuration);
                
                if (++m_BroadcastIndex >= m_RepeatingMessages.Length)
                    m_BroadcastIndex = 0;
            }
        }

        public async ValueTask DisposeAsync()
        {
            m_ServiceRunning = false;
            if (IsActive)
            {
                await ClearEffectCoroutine(0);
                IsActive = false;
            }
        }
    }
}