using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Extensions.Games.Abstractions.Items;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace NewEssentials.Commands.Items
{
    [Command("item")]
    [CommandAlias("i")]
    [CommandDescription("Spawn an item")]
    [CommandSyntax("<id>/<item name> [amount]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CItem : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfiguration m_Configuration;
        private readonly IItemDirectory m_ItemDirectory;

        public CItem(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider, IConfiguration configuration,
            IItemDirectory itemDirectory) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
            m_ItemDirectory = itemDirectory;
        }

        protected override async UniTask OnExecuteAsync()
        {
            // User either didn't provide an item or provided too much information
            if (Context.Parameters.Length < 1 || Context.Parameters.Length > 2)
                throw new CommandWrongUsageException(Context);

            string rawInput = Context.Parameters[0];
            var item = await m_ItemDirectory.FindByNameOrIdAsync(rawInput);

            if (item == null || !ushort.TryParse(item.ItemAssetId, out var id))
                throw new CommandWrongUsageException(m_StringLocalizer["item:invalid", new { Item = rawInput }]);

            var amount = Context.Parameters.Length == 2 ? await Context.Parameters.GetAsync<ushort>(1) : (ushort)1;
            if (!m_Configuration.GetItemAmount(amount, out amount))
                throw new UserFriendlyException(m_StringLocalizer["items:too_much", new { UpperLimit = amount }]);

            UnturnedUser uPlayer = (UnturnedUser)Context.Actor;
            Item uItem = new(id, EItemOrigin.ADMIN);

            await UniTask.SwitchToMainThread();
            for (ushort u = 0; u < amount; u++)
                uPlayer.Player.Player.inventory.forceAddItem(uItem, true);

            await PrintAsync(m_StringLocalizer["item:success", new { Amount = amount, Item = item.ItemName, ID = item.ItemAssetId }]);
        }
    }
}