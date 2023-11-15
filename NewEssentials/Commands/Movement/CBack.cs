using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;
using NewEssentials.Configuration.Serializable;
using NewEssentials.Unturned;

namespace NewEssentials.Commands.Movement
{
    [Command("back")]
    [CommandDescription("Teleport back to where you died")]
    [CommandActor(typeof(UnturnedUser))]
    public class CBack : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserDataStore m_UserDataStore;

        public CBack(IStringLocalizer stringLocalizer, IUserDataStore userDataStore, IServiceProvider serviceProvider) :
            base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserDataStore = userDataStore;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser)Context.Actor;
            var userData = await m_UserDataStore.GetUserDataAsync(uPlayer.Id, uPlayer.Type);

            if (!userData.Data.ContainsKey("deathLocation"))
            {
                throw new UserFriendlyException(m_StringLocalizer["back:none"]);
            }
            var backLocation = SerializableVector3.Deserialize(userData.Data["deathLocation"]);
            if (backLocation == null)
            {
                throw new UserFriendlyException(m_StringLocalizer["back:none"]);
            }

            await uPlayer.Player.Player.TeleportToLocationAsync(backLocation.ToUnityVector3());
            await PrintAsync(m_StringLocalizer["back:success"]);
        }
    }
}