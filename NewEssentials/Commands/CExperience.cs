using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Commands
{
    [Command("experience")]
    [CommandAlias("exp")]
    [CommandDescription("Give yourself or another player experience (must be positive)")]
    [CommandSyntax("<amount> [player]")]
    public class CExperience : UnturnedCommand
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CExperience(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            string permission = "newess.exp";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length > 2 || Context.Parameters.Length < 1)
                throw new CommandWrongUsageException(Context);

            //TODO: throw UserFriendlyException on bad input
            uint additionalExperience = await Context.Parameters.GetAsync<uint>(0);
            if (Context.Parameters.Length == 1)
            {
                if (Context.Actor.Type == KnownActorTypes.Console)
                    throw new CommandWrongUsageException(Context);

                UnturnedUser uPlayer = (UnturnedUser) Context.Actor;

                await UniTask.SwitchToMainThread();
                uPlayer.Player.skills.askAward(additionalExperience);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["experience:success", new {Experience = additionalExperience.ToString()}]);
            }
            else
            {
                string nestedPermission = "newess.exp.give";
                if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, nestedPermission) == PermissionGrantResult.Deny)
                    throw new NotEnoughPermissionException(Context, nestedPermission);
                
                string searchTerm = Context.Parameters[1];
                if (!PlayerTool.tryGetSteamPlayer(searchTerm, out SteamPlayer player))
                    throw new UserFriendlyException(m_StringLocalizer["general:invalid_player",
                        new {Player = searchTerm}]);

                await UniTask.SwitchToMainThread();
                player.player.skills.askAward(additionalExperience);
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["experience:gave",
                    new {Player = player.playerID.characterName, Experience = additionalExperience.ToString()}]);
            }
        }
    }
}