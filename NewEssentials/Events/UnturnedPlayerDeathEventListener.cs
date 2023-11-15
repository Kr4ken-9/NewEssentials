using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NewEssentials.Extensions;
using OpenMod.API.Eventing;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Unturned.Players.Life.Events;
using System.Threading.Tasks;
using NewEssentials.User;
using OpenMod.API.Permissions;
using OpenMod.Extensions.Games.Abstractions.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Events
{
    public class UnturnedPlayerDeathEventListener : IEventListener<UnturnedPlayerDeathEvent>
    {
        private readonly IUserDataStore m_UserDataStore;
        private readonly IUserDataSeeder m_UserDataSeeder;
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IUserProvider m_UserProvider;

        public UnturnedPlayerDeathEventListener(IUserDataStore userDataStore, IUserDataSeeder userDataSeeder, IPermissionChecker permissionChecker, IUserProvider users)
        {
            m_UserDataStore = userDataStore;
            m_UserDataSeeder = userDataSeeder;
            m_PermissionChecker = permissionChecker;
            m_UserProvider = users;
        }

        public async Task HandleEventAsync(object sender, UnturnedPlayerDeathEvent @event)
        {
            await UniTask.SwitchToMainThread();
            IUser usr = await m_UserProvider.ToUserAsync(@event.Player);
            var id = @event.Player.EntityInstanceId;
            var position = @event.Player.Transform.Position.ToSerializableVector();
            Dictionary<string, object?> toSeed = new Dictionary<string, object>(){{"deathLocation", position}};
            if (await m_PermissionChecker.CheckPermissionAsync(usr, "keepskills") ==
                PermissionGrantResult.Grant)
            {
                var skillSet = @event.Player.Player.skills.skills.ToArray();
                toSeed.Add("skillSet", skillSet);
            }
            UniTask.RunOnThreadPool(async () =>
            {
                var userData = await m_UserDataStore.GetUserDataAsync(id, KnownActorTypes.Player);
                if (userData == null)
                {
                    var displayName = @event.Player.SteamPlayer.playerID.characterName;
                    
                    await m_UserDataSeeder.SeedUserDataAsync(id, KnownActorTypes.Player, displayName,
                        toSeed);

                    return;
                }

                foreach (string k in toSeed.Keys)
                    userData.Data[k] = toSeed[k];
                
                await m_UserDataStore.SetUserDataAsync(userData);
            }).Forget();
        }
    }
}
