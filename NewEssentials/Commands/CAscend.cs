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
    [UsedImplicitly]
    [Command("ascend")]
    [CommandAlias("up")]
    [CommandDescription("Teleport up")]
    [CommandSyntax("[distance]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CAscend : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CAscend(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.ascend";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            
            //TODO: throw UserFriendlyException on bad input
            Vector3 newPosition = uPlayer.Player.transform.position;
            float upDistance = Context.Parameters.Length == 0 ? 10f : await Context.Parameters.GetAsync<float>(0);
            newPosition.y += upDistance;

            await UniTask.SwitchToMainThread();
            uPlayer.Player.teleportToLocationUnsafe(newPosition, uPlayer.Player.look.yaw);
            await uPlayer.PrintMessageAsync(m_StringLocalizer["ascend:success",
                new {Distance = upDistance.ToString()}]);
        }
    }
}