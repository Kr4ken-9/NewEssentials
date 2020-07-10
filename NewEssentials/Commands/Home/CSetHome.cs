using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Models;
using OpenMod.API.Permissions;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.Home
{
    [Command("sethome")]
    [CommandDescription("Save location as a home")]
    [CommandSyntax("<name>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CSetHome : UnturnedCommand
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserDataStore m_UserDataStore;
        
        public CSetHome(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IUserDataStore userDataStore, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
            m_UserDataStore = userDataStore;
        }

        protected override async UniTask OnExecuteAsync()
        {
            string permission = "newess.home.set";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);
            
            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            UserData userData = await m_UserDataStore.GetUserDataAsync(uPlayer.Id, uPlayer.Type);
            if (!userData.Data.ContainsKey("homes"))
                userData.Data.Add("homes", new Dictionary<string, SerializableVector3>());

            var homes = (Dictionary<object, object>)userData.Data["homes"];
            if (Context.Parameters.Length == 0)
            {
                homes["home"] = uPlayer.Player.transform.position.ToSerializableVector3();
                userData.Data["homes"] = homes;

                await m_UserDataStore.SaveUserDataAsync(userData);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["home:set", new {Home = "home"}]);
            }
            else
            {
                string home = Context.Parameters[0];
                homes[home] = uPlayer.Player.transform.position.ToSerializableVector3();
                userData.Data[home] = homes;

                await m_UserDataStore.SaveUserDataAsync(userData);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["home:set", new {Home = home}]);
            }
        }
    }
}