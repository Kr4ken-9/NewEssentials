using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Models;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Persistence;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Permissions;
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
        private readonly IUnturnedUserDirectory m_UserDirectory;
        private const string KitsKey = "kits";

        public CKitRoot(IDataStore dataStore, IStringLocalizer stringLocalizer, IPermissionChecker permissionChecker, IItemSpawner itemSpawner, IUnturnedUserDirectory userDirectory, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_DataStore = dataStore;
            m_StringLocalizer = stringLocalizer;
            m_PermissionChecker = permissionChecker;
            m_ItemSpawner = itemSpawner;
            m_UserDirectory = userDirectory;
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

            if (!kitsData.Kits.ContainsKey(kitName))
                throw new UserFriendlyException(m_StringLocalizer["kits:spawn:none", new {Kit = kitName}]);

            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, $"kits.kit.{kitName}") == PermissionGrantResult.Deny)
                throw new UserFriendlyException(m_StringLocalizer["kits:spawn:deny", new {Kit = kitName}]);
            
            if (Context.Parameters.Length == 2 &&
                await m_PermissionChecker.CheckPermissionAsync(Context.Actor, $"kits.kit.give.{kitName}") == PermissionGrantResult.Deny)
                throw new UserFriendlyException(m_StringLocalizer["kits:give:deny", new {Kit = kitName}]);

            UnturnedUser recipient;
            if (Context.Parameters.Length == 2)
                recipient = m_UserDirectory.FindUser(Context.Parameters[1], UserSearchMode.FindByName);
            else
                recipient = Context.Actor as UnturnedUser;

            if (recipient == null)
                throw new UserFriendlyException(m_StringLocalizer["commands:failed_player", new {Player = Context.Parameters[1]}]);
            
            SerializableKit kit = kitsData.Kits[kitName];
            
            // If there is a non-zero cooldown and the user is not exempt from cooldowns
            // Then we will add a cooldown to them for this specific kit
            // Unless they are already under a cooldown, in which we will deny them the kit
            if (kit.Cooldown != 0 && await CheckPermissionAsync("cooldowns.exempt") == PermissionGrantResult.Deny)
            {
                TimeSpan rightThisVeryInstant = DateTime.Now.TimeOfDay;
                if (!recipient.Session.SessionData.ContainsKey($"kits.cooldowns.{kitName}"))
                {
                    recipient.Session.SessionData.Add($"kits.cooldowns.{kitName}", rightThisVeryInstant);
                }
                else
                {
                    TimeSpan lastKitUse = (TimeSpan) recipient.Session.SessionData[$"kits.cooldowns.{kitName}"];
                    double secondsSinceLastUse = (rightThisVeryInstant - lastKitUse).TotalSeconds;
                    double remainingSeconds = Math.Round(kit.Cooldown - secondsSinceLastUse, 2);

                    if (secondsSinceLastUse < kit.Cooldown)
                    {
                        throw new UserFriendlyException(m_StringLocalizer["kits:spawn:cooldown",
                            new {Time = remainingSeconds, Kit = kitName}]);
                    }

                    recipient.Session.SessionData[$"kits.cooldowns.{kitName}"] = rightThisVeryInstant;
                }
            }
            
            foreach (var item in kit.SerializableItems)
            {
                await m_ItemSpawner.GiveItemAsync(
                    recipient.Player.Inventory,
                    item.ID,
                    new SerializedItemState(item.Quality, item.Durability, item.Amount, item.State));
            }

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["kits:spawn:success",
                new {Kit = kitName, Player = recipient.DisplayName}]);
        }
    }
}