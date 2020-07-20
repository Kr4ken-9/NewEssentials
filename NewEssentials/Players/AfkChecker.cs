using System;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
using OpenMod.Core.Users.Events;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Players
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class AfkChecker : IAfkChecker, IAsyncDisposable
    {
        private readonly IEventBus m_EventEventBus;
        private readonly IUserManager m_UserManager;
        private readonly IPermissionChecker m_PermissionChecker;
        private int m_Timeout;
        private bool m_ServiceRunning;

        public AfkChecker(IEventBus eventBus, IRuntime runtime, IUserManager users, IPermissionChecker permissionChecker)
        {
            m_EventEventBus = eventBus;
            m_UserManager = users;
            m_PermissionChecker = permissionChecker;
            m_ServiceRunning = true;

            if (Level.isLoaded)
                GetCurrentPlayers();
            
            m_EventEventBus.Subscribe<UserConnectedEvent>(runtime, (provider, sender, @event) => PlayerJoin(@event));

            AsyncHelper.Schedule("NewEssentials::AfkChecker", async () => await CheckAfkPlayers());
        }

        public void Configure(int timeout)
        {
            m_Timeout = timeout;
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

                    if (await ShouldBeKicked(unturnedUser))
                        await unturnedUser.Player.KickAsync("You were kicked for being afk!");
                }
            }
        }

        private async Task<bool> ShouldBeKicked(UnturnedUser user)
        {
            return (DateTime.Now.TimeOfDay - (TimeSpan) user.Session.SessionData["lastMovement"]).TotalSeconds >=
                m_Timeout && await m_PermissionChecker.CheckPermissionAsync(user, "afkchecker.exempt") !=
                PermissionGrantResult.Grant;
        }
        
        #region Dictionary Population
        
        private async Task GetCurrentPlayers()
        {
            foreach (var user in (await m_UserManager.GetUsersAsync(KnownActorTypes.Player)).Cast<UnturnedUser>())
                if (!user.Session.SessionData.ContainsKey("lastMovement"))
                    user.Session.SessionData.Add("lastMovement", DateTime.Now.TimeOfDay);
        }

        private async Task PlayerJoin(UserConnectedEvent @event)
        {
            UnturnedUser newUser = (UnturnedUser) @event.User;
            newUser.Session.SessionData.Add("lastMovement", DateTime.Now.TimeOfDay);

            PlayerMovementCheckerComponent component = newUser.Player.gameObject.AddComponent<PlayerMovementCheckerComponent>();
            component.Resolve(newUser);

        }
        
        #endregion

        public async ValueTask DisposeAsync()
        {
            m_ServiceRunning = false;
        }
    }
}