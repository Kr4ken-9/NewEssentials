using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using NewEssentials.Models;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.Home
{
    [Command("home")]
    [CommandDescription("Teleport to your bed or one set with /home set")]
    [CommandSyntax("<name>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CHome : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserDataStore m_UserDataStore;

        public CHome(IStringLocalizer stringLocalizer, IUserDataStore userDataStore, IServiceProvider serviceProvider) :
            base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserDataStore = userDataStore;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            if (Context.Parameters.Length == 0)
            {
                await UniTask.SwitchToMainThread();
                if (uPlayer.Player.Player.teleportToBed())
                    await uPlayer.PrintMessageAsync(m_StringLocalizer["home:success", new {Home = "bed"}]);
                else
                    throw new UserFriendlyException(m_StringLocalizer["home:no_bed"]);
            }
            else
            {
                UserData userData = await m_UserDataStore.GetUserDataAsync(uPlayer.Id, uPlayer.Type);
                if (!userData.Data.ContainsKey("homes"))
                    throw new UserFriendlyException(m_StringLocalizer["home:no_home"]);

                var homes = (Dictionary<object, object>) userData.Data["homes"];
                string homeName = Context.Parameters[0];

                if (!homes.ContainsKey(homeName))
                    throw new UserFriendlyException(m_StringLocalizer["home:invalid_home", new {Home = homeName}]);

                SerializableVector3 home = SerializableVector3.GetSerializableVector3FromUserData(homes, homeName);

                if (await uPlayer.Player.Player.TeleportToLocationAsync(home.ToUnityVector3()))
                    await uPlayer.PrintMessageAsync(m_StringLocalizer["home:success", new {Home = homeName}]);
                else
                    throw new UserFriendlyException(m_StringLocalizer["home:failure", new {Home = homeName}]);
            }
        }
    }
}