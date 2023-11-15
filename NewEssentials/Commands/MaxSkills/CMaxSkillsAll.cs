using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using SDG.Unturned;

namespace NewEssentials.Commands.MaxSkills
{
    [Command("all")]
    [CommandDescription("Grants all players max skills")]
    [CommandParent(typeof(CMaxSkillsRoot))]
    public class CMaxSkillsAll : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CMaxSkillsAll(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            await UniTask.SwitchToMainThread();
            foreach (SteamPlayer player in Provider.clients)
                player.player.skills.ServerUnlockAllSkills();

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["maxskills:granted_all"]);
        }
    }
}