using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.API.Commands;
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
            await Context.Actor.PrintMessageAsync(m_StringLocalizer["general:experimental"]);
            
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            await UniTask.SwitchToMainThread();
            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            Transform aim = uPlayer.Player.look.aim;
            if (!PhysicsUtility.raycast(new Ray(aim.position, aim.forward), out RaycastHit hit, 1024f,
                RayMasks.BLOCK_COLLISION))
                throw new UserFriendlyException(m_StringLocalizer["jump:none"]);

            if (hit.transform == null)
                throw new UserFriendlyException(m_StringLocalizer["jump:none"]);

            await uPlayer.Player.TeleportToLocationAsync(hit.transform.position);
            await uPlayer.PrintMessageAsync(m_StringLocalizer["jump:success"]);
        }
    }
}