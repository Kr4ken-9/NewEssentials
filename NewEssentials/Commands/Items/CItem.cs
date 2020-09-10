using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using NewEssentials.Helpers;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

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

        public CItem(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider, IConfiguration configuration) :
            base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
        }

        protected override async UniTask OnExecuteAsync()
        {
            // User either didn't provide an item or provided too much information
            if (Context.Parameters.Length < 1 || Context.Parameters.Length > 2)
                throw new CommandWrongUsageException(m_StringLocalizer["item:syntax"]);

            string rawInput = await Context.Parameters.GetAsync<string>(0);

            if (!UnturnedAssetHelper.GetItem(rawInput, out ItemAsset itemAsset))
                throw new CommandWrongUsageException(m_StringLocalizer["item:invalid", new { Item = rawInput }]);

            byte amount = Context.Parameters.Length == 2 ? await Context.Parameters.GetAsync<byte>(1) : (byte)1;
            if (!m_Configuration.GetItemAmount(amount, out amount))
                throw new UserFriendlyException(m_StringLocalizer["items:too_much", new { UpperLimit = amount }]);

            UnturnedUser uPlayer = (UnturnedUser)Context.Actor;
            Item item = new Item(itemAsset.id, EItemOrigin.ADMIN);

            await UniTask.SwitchToMainThread();
            for (byte b = 0; b < amount; b++)
                uPlayer.Player.Player.inventory.forceAddItem(item, true);

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["item:success", new { Amount = amount, Item = itemAsset.itemName }]);
        }
    }
}