using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Players;
using NewEssentials.API.User;
using NewEssentials.Configuration;
using NewEssentials.Configuration.Serializable;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Persistence;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Extensions.Games.Abstractions.Items;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.Kits
{
    [Command("kit")]
    [CommandDescription("Spawn a kit for yourself or another user")]
    [CommandSyntax("<name> [user]")]
    // TODO: This permission seems to default to grant, and there is an error with the DefaultGrant attribute
    //[RegisterCommandPermission("cooldowns.exempt", Description = "Bypass any kit-related cooldowns", DefaultGrant = PermissionGrantResult.Deny)]
    public class CKitRoot : UnturnedCommand
    {
        private readonly IDataStore m_DataStore;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IItemSpawner m_ItemSpawner;
        private readonly ICooldownManager m_CooldownManager;
        private readonly IUserParser m_UserParser;
        
        private const string KitsKey = "kits";

        public CKitRoot(IDataStore dataStore,
            IStringLocalizer stringLocalizer,
            IPermissionChecker permissionChecker,
            IItemSpawner itemSpawner,
            ICooldownManager cooldownManager,
            IServiceProvider serviceProvider, IUserParser userParser) : base(serviceProvider)
        {
            m_DataStore = dataStore;
            m_StringLocalizer = stringLocalizer;
            m_PermissionChecker = permissionChecker;
            m_ItemSpawner = itemSpawner;
            m_CooldownManager = cooldownManager;
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1 || Context.Parameters.Length > 2)
                throw new CommandWrongUsageException(Context);

            // If the console is trying to spawn itself a kit, remind to specify a player
            if (Context.Parameters.Length == 1 && Context.Actor.Type == KnownActorTypes.Console)
                throw new CommandWrongUsageException(Context);
            
            string kitName = Context.Parameters[0].ToUpperInvariant();

            KitsData kitsData = await m_DataStore.LoadAsync<KitsData>(KitsKey);

            if (!kitsData.ContainsKey(kitName))
                throw new UserFriendlyException(m_StringLocalizer["kits:spawn:none", new {Kit = kitName}]);

            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, $"kits.kit.{kitName}") == PermissionGrantResult.Deny)
                throw new UserFriendlyException(m_StringLocalizer["kits:spawn:denied", new {Kit = kitName}]);
            
            if (Context.Parameters.Length == 2 &&
                await m_PermissionChecker.CheckPermissionAsync(Context.Actor, $"kits.kit.give.{kitName}") == PermissionGrantResult.Deny)
                throw new UserFriendlyException(m_StringLocalizer["kits:give:deny", new {Kit = kitName}]);

            UnturnedUser recipient;
            if (Context.Parameters.Length == 2)
                recipient = await m_UserParser.ParseUserAsync(Context.Parameters[1]);
            else
                recipient = Context.Actor as UnturnedUser;

            if (recipient == null)
                throw new UserFriendlyException(m_StringLocalizer["commands:failed_player", new {Player = Context.Parameters[1]}]);
            
            SerializableKit kit = kitsData[kitName];

            double? cooldown = await m_CooldownManager.OnCooldownAsync(recipient, "kits", kitName, kit.Cooldown);
            if (cooldown.HasValue)
                throw new UserFriendlyException(m_StringLocalizer["kits:spawn:cooldown", new {Kit = kitName, Time = cooldown}]);
            
            foreach (var item in kit.SerializableItems)
            {
                await m_ItemSpawner.GiveItemAsync(
                    recipient.Player.Inventory,
                    item.ID,
                    new SerializableItemState(item.Quality, item.Durability, item.Amount, item.State));
            }

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["kits:spawn:success",
                new {Kit = kitName, Player = recipient.DisplayName}]);
        }
    }
}
