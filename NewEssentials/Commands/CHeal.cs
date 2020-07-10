using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Commands
{
    [Command("heal")]
    [CommandDescription("Heal yourself or another player")]
    [CommandSyntax("[player]")]
    public class CHeal : UnturnedCommand
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CHeal(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            string permission = "newess.heal";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            if (Context.Parameters.Length == 0)
            {
                UnturnedUser uPlayer = (UnturnedUser) Context.Actor;

                await UniTask.SwitchToMainThread();
                uPlayer.Player.life.askHeal(100, true, true);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["heal:success"]);
            }
            else
            {
                string searchTerm = Context.Parameters[0];
                if (!PlayerTool.tryGetSteamPlayer(searchTerm, out SteamPlayer recipient))
                    throw new UserFriendlyException(m_StringLocalizer["general:invalid_player",
                        new {Player = searchTerm}]);

                await UniTask.SwitchToMainThread();
                recipient.player.life.askHeal(100, true, true);
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["heal:success_other", new {Player = recipient.playerID.characterName}]);
            }
        }
    }
}