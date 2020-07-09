using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using NewEssentials.Models;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands
{
    [UsedImplicitly]
    [Command("back")]
    [CommandDescription("Teleport back to where you died")]
    [CommandActor(typeof(UnturnedUser))]
    public class CBack : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserDataStore m_UserDataStore;
        
        public CBack(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer,
            IUserDataStore userDataStore, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
            m_UserDataStore = userDataStore;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.back";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            var userData = await m_UserDataStore.GetUserDataAsync(uPlayer.Id, uPlayer.Type);

            if (!userData.Data.ContainsKey("deathLocation"))
                throw new UserFriendlyException(m_StringLocalizer["back:none"]);

            var backLocation =
                SerializableVector3.GetSerializableVector3FromUserData(
                    (Dictionary<object, object>) userData.Data["deathLocation"]);

            await UniTask.SwitchToMainThread();
            uPlayer.Player.teleportToLocation(backLocation.ToUnityVector3());
            await uPlayer.PrintMessageAsync(m_StringLocalizer["back:success"]);
        }
    }
}