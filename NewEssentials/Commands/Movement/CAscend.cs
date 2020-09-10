using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using UnityEngine;

namespace NewEssentials.Commands.Movement
{
    [Command("ascend")]
    [CommandAlias("up")]
    [CommandDescription("Teleport up")]
    [CommandSyntax("[distance]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CAscend : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CAscend(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            
            //TODO: throw UserFriendlyException on bad input
            Vector3 newPosition = uPlayer.Player.Player.transform.position;
            float upDistance = Context.Parameters.Length == 0 ? 10f : await Context.Parameters.GetAsync<float>(0);
            newPosition.y += upDistance;

            await uPlayer.Player.Player.TeleportToLocationUnsafeAsync(newPosition);
            await uPlayer.PrintMessageAsync(m_StringLocalizer["ascend:success",
                new {Distance = upDistance.ToString()}]);
        }
    }
}