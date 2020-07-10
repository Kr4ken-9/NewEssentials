using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Users;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands
{
    [Command("jump")]
    [CommandAlias("jmp")]
    [CommandDescription("Jump to where you're looking")]
    [CommandActor(typeof(UnturnedUser))]
    public class CJump : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CJump(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.jump";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);
            
            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            Transform aim = uPlayer.Player.look.aim;
            if (!PhysicsUtility.raycast(new Ray(aim.position, aim.forward), out RaycastHit hit, 1024f,
                RayMasks.DAMAGE_SERVER))
                throw new UserFriendlyException(m_StringLocalizer["jump:none"]);

            if (hit.transform == null)
                throw new UserFriendlyException(m_StringLocalizer["jump:none"]);

            await UniTask.SwitchToMainThread();
            //TODO: Not working lmao
            uPlayer.Player.teleportToLocation(hit.transform.position);
            await uPlayer.PrintMessageAsync(m_StringLocalizer["jump:success"]);
        }
    }
}