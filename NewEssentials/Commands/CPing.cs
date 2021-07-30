using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Core.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace NewEssentials.Commands
{
    [Command("ping")]
    [CommandDescription("View your/player ping")]
    [CommandSyntax("[user]")]
    [RegisterCommandPermission("other", Description = "Allows to view ping of other player")]
    public class CPing : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CPing(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var targetActor = Context.Parameters.Count == 2
                ? await Context.Parameters.GetAsync<UnturnedUser>(0)
                : Context.Actor as UnturnedUser;

            if (targetActor != Context.Actor && await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
            {
                throw new NotEnoughPermissionException(Context, "other");
            }

            if (targetActor == null)
            {
                return;
            }

            await PrintAsync(m_StringLocalizer["ping:ping", new
            {
                User = targetActor,
                Ping = targetActor.Player.Player.channel.owner.ping * 1000f
            }]);
        }
    }
}
