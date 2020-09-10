using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using NewEssentials.Models;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.Home
{
    [Command("set")]
    [CommandDescription("Save location as a home")]
    [CommandSyntax("[name]")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(CHome))]
    public class CSetHome : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserDataStore m_UserDataStore;

        public CSetHome(IStringLocalizer stringLocalizer, IUserDataStore userDataStore,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserDataStore = userDataStore;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            UserData userData = await m_UserDataStore.GetUserDataAsync(uPlayer.Id, uPlayer.Type);
            if (!userData.Data.ContainsKey("homes"))
                userData.Data.Add("homes", new Dictionary<string, SerializableVector3>());

            var homes = (Dictionary<object, object>) userData.Data["homes"];
            if (Context.Parameters.Length == 0)
            {
                homes["home"] = uPlayer.Player.Player.transform.position.ToSerializableVector3();
                userData.Data["homes"] = homes;

                await m_UserDataStore.SetUserDataAsync(userData);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["home:set", new {Home = "home"}]);
            }
            else
            {
                string home = Context.Parameters[0];
                homes[home] = uPlayer.Player.Player.transform.position.ToSerializableVector3();
                userData.Data["homes"] = homes;

                await m_UserDataStore.SetUserDataAsync(userData);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["home:set", new {Home = home}]);
            }
        }
    }
}