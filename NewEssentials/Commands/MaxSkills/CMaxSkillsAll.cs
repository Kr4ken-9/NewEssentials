using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;
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
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CMaxSkillsAll(IPermissionChecker permissionChecker,
            IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            string permission = "newess.maxskills.all";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
            {
                throw new NotEnoughPermissionException(Context, permission);
            }
            
            foreach(SteamPlayer player in Provider.clients)
                await player.player.skills.MaxAllSkills();

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["maxskills:granted_all"]);
        }
    }
}