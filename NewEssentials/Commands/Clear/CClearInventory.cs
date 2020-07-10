using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Core.Users;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.Clear
{
    [Command("inventory")]
    [CommandParent(typeof(CClearRoot))]
    [CommandDescription("Clears inventory of all items")]
    public class CClearInventory : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CClearInventory(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.clear.inventory";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            if (Context.Parameters.Length == 0)
            {
                //TODO: Use a more descriptive error message
                if (Context.Actor.Type == KnownActorTypes.Console)
                    throw new CommandWrongUsageException(Context);
                
                UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
                
                await UniTask.SwitchToMainThread();
                uPlayer.Player.ClearInventory();
                await uPlayer.PrintMessageAsync(m_StringLocalizer["clear:inventory"]);
            }
            else
            {
                string searchTerm = Context.Parameters[0];
                if (!PlayerTool.tryGetSteamPlayer(searchTerm, out SteamPlayer recipient))
                    throw new UserFriendlyException(m_StringLocalizer["general:invalid_player", new {Player = searchTerm}]);

                await UniTask.SwitchToMainThread();
                recipient.player.ClearInventory();
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["clear:inventory_other",
                    new {Player = recipient.playerID.characterName}]);
            }
        }
    }
}