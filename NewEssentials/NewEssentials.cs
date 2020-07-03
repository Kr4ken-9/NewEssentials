using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NewEssentials.API;
using OpenMod.API.Users;
using OpenMod.Core.Plugins;
using OpenMod.Unturned.Plugins;
using SDG.Unturned;
using Steamworks;

[assembly: PluginMetadata("NewEssentials", Author="Kr4ken", DisplayName="New Essentials")]

namespace NewEssentials
{
    [UsedImplicitly]
    public class NewEssentials : OpenModUnturnedPlugin
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ILogger<NewEssentials> m_Logger;
        private readonly IConfiguration m_Configuration;
        private readonly IUserDataStore m_UserDataStore;
        private readonly ITPAManager m_TPAManager;

        public NewEssentials(IStringLocalizer stringLocalizer, ILogger<NewEssentials> logger, IConfiguration configuration, IUserDataStore userDataStore, ITPAManager tpaManager, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_Logger = logger;
            m_Configuration = configuration;
            m_UserDataStore = userDataStore;
            m_TPAManager = tpaManager;
        }

        protected override async UniTask OnLoadAsync()
        {
            await UniTask.SwitchToThreadPool();

            m_TPAManager.SetLocalizer(m_StringLocalizer);
            PlayerLife.onPlayerDied += SaveDeathLocation;
        }

        protected override async UniTask OnUnloadAsync()
        {
            PlayerLife.onPlayerDied -= SaveDeathLocation;
            await Task.Yield();
        }

        private async void SaveDeathLocation(PlayerLife sender, EDeathCause cause, ELimb limb, CSteamID instigator)
        {
            var userData = await m_UserDataStore.GetUserDataAsync(sender.player.channel.owner.playerID.steamID.ToString(), "player");
            userData.Data["deathLocation"] = sender.transform.position.ToSerializableVector3();
            await m_UserDataStore.SaveUserDataAsync(userData);
        }
    }
}