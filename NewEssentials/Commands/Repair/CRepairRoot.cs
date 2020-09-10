using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Commands.Repair
{
    [Command("repair")]
    [CommandDescription("Repair items in your inventory or a vehicle")]
    [CommandSyntax("[vehicle]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CRepairRoot : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CRepairRoot(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            PlayerInventory inventory = uPlayer.Player.Player.inventory;

            await UniTask.SwitchToMainThread();
            foreach (SDG.Unturned.Items itemContainer in inventory.items)
            {
                if (itemContainer == null)
                    continue;

                foreach (ItemJar itemJar in itemContainer.items)
                {
                    Item item = itemJar.item;
                    
                    if (item.quality != 100)
                        inventory.sendUpdateQuality(itemContainer.page, itemJar.x, itemJar.y, 100);

                    if (!HasDurableBarrel(item, out ushort barrelID))
                        continue;
                    
                    if (barrelID == 0)
                        continue;

                    if (item.state[16] == 100)
                        continue;
                    
                    item.state[16] = 100;
                    inventory.sendUpdateInvState(itemContainer.page, itemJar.x, itemJar.y, item.state);
                }
            }

            await uPlayer.PrintMessageAsync(m_StringLocalizer["repair:inventory"]);
        }

        private bool HasDurableBarrel(Item item, out ushort barrelID)
        {
            barrelID = 0;
            var itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, item.id);

            if (itemAsset == null || itemAsset.type != EItemType.GUN || item.state == null || item.state.Length != 18)
                return false;

            var itemGunAsset = (ItemGunAsset) itemAsset;
            
            if (itemGunAsset.hasBarrel)
                barrelID = BitConverter.ToUInt16(item.state, 6);
            
            return itemGunAsset.hasBarrel;
        }
    }
}