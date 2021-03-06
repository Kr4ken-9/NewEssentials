using System;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Players;
using NewEssentials.Extensions;
using OpenMod.API;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.API.Permissions;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Users.Events;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Players
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class AfkChecker : IAfkChecker, IAsyncDisposable
    {
        private readonly IEventBus m_EventBus;
        private readonly IUserManager m_UserManager;
        private readonly IPermissionChecker m_PermissionChecker;
        private IStringLocalizer m_StringLocalizer;
        private int m_Timeout;
        private bool m_AnnounceAFK;
        private bool m_KickAFK;
        private bool m_ServiceRunning;

        public AfkChecker(IEventBus eventBus, IRuntime runtime, IUserManager users, IPermissionChecker permissionChecker)
        {
            m_EventBus = eventBus;
            m_UserManager = users;
            m_PermissionChecker = permissionChecker;
            m_ServiceRunning = true;

            if (Level.isLoaded)
                GetCurrentPlayers();

            m_EventBus.Subscribe<UnturnedUserConnectedEvent>(runtime, (provider, sender, @event) => PlayerJoin(@event));

            AsyncHelper.Schedule("NewEssentials::AfkChecker", async () => await CheckAfkPlayers());
        }

        public void Configure(int timeout, bool announceAFK, bool kickAFK, IStringLocalizer stringLocalizer)
        {
            m_StringLocalizer = stringLocalizer;
            m_Timeout = timeout;
            m_AnnounceAFK = announceAFK;
            m_KickAFK = kickAFK;

            if (m_Timeout < 0)
                AsyncHelper.RunSync(async () => await DisposeAsync());
        }

        private async UniTask CheckAfkPlayers()
        {
            while (m_ServiceRunning)
            {
                await UniTask.Delay(10000);

                var users = await m_UserManager.GetUsersAsync(KnownActorTypes.Player);

                foreach (var user in users)
                {
                    if (!(user is UnturnedUser unturnedUser))
                        continue;

                    bool afk = (DateTime.Now.TimeOfDay - (TimeSpan)user.Session.SessionData["lastMovement"])
                                .TotalSeconds >=
                                m_Timeout &&
                                await m_PermissionChecker.CheckPermissionAsync(user, "Kr4ken.NewEssentials:afkchecker.exempt") !=
                                PermissionGrantResult.Grant;

                    if (!afk)
                        continue;

                    if (m_AnnounceAFK)
                        await Announce(m_StringLocalizer["afk:announcement", new { Player = unturnedUser.DisplayName }], Color.white);

                    if (m_KickAFK)
                        await unturnedUser.Player.Player.KickAsync(m_StringLocalizer["afk:kicked"]);
                }
            }
        }

        private async UniTask Announce(string text, Color color)
        {
            await UniTask.SwitchToMainThread();
            ChatManager.serverSendMessage(text, color);
        }

        #region Dictionary Population

        private async Task GetCurrentPlayers()
        {
            foreach (var user in (await m_UserManager.GetUsersAsync(KnownActorTypes.Player)).Cast<UnturnedUser>())
                if (!user.Session.SessionData.ContainsKey("lastMovement"))
                    user.Session.SessionData.Add("lastMovement", DateTime.Now.TimeOfDay);
        }

        private async Task PlayerJoin(UnturnedUserConnectedEvent @event)
        {
            await UniTask.SwitchToMainThread();
            @event.User.Session.SessionData.Add("lastMovement", DateTime.Now.TimeOfDay);

            PlayerMovementCheckerComponent component = @event.User.Player.Player.gameObject.AddComponent<PlayerMovementCheckerComponent>();
            component.Resolve(@event.User);
        }

        #endregion

        public async ValueTask DisposeAsync()
        {
            m_ServiceRunning = false;
        }
    }
}