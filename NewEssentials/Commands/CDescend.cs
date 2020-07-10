using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Users;
using UnityEngine;

namespace NewEssentials.Commands
{
    [Command("descend")]
    [CommandAlias("down")]
    [CommandDescription("Teleport down")]
    [CommandSyntax("[distance]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CDescend : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CDescend(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.descend";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            
            //TODO: throw UserFriendlyException on bad input
            Vector3 newPosition = uPlayer.Player.transform.position;
            float downDistance = Context.Parameters.Length == 0 ? 10f : await Context.Parameters.GetAsync<float>(0);
            newPosition.y -= downDistance;

            await UniTask.SwitchToMainThread();
            uPlayer.Player.teleportToLocationUnsafe(newPosition);
            await uPlayer.PrintMessageAsync(m_StringLocalizer["descend:success",
                new {Distance = downDistance.ToString()}]);
        }
    }
}