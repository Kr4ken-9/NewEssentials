using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.MaxSkills
{
    [Command("maxskills")]
    [CommandDescription("Grants maxskills")]
    [CommandSyntax("<none/player/all/kunii>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CMaxSkillsRoot : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CMaxSkillsRoot(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            await uPlayer.Player.Player.skills.MaxAllSkillsAsync();

            await uPlayer.PrintMessageAsync(m_StringLocalizer["maxskills:granted"]);
        }
    }
}