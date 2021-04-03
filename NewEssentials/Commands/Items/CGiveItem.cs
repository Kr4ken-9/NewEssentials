using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Extensions.Games.Abstractions.Items;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Players;
using SDG.Unturned;
using System;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.Items
{
    [Command("giveitem")]
    [CommandAlias("gi")]
    [CommandDescription("Give another player an item")]
    [CommandSyntax("<player> <id>/<item name> [amount]")]
    public class CGiveItem : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfiguration m_Configuration;
        private readonly IItemDirectory m_ItemDirectory;

        public CGiveItem(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider,
            IConfiguration configuration, IItemDirectory itemDirectory) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
            m_ItemDirectory = itemDirectory;
        }

        protected override async UniTask OnExecuteAsync()
        {
            // User either didn't provide an item and player or provided too much information
            if (Context.Parameters.Length < 2 || Context.Parameters.Length > 3)
                throw new CommandWrongUsageException(Context);

            var user = await Context.Parameters.GetAsync<UnturnedUser>(0);

            if (user == null)
                throw new UserFriendlyException(m_StringLocalizer["commands:failed_player", new { Player = Context.Parameters[0] }]);

            string rawItem = Context.Parameters[1];

            var item = await m_ItemDirectory.FindByNameOrIdAsync(rawItem);

            if (item == null)
                throw new CommandWrongUsageException(m_StringLocalizer["item:invalid", new { Item = rawItem }]);

            var amount = Context.Parameters.Length == 3 ? await Context.Parameters.GetAsync<ushort>(2) : (ushort)1;
            if (!m_Configuration.GetItemAmount(amount, out amount))
                throw new UserFriendlyException(m_StringLocalizer["items:too_much", new { UpperLimit = amount }]);

            Item uItem = new(item.ItemAsset.id, EItemOrigin.ADMIN);

            await UniTask.SwitchToMainThread();
            for (ushort u = 0; u < amount; u++)
                user.Player.Player.inventory.forceAddItem(uItem, true);

            await PrintAsync(m_StringLocalizer["item:success", new { Amount = amount, Item = item.ItemName, ID = item.ItemAssetId }]);
        }
    }
}