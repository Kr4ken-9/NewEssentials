using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.Models;
using OpenMod.API.Permissions;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using UnityEngine;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.Home
{
    [UsedImplicitly]
    [Command("home")]
    [CommandDescription("Teleport to your bed or one set with /sethome")]
    [CommandSyntax("<name>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CHome : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserDataStore m_UserDataStore;
        
        public CHome(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IUserDataStore userDataStore, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
            m_UserDataStore = userDataStore;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.home";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            if (Context.Parameters.Length == 0)
            {
                await UniTask.SwitchToMainThread();
                if (uPlayer.Player.teleportToBed())
                    await uPlayer.PrintMessageAsync(m_StringLocalizer["home:success", new {Home = "bed"}]);
                else
                    await uPlayer.PrintMessageAsync(m_StringLocalizer["home:no_bed"]);
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

                await UniTask.SwitchToMainThread();
                if (uPlayer.Player.teleportToLocation(home.ToUnityVector3(), uPlayer.Player.look.yaw))
                    await uPlayer.PrintMessageAsync(m_StringLocalizer["home:success", new {Home = homeName}]);
                else
                    throw new UserFriendlyException(m_StringLocalizer["home:failure", new {Home = homeName}]);
            }
        }
    }
}