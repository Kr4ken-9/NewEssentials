using System;
using System.Text;
using Cysharp.Threading.Tasks;
using NewEssentials.API.User;
using NewEssentials.System;
using OpenMod.API.Commands;
using OpenMod.API.Localization;
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
        private readonly IOpenModStringLocalizer m_StringLocalizer;
        private readonly IUserParser m_UserParser;

        public CMaxSkillsPlayer(IOpenModStringLocalizer stringLocalizer, IServiceProvider serviceProvider, IUserParser userParser) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1)
            {
                throw new CommandWrongUsageException(Context, m_StringLocalizer);
            }

            await UniTask.SwitchToMainThread();
            
            StringBuilder playersNotFound = new StringBuilder();
            ReferenceBoolean b = new ReferenceBoolean();
            for (int i = 0; i < Context.Parameters.Length; i++)
            {
                var user = await m_UserParser.TryParseUserAsync(Context.Parameters[i], b);
                if (!b)
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
