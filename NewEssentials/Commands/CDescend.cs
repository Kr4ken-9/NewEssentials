using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using UnityEngine;

namespace NewEssentials.Commands
{
    [Command("descend")]
    [CommandAlias("down")]
    [CommandDescription("Teleport down")]
    [CommandSyntax("[distance]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CDescend : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CDescend(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
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
            float downDistance = Context.Parameters.Length == 0 ? 10f : await Context.Parameters.GetAsync<float>(0);
            newPosition.y -= downDistance;

            await uPlayer.Player.TeleportToLocationUnsafeAsync(newPosition);
            await uPlayer.PrintMessageAsync(m_StringLocalizer["descend:success",
                new {Distance = downDistance.ToString()}]);
        }
    }
}