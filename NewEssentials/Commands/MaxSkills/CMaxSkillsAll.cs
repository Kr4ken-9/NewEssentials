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
    [Command("all")]
    [CommandDescription("Grants all players max skills")]
    [CommandParent(typeof(CMaxSkillsRoot))]
    public class CMaxSkillsAll : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CMaxSkillsAll(IPermissionChecker permissionChecker,
            IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.maxskills.all";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
            {
                throw new NotEnoughPermissionException(Context, permission);
            }

            foreach(SteamPlayer player in Provider.clients)
                player.player.skills.MaxAllSkills();

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["maxskills:granted_all"]);
        }
    }
}