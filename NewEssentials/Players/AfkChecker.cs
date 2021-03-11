using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Eventing;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Users.Events;
using SDG.Unturned;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Color = System.Drawing.Color;

namespace NewEssentials.Players
{
    public class AfkChecker : IAsyncDisposable
    {
        private readonly IEventBus m_EventBus;
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IDisposable m_EventListener;

        private bool m_ServiceRunning;

        public AfkChecker(IEventBus eventBus, NewEssentials plugin, IConfiguration configuration,
            IStringLocalizer stringLocalizer, IUnturnedUserDirectory unturnedUserDirectory, IPermissionChecker permissionChecker)
        {
            m_EventBus = eventBus;
            m_Configuration = configuration;
            m_StringLocalizer = stringLocalizer;
            m_UnturnedUserDirectory = unturnedUserDirectory;
            m_PermissionChecker = permissionChecker;
            m_ServiceRunning = true;

            if (Provider.isServer)
                SyncPlayers();

            m_EventListener = m_EventBus.Subscribe<UnturnedUserConnectedEvent>(plugin, OnPlayerJoin);

            UniTask.RunOnThreadPool(CheckAfkPlayers);
        }

        private async UniTask CheckAfkPlayers()
        {
            while (m_ServiceRunning)
            {
                await UniTask.SwitchToThreadPool();
                await UniTask.Delay(10000);

                var timeout = m_Configuration.GetSection("afkchecker:timeout").Get<int>();
                var announce = m_Configuration.GetSection("afkchecker:announceAFK").Get<bool>();
                var kick = m_Configuration.GetSection("afkchecker:kickAFK").Get<bool>();

                if (timeout < 0)
                {
                    continue;
                }

                foreach (var user in m_UnturnedUserDirectory.GetOnlineUsers())
                {
                    if (!user.Session.SessionData.TryGetValue("lastMovement", out object @time) || @time is not TimeSpan span)
                    {
                        continue;
                    }

                    bool afk = (DateTime.Now.TimeOfDay - span).TotalSeconds >= timeout
                        && await m_PermissionChecker.CheckPermissionAsync(user, "afkchecker.exempt") != PermissionGrantResult.Grant;

                    if (!afk)
                        continue;

                    if (announce)
                    {
                        await user.Provider?.BroadcastAsync(m_StringLocalizer["afk:announcement", new { Player = user.DisplayName }],
                            Color.White);
                    }

                    if (kick)
                        await user.Session.DisconnectAsync(m_StringLocalizer["afk:kicked"]);
                }
            }
        }

        #region Dictionary Population

        private void SyncPlayers()
        {
            foreach (var user in m_UnturnedUserDirectory.GetOnlineUsers())
            {
                if (!user.Session.SessionData.ContainsKey("lastMovement"))
                    user.Session.SessionData.Add("lastMovement", DateTime.Now.TimeOfDay);
            }
        }

        private async Task OnPlayerJoin(IServiceProvider serviceProvider, object sender, UnturnedUserConnectedEvent @event)
        {
            if (@event.User.Session.SessionData.ContainsKey("lastMovement"))
                return;

            await UniTask.SwitchToMainThread();
            @event.User.Session.SessionData.Add("lastMovement", DateTime.Now.TimeOfDay);

            PlayerMovementCheckerComponent component = @event.User.Player.Player.gameObject.AddComponent<PlayerMovementCheckerComponent>();
            component.Resolve(@event.User);
        }

        #endregion

        public async ValueTask DisposeAsync()
        {
            m_ServiceRunning = false;
            m_EventListener?.Dispose();

            await UniTask.SwitchToMainThread();
            foreach (var user in m_UnturnedUserDirectory.GetOnlineUsers())
            {
                user.Player.Player.transform.DestroyComponentIfExists<PlayerMovementCheckerComponent>();
            }
        }
    }
}