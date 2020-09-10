using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.API.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Commands.Clear
{
    [Command("inventory")]
    [CommandParent(typeof(CClearRoot))]
    [CommandDescription("Clears inventory of all items")]
    public class CClearInventory : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CClearInventory(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            if (Context.Parameters.Length == 0)
            {
                //TODO: Use a more descriptive error message
                if (Context.Actor.Type == KnownActorTypes.Console)
                    throw new CommandWrongUsageException(Context);
                
                UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
                
                await UniTask.SwitchToMainThread();
                uPlayer.Player.Player.ClearInventory();
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