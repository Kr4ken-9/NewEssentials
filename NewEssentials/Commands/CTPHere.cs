using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;
using System.Linq;

namespace NewEssentials.Commands
{
    [Command("tphere")]
    [CommandDescription("Tp a player to you")]
    [CommandSyntax("<player>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CTPHere : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserManager m_UserManager;

        public CTPHere(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer, IUserManager userManager) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserManager = userManager;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
            {
                throw new CommandWrongUsageException(Context);
            }

            var searchTerm = Context.Parameters[0];
            var user = await m_UserManager.FindUserAsync(KnownActorTypes.Player, searchTerm, UserSearchMode.FindByNameOrId);

            if (user is not UnturnedUser unturnedUser)
                throw new UserFriendlyException(m_StringLocalizer["general:invalid_player", new { Player = searchTerm }]);

            var callingPlayer = (UnturnedUser) Context.Actor;
            var position = callingPlayer.Player.Transform.Position;

            await unturnedUser.Player.SetPositionAsync(position);

            await unturnedUser.PrintMessageAsync(
                m_StringLocalizer["tphere:successful_tp", new {Player = user.DisplayName}]);
            await user.PrintMessageAsync(
                m_StringLocalizer["tphere:successful_tp_other", new {Player = unturnedUser.DisplayName}]);
        }
    }
}
