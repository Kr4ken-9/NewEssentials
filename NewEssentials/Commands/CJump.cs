using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Commands
{
    [Command("jump")]
    [CommandAlias("jmp")]
    [CommandDescription("Jump to where you're looking")]
    [CommandActor(typeof(UnturnedUser))]
    public class CJump : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CJump(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
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