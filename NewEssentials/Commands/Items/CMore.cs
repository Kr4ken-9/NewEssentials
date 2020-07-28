using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Commands.Items
{
    [Command("more")]
    [CommandDescription("Spawn more of the item you're holding")]
    [CommandSyntax("[amount]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CMore : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CMore(IStringLocalizer stringLocalizer,  IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            int amount = 10;
            if (Context.Parameters.Length == 1)
                amount = await Context.Parameters.GetAsync<int>(0);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            PlayerEquipment equipment = uPlayer.Player.equipment;

            if (equipment.itemID == 0)
                throw new UserFriendlyException(m_StringLocalizer["more:none"]);
            
            Item item = new Item(equipment.itemID, EItemOrigin.ADMIN);

            await UniTask.SwitchToMainThread();
            for(int i = 0; i < amount; i++)
                uPlayer.Player.inventory.forceAddItem(item, true);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["more:success",
                new {Amount = amount, Item = equipment.asset.itemName}]);
        }
    }
}