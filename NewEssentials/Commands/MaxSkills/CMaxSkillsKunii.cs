using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.MaxSkills
{
    [UsedImplicitly]
    [Command("kunii")]
    [CommandDescription("Grants yourself max skills")]
    [CommandParent(typeof(CMaxSkillsRoot))]
    [CommandActor(typeof(UnturnedUser))]
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
            string permission = "newess.maxskills.kunii";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
            {
                throw new NotEnoughPermissionException(Context, permission);
            }

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            await uPlayer.Player.skills.MaxAllSkills(true);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["maxskills:kunii"]);
        }
    }
}