using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using SDG.Unturned;

namespace NewEssentials.Commands.Items
{
    [Command("giveitem")]
    [CommandAlias("gi")]
    [CommandDescription("Give another player an item")]
    [CommandSyntax("<player> <id>/<item name> [amount]")]
    public class CGiveItem : UnturnedCommand
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfiguration m_Configuration;
        
        public CGiveItem(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
        }

        protected override async UniTask OnExecuteAsync()
        {
            // User either didn't provide an item and player or provided too much information
            if (Context.Parameters.Length < 2 || Context.Parameters.Length > 3)
                throw new CommandWrongUsageException(m_StringLocalizer["item:give_syntax"]);

            string permission = "newess.item.give";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            string rawPlayer = await Context.Parameters.GetAsync<string>(0);

            if (!PlayerTool.tryGetSteamPlayer(rawPlayer, out SteamPlayer player))
                throw new UserFriendlyException(m_StringLocalizer["commands:failed_player", new {Player = rawPlayer}]);

            string rawItem = await Context.Parameters.GetAsync<string>(1);

            if (!Utilities.GetItem(rawItem, out ItemAsset itemAsset))
                throw new CommandWrongUsageException(m_StringLocalizer["item:invalid", new {Item = rawItem}]);

            byte amount = Context.Parameters.Length == 2 ? await Context.Parameters.GetAsync<byte>(1) : (byte)1;
            if (!Utilities.GetItemAmount(amount, m_Configuration, out amount))
                throw new UserFriendlyException(m_StringLocalizer["items:too_much", new {UpperLimit = amount}]);

            Item item = new Item(itemAsset.id, EItemOrigin.ADMIN);
            
            await UniTask.SwitchToMainThread();
            for (byte b = 0; b < amount; b++)
                player.player.inventory.forceAddItem(item, true);

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["item:success", new {Amount = amount, Item = itemAsset.itemName}]);
        }
    }
}