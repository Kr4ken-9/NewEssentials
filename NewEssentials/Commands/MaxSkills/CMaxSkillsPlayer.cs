using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using SDG.Unturned;

namespace NewEssentials.Commands.MaxSkills
{
    [Command("player")]
    [CommandDescription("Grants a player max skills")]
    [CommandSyntax("[<player>/<players>]")]
    [CommandParent(typeof(CMaxSkillsRoot))]
    public class CMaxSkillsPlayer : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CMaxSkillsPlayer(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1)
            {
                throw new CommandWrongUsageException(Context, m_StringLocalizer);
            }

            string playersNotFound = "";
            for (int i = 0; i < Context.Parameters.Length; i++)
            {
                string userInput = await Context.Parameters.GetAsync<string>(i);
                if (!PlayerTool.tryGetSteamPlayer(userInput, out SteamPlayer player))
                {
                    playersNotFound += $"{userInput}, ";
                    continue;
                }
                
                await player.player.skills.MaxAllSkillsAsync();
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["maxskills:granted_other", new {Player = player.playerID.playerName}]);
            }

            if (playersNotFound != "")
            {
                throw new UserFriendlyException(m_StringLocalizer["commands:failed_players", new {Players = playersNotFound}]);
            }
        }
    }
}