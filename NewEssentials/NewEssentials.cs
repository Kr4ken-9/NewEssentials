using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NewEssentials.API;
using NewEssentials.API.Players;
using NewEssentials.Extensions;
using NewEssentials.Models;
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
        private readonly IUserDataStore m_UserDataStore;
        private readonly IDataStore m_DataStore;
        private readonly ITeleportRequestManager m_TpaRequestManager;

        private const string WarpsKey = "warps";

        public NewEssentials(IStringLocalizer stringLocalizer,
            IUserDataStore userDataStore, 
            ITeleportRequestManager tpaRequestManager,
            IDataStore dataStore, 
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserDataStore = userDataStore;
            m_DataStore = dataStore;
            m_TpaRequestManager = tpaRequestManager;
        }

        protected override async UniTask OnLoadAsync()
        {
            await UniTask.SwitchToThreadPool();

            if (!await m_DataStore.ExistsAsync(WarpsKey))
            {
                await m_DataStore.SaveAsync(WarpsKey, new WarpsData
                {
                    Warps = new Dictionary<string, SerializableVector3>()
                });
            }

            m_TpaRequestManager.SetLocalizer(m_StringLocalizer);
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