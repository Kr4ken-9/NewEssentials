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
        private readonly IServiceProvider m_ServiceProvider;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserManager m_UserManager;

        public CTPHere(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer, IUserManager userManager) :base(serviceProvider)
        {
            m_ServiceProvider = serviceProvider;
            m_StringLocalizer = stringLocalizer;
            m_UserManager = userManager;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
            {
                throw new CommandWrongUsageException(Context);
            }

            IUser user = await m_UserManager.FindUserAsync(KnownActorTypes.Player, Context.Parameters[0], UserSearchMode.Name);
            if (user == null)
            {
                await PrintAsync(m_StringLocalizer["general:invalid_player", new { Player = Context.Parameters[0] }]);
                return;
            }

            UnturnedUser uPlayer = (UnturnedUser)Context.Actor;
            Player tpPlayer = (user as UnturnedUser).Player;
            await tpPlayer.TeleportToLocationAsync(uPlayer.Player.transform.position);
            await PrintAsync(m_StringLocalizer["tphere:successfull_tp", new { Player = user.DisplayName }]);
        }
    }
}
