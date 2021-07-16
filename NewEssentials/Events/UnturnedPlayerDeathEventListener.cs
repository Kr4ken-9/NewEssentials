using Cysharp.Threading.Tasks;
using NewEssentials.Extensions;
using OpenMod.API.Eventing;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Unturned.Players.Life.Events;
using System.Threading.Tasks;

namespace NewEssentials.Events
{
    public class UnturnedPlayerDeathEventListener : IEventListener<UnturnedPlayerDeathEvent>
    {
        private readonly IUserDataStore m_UserDataStore;
        private readonly IUserDataSeeder m_UserDataSeeder;

        public UnturnedPlayerDeathEventListener(IUserDataStore userDataStore, IUserDataSeeder userDataSeeder)
        {
            m_UserDataStore = userDataStore;
            m_UserDataSeeder = userDataSeeder;
        }

        public async Task HandleEventAsync(object sender, UnturnedPlayerDeathEvent @event)
        {
            var id = @event.Player.EntityInstanceId;
            var position = @event.Player.Transform.Position.ToSerializableVector();

            await UniTask.SwitchToThreadPool();

            var userData = await m_UserDataStore.GetUserDataAsync(id, KnownActorTypes.Player);
            if (userData == null)
            {
                var displayName = @event.Player.SteamPlayer.playerID.characterName;

                await m_UserDataSeeder.SeedUserDataAsync(id, KnownActorTypes.Player, displayName,
                    new() { { "deathLocation", position } });

                return;
            }

            userData.Data["deathLocation"] = position;
            await m_UserDataStore.SetUserDataAsync(userData);
        }
    }
}
