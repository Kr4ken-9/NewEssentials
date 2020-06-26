using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.MaxSkills
{
    [UsedImplicitly]
    [Command("player")]
    [CommandDescription("Grants a player max skills")]
    [CommandSyntax("[<player>/<players>]")]
    [CommandParent(typeof(CMaxSkillsRoot))]
    public class CMaxSkillsPlayer : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CMaxSkillsPlayer(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            await UniTask.SwitchToMainThread();

            if (Context.Parameters.Length < 1)
            {
                throw new CommandWrongUsageException(Context, m_StringLocalizer);
            }

            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, "newess.maxskills.player") ==
                PermissionGrantResult.Deny)
            {
                throw new NotEnoughPermissionException("newess.maxskills.player", m_StringLocalizer);
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
                
                player.player.skills.MaxAllSkills();
                await Context.Actor.PrintMessageAsync($"{player.playerID.playerName} " + m_StringLocalizer["maxskills:granted_other"]);
            }

            if (playersNotFound != "")
            {
                throw new UserFriendlyException($"Players not found: {playersNotFound}");
            }
        }
    }
}