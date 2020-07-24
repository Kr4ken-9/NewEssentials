using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Chat;
using NewEssentials.API.Players;
using NewEssentials.Extensions;
using NewEssentials.Models;
using OpenMod.API.Permissions;
using OpenMod.API.Persistence;
using OpenMod.API.Plugins;
using OpenMod.API.Users;
using OpenMod.Unturned.Plugins;
using SDG.Unturned;
using Steamworks;

[assembly: PluginMetadata("NewEssentials", Author="Kr4ken", DisplayName="New Essentials")]

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
        private readonly IAfkChecker m_AfkChecker;
        private readonly IPermissionRegistry m_PermissionRegistry;

        private const string WarpsKey = "warps";

        public NewEssentials(IStringLocalizer stringLocalizer,
            IConfiguration configuration,
            IUserDataStore userDataStore, 
            ITeleportRequestManager tpaRequestManager,
            IDataStore dataStore, 
            IBroadcastingService broadcastingService,
            IAfkChecker afkChecker,
            IPermissionRegistry permissionRegistry,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
            m_UserDataStore = userDataStore;
            m_DataStore = dataStore;
            m_TpaRequestManager = tpaRequestManager;
            m_BroadcastingService = broadcastingService;
            m_AfkChecker = afkChecker;
            m_PermissionRegistry = permissionRegistry;
        }

        protected override async UniTask OnLoadAsync()
        {
            await UniTask.SwitchToThreadPool();

            m_PermissionRegistry.RegisterPermission(this, "commands.experience.give", "Give experience to players");
            m_PermissionRegistry.RegisterPermission(this, "commands.reputation.give", "Give reputation to players");
            m_PermissionRegistry.RegisterPermission(this, "afkchecker.exempt", "Don't get kicked if you go afk");

            if (!await m_DataStore.ExistsAsync(WarpsKey))
            {
                await m_DataStore.SaveAsync(WarpsKey, new WarpsData
                {
                    Warps = new Dictionary<string, SerializableVector3>()
                });
            }

            m_TpaRequestManager.SetLocalizer(m_StringLocalizer);
            
            // https://github.com/aspnet/Configuration/issues/451
           m_BroadcastingService.Configure(m_Configuration.GetValue<ushort>("broadcasting:effectId"),
                m_Configuration.GetSection("broadcasting:repeatingMessages").Get<string[]>(),
                m_Configuration.GetValue<int>("broadcasting:repeatingInterval"),
                m_Configuration.GetValue<int>("broadcasting:repeatingDuration"));

           m_AfkChecker.Configure(m_Configuration.GetValue<int>("afkchecker:timeout"),
               m_Configuration.GetValue<bool>("afkchecker:announceAFK"),
               m_Configuration.GetValue<bool>("afkchecker:kickAFK"), m_StringLocalizer);
           
            PlayerLife.onPlayerDied += SaveDeathLocation;
        }

        protected override async UniTask OnUnloadAsync()
        {
            PlayerLife.onPlayerDied -= SaveDeathLocation;
        }

        private async void SaveDeathLocation(PlayerLife sender, EDeathCause cause, ELimb limb, CSteamID instigator)
        {
            var userData = await m_UserDataStore.GetUserDataAsync(sender.player.channel.owner.playerID.steamID.ToString(), "player");
            userData.Data["deathLocation"] = sender.transform.position.ToSerializableVector3();
            await m_UserDataStore.SaveUserDataAsync(userData);
        }
    }
}