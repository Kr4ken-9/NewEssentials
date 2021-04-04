using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
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

            await UniTask.SwitchToMainThread();
            
            StringBuilder playersNotFound = new StringBuilder();
            for (int i = 0; i < Context.Parameters.Length; i++)
            {
                var user = await Context.Parameters.GetAsync<UnturnedUser>(i);
                if (user == null)
                {
                    playersNotFound.Append($"{Context.Parameters[i]}, ");
                    continue;
                }
                
                user.Player.Player.skills.ServerUnlockAllSkills();
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["maxskills:granted_other", new {Player = user.DisplayName}]);
            }

            if (playersNotFound.Length != 0)
            {
                playersNotFound.Remove(playersNotFound.Length - 2, 2);
                throw new UserFriendlyException(m_StringLocalizer["commands:failed_players", new {Players = playersNotFound.ToString()}]);
            }
        }
    }
}