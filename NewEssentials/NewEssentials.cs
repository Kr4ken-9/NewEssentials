using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Chat;
using NewEssentials.API.Players;
using OpenMod.API.Permissions;
using OpenMod.API.Persistence;
using OpenMod.API.Plugins;
using OpenMod.API.Users;
using OpenMod.Core.Ioc;
using OpenMod.Unturned.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewEssentials.Configuration;
using NewEssentials.Configuration.Serializable;
using NewEssentials.Movement;

[assembly: PluginMetadata("NewEssentials", Author = "Kr4ken", DisplayName = "New Essentials")]

namespace NewEssentials
{
    public class NewEssentials : OpenModUnturnedPlugin
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfiguration m_Configuration;
        private readonly IUserDataStore m_UserDataStore;
        private readonly IDataStore m_DataStore;
        private readonly ITeleportRequestManager m_TpaRequestManager;
        private readonly IBroadcastingService m_BroadcastingService;
        private readonly IPermissionRegistry m_PermissionRegistry;

        private const string WarpsKey = "warps";
        private const string KitsKey = "kits";

        public NewEssentials(IStringLocalizer stringLocalizer,
            IConfiguration configuration,
            IUserDataStore userDataStore,
            ITeleportRequestManager tpaRequestManager,
            IDataStore dataStore,
            IBroadcastingService broadcastingService,
            IPermissionRegistry permissionRegistry,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
            m_UserDataStore = userDataStore;
            m_DataStore = dataStore;
            m_TpaRequestManager = tpaRequestManager;
            m_BroadcastingService = broadcastingService;
            m_PermissionRegistry = permissionRegistry;
        }

        private void RegisterPermissions()
        {
            m_PermissionRegistry.RegisterPermission(this, "afkchecker.exempt", "Don't get kicked if you go afk", PermissionGrantResult.Deny);

            // Registering the following permissions without attributes because my MSBuild is fucked or something
            m_PermissionRegistry.RegisterPermission(this, "commands.experience.give", "Give experience to players", PermissionGrantResult.Deny);
            m_PermissionRegistry.RegisterPermission(this, "commands.reputation.give", "Give reputation to players", PermissionGrantResult.Deny);

            // Create permissions for allowing between 1-10 homes
            for (byte b = 1; b < 11; b++)
                m_PermissionRegistry.RegisterPermission(this, $"commands.home.set.{b}", "Allow user to have {b} homes", PermissionGrantResult.Deny);

            m_PermissionRegistry.RegisterPermission(this, "commands.home.set.infinite", "Allow user to have infinite homes", PermissionGrantResult.Deny);
            m_PermissionRegistry.RegisterPermission(this, "commands.homes.others", "Allow user to list another user's homes", PermissionGrantResult.Deny);
            m_PermissionRegistry.RegisterPermission(this, "commands.homes.delete.others", "Allow user to delete another user's homes", PermissionGrantResult.Deny);
            
            m_PermissionRegistry.RegisterPermission(this, "co");

            m_PermissionRegistry.RegisterPermission(this, "warps.cooldowns.exempt", "Bypass any warps-related cooldowns", PermissionGrantResult.Deny);
            m_PermissionRegistry.RegisterPermission(this, "kits.cooldowns.exempt", "Bypass any kits-related cooldowns", PermissionGrantResult.Deny);
            
            m_PermissionRegistry.RegisterPermission(this, "keepskills", "Keep skills no matter the server configuration", PermissionGrantResult.Deny);
        }

        private async Task RegisterDataStores()
        {
            if (!await m_DataStore.ExistsAsync(WarpsKey))
            {
                await m_DataStore.SaveAsync(WarpsKey, new WarpsData
                {
                    Warps = new Dictionary<string, SerializableWarp>()
                });
            }
            else
            {
                foreach (var warpPair in (await m_DataStore.LoadAsync<WarpsData>(WarpsKey)).Warps)
                    m_PermissionRegistry.RegisterPermission(this, $"warps.{warpPair.Key}", $"Permission to warp to {warpPair.Key}", PermissionGrantResult.Deny);
            }

            if (!await m_DataStore.ExistsAsync(KitsKey))
            {
                await m_DataStore.SaveAsync(KitsKey, new KitsData
                {
                    Kits = new Dictionary<string, SerializableKit>()
                });
            }
            else
            {
                foreach (var kitPair in (await m_DataStore.LoadAsync<KitsData>(KitsKey)).Kits)
                {
                    // Outdated permissions but left for compatibility
                    m_PermissionRegistry.RegisterPermission(this, $"kits.kit.{kitPair.Key}", $"Migrate to kits.{kitPair.Key} when possible please", PermissionGrantResult.Deny);
                    m_PermissionRegistry.RegisterPermission(this, $"kits.kit.give.{kitPair.Key}", $"Migrate to kits.give.{kitPair.Key} when possible please", PermissionGrantResult.Deny);

                    // Updated permissions for consistency
                    m_PermissionRegistry.RegisterPermission(this, $"kits.{kitPair.Key}", $"Permission to spawn {kitPair.Key} kit", PermissionGrantResult.Deny);
                    m_PermissionRegistry.RegisterPermission(this, $"kits.give.{kitPair.Key}", $"Permission to give others {kitPair.Key} kit", PermissionGrantResult.Deny);
                }
            }
        }

        protected override async UniTask OnLoadAsync()
        {
            await UniTask.SwitchToThreadPool();

            RegisterPermissions();

            await RegisterDataStores();

            m_TpaRequestManager.SetLocalizer(m_StringLocalizer);

            // https://github.com/aspnet/Configuration/issues/451
            m_BroadcastingService.Configure(m_Configuration.GetValue<ushort>("broadcasting:effectId"),
                 m_Configuration.GetSection("broadcasting:repeatingMessages").Get<string[]>(),
                 m_Configuration.GetValue<int>("broadcasting:repeatingInterval"),
                 m_Configuration.GetValue<int>("broadcasting:repeatingDuration"));

            ActivatorUtilitiesEx.CreateInstance<AfkChecker>(LifetimeScope);
        }

        public void RegisterNewKitPermission(string kitName)
        {
            m_PermissionRegistry.RegisterPermission(this, $"kits.kit.{kitName}");
            m_PermissionRegistry.RegisterPermission(this, $"kits.kit.give.{kitName}");

            m_PermissionRegistry.RegisterPermission(this, $"kits.{kitName}");
            m_PermissionRegistry.RegisterPermission(this, $"kits.give.{kitName}");
        }

        public void RegisterNewWarpPermission(string warpName) => m_PermissionRegistry.RegisterPermission(this,
            $"warps.{warpName}", $"Permission to warp to {warpName}", PermissionGrantResult.Deny);
    }
}