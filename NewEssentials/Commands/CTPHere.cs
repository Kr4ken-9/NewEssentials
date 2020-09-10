using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            string searchTerm = Context.Parameters[0];
            IUser user = await m_UserManager.FindUserAsync(KnownActorTypes.Player, searchTerm, UserSearchMode.FindByName);
            if (user == null || !(Context.Actor is UnturnedUser uPlayer))
            {
                await PrintAsync(m_StringLocalizer["general:invalid_player", new { Player = searchTerm }]);
                return;
            }

            // Player is offline
            if (Provider.clients.All(x => x.playerID.steamID != uPlayer.SteamId))
            {
                await PrintAsync(m_StringLocalizer["general:invalid_player", new { Player = searchTerm }]);
                return;
            }

            Player tpPlayer = uPlayer.Player.Player;
            await tpPlayer.TeleportToLocationAsync(uPlayer.Player.Player.transform.position);
            
            await uPlayer.PrintMessageAsync(m_StringLocalizer["tphere:successful_tp", new { Player = user.DisplayName }]);
            await user.PrintMessageAsync(m_StringLocalizer["tphere:successful_tp_other", new {Player = uPlayer.DisplayName}]);
        }
    }
}
