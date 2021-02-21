using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Models;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Persistence;
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
        private const string KitsKey = "kits";

        public CKitCreate(IDataStore dataStore, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_DataStore = dataStore;
            m_StringLocalizer = stringLocalizer;
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
            
            // IInventoryPage separates items into pages (basically categories) including equipped items
            // IInventoryItem is a wrapper around items for manipulating inventory
            // e.g dropping/destroying the item. Also provides access to ItemJar
            // We are not interacting with either by using SelectMany and Select. This is here for me to keep track
            var items = unturnedUser.Player.Inventory.SelectMany(page => page.Items.Select(invItem => invItem.Item));
            if (!items.Any())
                throw new UserFriendlyException(m_StringLocalizer["kits:create:none"]);
        }
    }
}