using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.MaxSkills
{
    [Command("kunii")]
    [CommandDescription("Grants yourself max skills")]
    [CommandParent(typeof(CMaxSkillsRoot))]
    [CommandActor(typeof(UnturnedUser))]
    public class CMaxSkillsKunii : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CMaxSkillsKunii(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        { 
            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            await uPlayer.Player.Player.skills.MaxAllSkillsAsync(true);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["maxskills:kunii"]);
        }
    }
}