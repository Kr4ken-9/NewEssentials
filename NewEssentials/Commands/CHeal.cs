using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Commands
{
    [Command("heal")]
    [CommandDescription("Heal yourself or another player")]
    [CommandSyntax("[player]")]
    public class CHeal : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CHeal(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            await UniTask.SwitchToMainThread();
            if (Context.Parameters.Length == 0)
            {
                UnturnedUser uPlayer = (UnturnedUser)Context.Actor;

                uPlayer.Player.Player.life.askHeal(100, true, true);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["heal:success"]);
            }
            else
            {
                string searchTerm = Context.Parameters[0];
                if (!PlayerTool.tryGetSteamPlayer(searchTerm, out SteamPlayer recipient))
                    throw new UserFriendlyException(m_StringLocalizer["general:invalid_player",
                        new { Player = searchTerm }]);

                recipient.player.life.askHeal(100, true, true);
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["heal:success_other", new { Player = recipient.playerID.characterName }]);
            }
        }
    }
}