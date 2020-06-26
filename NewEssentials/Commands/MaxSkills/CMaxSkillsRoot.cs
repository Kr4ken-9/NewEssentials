using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.MaxSkills
{
    [UsedImplicitly]
    [Command("maxskills")]
    [CommandDescription("Grants maxskills")]
    [CommandSyntax("<none/player/all/kunii>")]
    public class CMaxSkillsRoot : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CMaxSkillsRoot(IPermissionChecker permissionChecker,
            IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            await UniTask.SwitchToMainThread();
            if (Context.Actor is ConsoleActor)
            {
                throw new CommandWrongUsageException(m_StringLocalizer["commands:playeronly"]);
            }

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, "newess.maxskills") ==
                PermissionGrantResult.Deny)
            {
                throw new NotEnoughPermissionException("newess.maxskills", m_StringLocalizer);
            }

            uPlayer.Player.skills.MaxAllSkills();

            await uPlayer.PrintMessageAsync(m_StringLocalizer["maxskills:granted"]);
        }
    }
}