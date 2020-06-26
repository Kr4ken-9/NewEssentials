using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands
{
    [UsedImplicitly]
    [Command("item")]
    [CommandAlias("i")]
    [CommandDescription("Add an item to inventory")]
    [CommandSyntax("[<id>/<item name>] <amount>")]
    public class CItem : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfiguration m_Configuration;
        
        public CItem(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Actor is ConsoleActor)
                throw new CommandWrongUsageException(m_StringLocalizer["commands:playeronly"]);

            // User either didn't provide an item or provided too much information
            if (Context.Parameters.Length < 1 || Context.Parameters.Length > 2)
                throw new CommandWrongUsageException(m_StringLocalizer["item:syntax"]);

            string permission = "newess.item";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            string rawInput = await Context.Parameters.GetAsync<string>(0);
            
            if (string.IsNullOrEmpty(rawInput.Trim()))
                throw new CommandWrongUsageException($"{rawInput} " + m_StringLocalizer["item:invalid"]);

            if (!Utilities.GetItem(rawInput, out ItemAsset itemAsset))
                throw new CommandWrongUsageException($"{rawInput} " + m_StringLocalizer["item:invalid"]);

            byte amount = 1;

            if (Context.Parameters.Length == 2)
                amount = await Context.Parameters.GetAsync<byte>(1);

            if (m_Configuration.GetValue<bool>("items:enableamountlimit"))
            {
                byte maxAmount = m_Configuration.GetValue<byte>("items:maxspawnamount");
                if (amount > maxAmount)
                {
                    if (m_Configuration.GetValue<bool>("items:silentamountlimit"))
                        amount = maxAmount;
                    else
                        throw new UserFriendlyException(m_StringLocalizer["items:too_much"] + maxAmount);
                }
            }

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            Item item = new Item(itemAsset.id, EItemOrigin.ADMIN);

            await UniTask.SwitchToMainThread();
            for (byte b = 0; b < amount; b++)
                uPlayer.Player.inventory.forceAddItem(item, true);

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["item:success"] + $"{amount} {itemAsset.itemName}");
        }
    }
}