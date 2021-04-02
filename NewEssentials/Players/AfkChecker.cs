using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Eventing;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Users.Events;
using SDG.Unturned;
using System;
using System.Linq;
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

            m_EventListener = m_EventBus.Subscribe<UnturnedUserConnectedEvent>(plugin, OnPlayerJoin);

            UniTask.RunOnThreadPool(CheckAfkPlayers);
        }

        private async UniTask CheckAfkPlayers()
        {
            if (Provider.isServer)
            {
                await SyncPlayers();
            }

            while (m_ServiceRunning)
            {
                await UniTask.SwitchToThreadPool();
                await UniTask.Delay(10000);

                var timeout = m_Configuration.GetSection("afkchecker:timeout").Get<int>();
                var announce = m_Configuration.GetSection("afkchecker:announceAFK").Get<bool>();
                var kick = m_Configuration.GetSection("afkchecker:kickAFK").Get<bool>();

                if (timeout < 0 || (!kick && !announce))
                {
                    continue;
                }

                foreach (var user in m_UnturnedUserDirectory.GetOnlineUsers().ToList())
                {
                    if (!user.Session.SessionData.TryGetValue("lastMovement", out object @time) || @time is not TimeSpan span)
                    {
                        continue;
                    }

                    bool afk = (DateTime.Now.TimeOfDay - span).TotalSeconds >= timeout
                        && await m_PermissionChecker.CheckPermissionAsync(user, "afkchecker.exempt") != PermissionGrantResult.Grant;

                    if (!afk)
                    {
                        user.Session.SessionData["isAfk"] = false;
                        continue;
                    }

                    if (!user.Session.SessionData.TryGetValue("isAfk", out var unparsedAfk) || unparsedAfk is not bool isAfk)
                    {
                        continue;
                    }

                    // todo: add announceTime and kickTime (not just timeout)

                    if (announce && !isAfk)
                    {
                        await user.Provider?.BroadcastAsync(m_StringLocalizer["afk:announcement", new { Player = user.DisplayName }],
                            Color.White);
                    }

                    if (kick)
                    {
                        await user.Session?.DisconnectAsync(m_StringLocalizer["afk:kicked"]);
                        continue;
                    }

                    user.Session.SessionData["isAfk"] = true;
                }
            }
        }

        #region Dictionary Population

        private async UniTask SyncPlayers()
        {
            await UniTask.SwitchToMainThread();

            foreach (var user in m_UnturnedUserDirectory.GetOnlineUsers().ToList())
            {
                if (!user.Session.SessionData.ContainsKey("lastMovement"))
                {
                    user.Session.SessionData.Add("lastMovement", DateTime.Now.TimeOfDay);
                }

                if (!user.Session.SessionData.ContainsKey("isAfk"))
                {
                    user.Session.SessionData.Add("isAfk", false);
                }

                var component = user.Player.Player.transform.GetOrAddComponent<PlayerMovementCheckerComponent>();
                component.Resolve(user);
            }
        }

        private async Task OnPlayerJoin(IServiceProvider serviceProvider, object sender, UnturnedUserConnectedEvent @event)
        {
            if (!@event.User.Session.SessionData.ContainsKey("lastMovement"))
            {
                @event.User.Session.SessionData.Add("lastMovement", DateTime.Now.TimeOfDay);
            }

            if (!@event.User.Session.SessionData.ContainsKey("isAfk"))
            {
                @event.User.Session.SessionData.Add("isAfk", false);
            }

            await UniTask.SwitchToMainThread();

            var component = @event.User.Player.Player.transform.GetOrAddComponent<PlayerMovementCheckerComponent>();
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