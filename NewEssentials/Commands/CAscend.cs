using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using UnityEngine;

namespace NewEssentials.Commands
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
            Vector3 newPosition = uPlayer.Player.transform.position;
            float upDistance = Context.Parameters.Length == 0 ? 10f : await Context.Parameters.GetAsync<float>(0);
            newPosition.y += upDistance;

            await UniTask.SwitchToMainThread();
            uPlayer.Player.teleportToLocationUnsafe(newPosition);
            await uPlayer.PrintMessageAsync(m_StringLocalizer["ascend:success",
                new {Distance = upDistance.ToString()}]);
        }
    }
}