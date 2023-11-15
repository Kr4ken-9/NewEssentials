using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using NewEssentials.API.User;
using OpenMod.API.Commands;

namespace NewEssentials.Commands
{
    [Command("heal")]
    [CommandDescription("Heal yourself or another player")]
    [CommandSyntax("[player]")]
    public class CHeal : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserParser m_UserParser;

        public CHeal(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider, IUserParser userParser) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            await UniTask.SwitchToMainThread();

            void Heal(PlayerLife life)
            {
                life.askHeal(100, true, true);
                life.serverModifyFood(100);
                life.serverModifyWater(100);
                life.serverModifyStamina(100);
                life.serverModifyVirus(100);
            }

            if (Context.Parameters.Length == 0)
            {
                var player = (UnturnedUser)Context.Actor;

                Heal(player.Player.Player.life);

                await player.PrintMessageAsync(m_StringLocalizer["heal:success"]);
            }
            else
            {
                var player = await m_UserParser.ParseUserAsync(Context.Parameters[0]);
                if (player == null)
                    throw new UserFriendlyException(m_StringLocalizer["commands:failed_player"]);
                Heal(player.Player.Player.life);

                await Context.Actor.PrintMessageAsync(m_StringLocalizer["heal:success_other", new { Player = player.DisplayName }]);
            }
        }
    }
}