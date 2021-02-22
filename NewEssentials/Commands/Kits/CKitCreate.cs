using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using NewEssentials.Models;
using OpenMod.API.Commands;
using OpenMod.API.Persistence;
using OpenMod.API.Plugins;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.Kits
{
    [Command("create")]
    [CommandParent(typeof(CKitRoot))]
    [CommandDescription("Create a kit based on the items in your inventory")]
    [CommandSyntax("<name> [cooldown]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CKitCreate : UnturnedCommand
    {
        private readonly IDataStore m_DataStore;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IPluginAccessor<NewEssentials> m_PluginAccessor;
        private const string KitsKey = "kits";

        public CKitCreate(IDataStore dataStore,
            IStringLocalizer stringLocalizer,
            IPluginAccessor<NewEssentials> pluginAccessor,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_DataStore = dataStore;
            m_StringLocalizer = stringLocalizer;
            m_PluginAccessor = pluginAccessor;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1 || Context.Parameters.Length > 2)
                throw new CommandWrongUsageException(Context);

            string kitName = Context.Parameters[0].ToUpperInvariant();
            int kitCooldown = Context.Parameters.Length == 2 ? await Context.Parameters.GetAsync<int>(1) : 0;
            KitsData kitsData = await m_DataStore.LoadAsync<KitsData>(KitsKey);

            if (kitsData.Kits.ContainsKey(kitName))
                throw new UserFriendlyException(m_StringLocalizer["kits:create:exists", new {Kit = kitName}]);

            var unturnedUser = Context.Actor as UnturnedUser;
            
            // Player clothes are not included in inventory, so we need to collect them manually
            SerializableItem[] serializableClothes = unturnedUser.Player.Player.clothing.ToSerializableItems();
            
            // IInventoryPage separates items into pages (basically categories) including equipped items
            // IInventoryItem is a wrapper around items for manipulating inventory
            // e.g dropping/destroying the item. Also provides access to ItemJar
            // We are not interacting with either by using SelectMany and Select. This is here for me to keep track
            var items = unturnedUser.Player.Inventory.SelectMany(page => page.Items.Select(invItem => invItem.Item));
            
            // Since we are checking the length and then later enumerating again this will prevent "multiple enumeration"
            // https://www.jetbrains.com/help/rider/PossibleMultipleEnumeration.html
            var serializableItems = new List<SerializableItem>();
            
            // By adding the clothes first, the player will receive them first and equip them
            serializableItems.AddRange(serializableClothes);

            serializableItems.AddRange(items.Select(item => new SerializableItem(
                item.Asset.ItemAssetId,
                item.State.StateData,
                item.State.ItemAmount,
                item.State.ItemDurability,
                item.State.ItemQuality)));

            if (serializableItems.Count == 0)
                throw new UserFriendlyException(m_StringLocalizer["kits:create:none"]);
            
            kitsData.Kits.Add(kitName, new SerializableKit(serializableItems.ToArray(), kitCooldown));
            await m_DataStore.SaveAsync(KitsKey, kitsData);
            m_PluginAccessor.Instance.RegisterNewKitPermission(kitName);

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["kits:create:success", new {Kit = kitName}]);
        }
    }
}