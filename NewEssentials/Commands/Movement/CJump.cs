using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Commands.Movement
{
    [Command("jump")]
    [CommandAlias("jmp")]
    [CommandDescription("Jump to where you're looking")]
    [CommandActor(typeof(UnturnedUser))]
    public class CJump : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        
        private readonly int COLLISION_NO_SKY = RayMasks.BLOCK_COLLISION - RayMasks.SKY;

        public CJump(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            await UniTask.SwitchToMainThread();
            
            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            Transform aim = uPlayer.Player.look.aim;
            
            if (!PhysicsUtility.raycast(new Ray(aim.position, aim.forward), out RaycastHit hit, 1024f,
                COLLISION_NO_SKY))
                throw new UserFriendlyException(m_StringLocalizer["jump:none"]);

            await uPlayer.Player.TeleportToLocationUnsafeAsync(hit.point + new Vector3(0f, 2f, 0f));
            await uPlayer.PrintMessageAsync(m_StringLocalizer["jump:success"]);
        }
    }
}