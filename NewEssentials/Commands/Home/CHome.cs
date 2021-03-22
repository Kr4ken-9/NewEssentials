using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Players;
using NewEssentials.Extensions;
using NewEssentials.Models;
using NewEssentials.Options;
using OpenMod.API.Commands;
using OpenMod.API.Plugins;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;
using System.Collections.Generic;

namespace NewEssentials.Commands.Home
{
    [Command("home")]
    [CommandDescription("Teleport to your bed or one set with /home set")]
    [CommandSyntax("<name>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CHome : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserDataStore m_UserDataStore;
        private readonly IConfiguration m_Configuration;
        private readonly IPluginAccessor<NewEssentials> m_PluginAccessor;
        private readonly ITeleportService m_TeleportService;

        public CHome(IStringLocalizer stringLocalizer,
            IUserDataStore userDataStore,
            IConfiguration configuration,
            IPluginAccessor<NewEssentials> pluginAccessor,
            ITeleportService teleportService,
            IServiceProvider serviceProvider) :
            base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserDataStore = userDataStore;
            m_Configuration = configuration;
            m_PluginAccessor = pluginAccessor;
            m_TeleportService = teleportService;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser)Context.Actor;
            bool bed = Context.Parameters.Length == 0;
            string homeName = Context.Parameters.Length == 1 ? Context.Parameters[0] : "";
            SerializableVector3 home = null;

            // If user is teleporting to a home, query the datastore for the Vector3 location
            if (!bed)
            {
                UserData userData = await m_UserDataStore.GetUserDataAsync(uPlayer.Id, uPlayer.Type);
                if (!userData.Data.ContainsKey("homes"))
                    throw new UserFriendlyException(m_StringLocalizer["home:no_home"]);

                var homes = (Dictionary<object, object>)userData.Data["homes"];

                if (!homes.ContainsKey(homeName))
                    throw new UserFriendlyException(m_StringLocalizer["home:invalid_home", new { Home = homeName }]);

                home = SerializableVector3.Deserialize(homes[homeName]);
                if (home == null)
                {
                    throw new UserFriendlyException(m_StringLocalizer["home:invalid_home", new { Home = homeName }]);
                }
            }

            // Here we will delay the teleportation whether it be to a bed or home
            int delay = m_Configuration.GetValue<int>("teleportation:delay");
            bool cancelOnMove = m_Configuration.GetValue<bool>("teleportation:cancelOnMove");
            bool cancelOnDamage = m_Configuration.GetValue<bool>("teleportation:cancelOnDamage");

            await uPlayer.PrintMessageAsync(m_StringLocalizer["home:success", new { Home = homeName, Time = delay }]);

            bool success = await m_TeleportService.TeleportAsync(uPlayer, new TeleportOptions(m_PluginAccessor.Instance, delay, cancelOnMove, cancelOnDamage));

            if (!success)
                throw new UserFriendlyException(m_StringLocalizer["teleport:canceled"]);

            // Bed-specific teleportation
            if (bed)
            {
                await UniTask.SwitchToMainThread();
                if (!uPlayer.Player.Player.teleportToBed())
                    throw new UserFriendlyException(m_StringLocalizer["home:no_bed"]);

                return;
            }

            if (!await uPlayer.Player.Player.TeleportToLocationAsync(home.ToUnityVector3()))
                throw new UserFriendlyException(m_StringLocalizer["home:failure", new { Home = homeName }]);
        }
    }
}