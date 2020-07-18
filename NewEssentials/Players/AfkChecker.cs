using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NewEssentials.API.Players;
using NewEssentials.Extensions;
using OpenMod.API;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.API.Permissions;
using OpenMod.API.Plugins;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using OpenMod.Core.Users.Events;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Players
{
    [PluginServiceImplementation]
    public class AfkChecker : IAfkChecker
    {
        
        //TODO: Add translations

        private Dictionary<IUser, TimeSpan> m_Users;
        
        private readonly IConfiguration m_Config;
        private readonly NewEssentials m_Plugin;
        private readonly IUserManager m_UserManager;
        private readonly IPermissionChecker m_PermissionChecker;
        

        public AfkChecker(IEventBus bus, IConfiguration config, NewEssentials plugin, IUserManager users, IPermissionChecker checker)
        {
            if (!config.GetValue<bool>("afkchecker:enabled"))
                return;
            
            m_Users = new Dictionary<IUser, TimeSpan>();
            
            bus.Subscribe<UserConnectedEvent>(plugin, (provider, sender, @event) => PlayerJoin(@event));
            bus.Subscribe<UserConnectedEvent>(plugin, (provider, sender, @event) => PlayerLeave(@event));
            
            
            AsyncHelper.Schedule("NewEssentials::AfkChecker", async () => await CheckAfkPlayers());
            m_Config = config;
            m_Plugin = plugin;
            m_UserManager = users;
            m_PermissionChecker = checker;
        }

        private async Task PlayerJoin(UserConnectedEvent @event)
        {
            if (m_Users.ContainsKey((UnturnedUser)@event.User))
            {
                m_Users[(UnturnedUser)@event.User] = DateTime.Now.TimeOfDay;
                return;
            }
            
            m_Users.Add((UnturnedUser)@event.User, DateTime.Now.TimeOfDay);
            PlayerMovementCheckerComponent component = ((UnturnedUser) @event.User).Player.gameObject.AddComponent<PlayerMovementCheckerComponent>();
            component.Resolve(this);

        }

        private async UniTask CheckAfkPlayers()
        {
            while (m_Plugin.IsComponentAlive)
            {
                await UniTask.Delay(3000);

                IUser user;
                
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < Provider.clients.Count; i++)
                {
                    user = await m_UserManager.FindUserAsync(KnownActorTypes.Player,
                        Provider.clients[i].playerID.steamID.ToString(), UserSearchMode.Id);
                    if (await ShouldBeKicked(user))
                        await ((UnturnedUser) user).KickAsync("You were kicked for being afk!");
                }
            }
        }

        private async Task<bool> ShouldBeKicked(IUser user)
        {
            return DateTime.Now.TimeOfDay.TotalSeconds - m_Users[user].TotalSeconds >=
                m_Config.GetValue<float>("afkchecker:timeout") && await m_PermissionChecker.CheckPermissionAsync(user, "afkchecker.exempt") != PermissionGrantResult.Grant;
        }
        
        private async Task PlayerLeave(UserConnectedEvent @event)
        {
            if (m_Users.ContainsKey((UnturnedUser)@event.User))
                m_Users.Remove((UnturnedUser)@event.User);
        }


        public async UniTask UpdateUser(IUser user)
        {
            if (m_Users.ContainsKey(user))
                m_Users[user] = DateTime.Now.TimeOfDay;
        }

        //TODO: Maybe switch to steam IDs in the future
        public async UniTask UpdatePlayer(Player player)
        {
            IUser user = m_UserManager.FindUserAsync(KnownActorTypes.Player, player.channel.owner.playerID.playerName,
                UserSearchMode.Name).Result;
            
            await UpdateUser(user);
        }

        
    }
}