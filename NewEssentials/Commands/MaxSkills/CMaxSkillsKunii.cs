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
    [Command("kunii")]
    [CommandDescription("Grants yourself max skills")]
    [CommandParent(typeof(CMaxSkillsRoot))]
    public class CMaxSkillsKunii : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CMaxSkillsKunii(IPermissionChecker permissionChecker,
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
            
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, "newess.maxskills.kunii") ==
                PermissionGrantResult.Deny)
            {
                throw new NotEnoughPermissionException("newess.maxskills.kunii", m_StringLocalizer);
            }

            uPlayer.Player.skills.MaxAllSkills(true);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["maxskills:kunii"]);
        }
    }
}