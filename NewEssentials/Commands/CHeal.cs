using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

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

                var life = uPlayer.Player.Player.life;

                life.askHeal(100, true, true);
                life.serverModifyFood(100);
                life.serverModifyWater(100);
                life.serverModifyStamina(100);
                life.serverModifyVirus(100);

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