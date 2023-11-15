using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Players;
using NewEssentials.Configuration;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Persistence;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.Warps
{
    [Command("warp")]
    [CommandDescription("Warp to a saved warp")]
    [CommandSyntax("<name>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CWarpRoot : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IDataStore m_DataStore;
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IConfiguration m_Configuration;
        private readonly IPluginAccessor<NewEssentials> m_PluginAccessor;
        private readonly ITeleportService m_TeleportService;
        private readonly ICooldownManager m_CooldownManager;
        private const string WarpsKey = "warps";

        public CWarpRoot(IStringLocalizer stringLocalizer,
            IDataStore dataStore,
            IPermissionChecker permissionChecker,
            IConfiguration configuration,
            IPluginAccessor<NewEssentials> pluginAccessor,
            ITeleportService teleportService,
            ICooldownManager cooldownManager,
            IServiceProvider serviceProvider) :
            base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_DataStore = dataStore;
            m_PermissionChecker = permissionChecker;
            m_Configuration = configuration;
            m_PluginAccessor = pluginAccessor;
            m_TeleportService = teleportService;
            m_CooldownManager = cooldownManager;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);

            var warpsData = await m_DataStore.LoadAsync<WarpsData>(WarpsKey);
            string searchTerm = Context.Parameters[0];

            if (!warpsData.ContainsWarp(searchTerm))
                throw new UserFriendlyException(m_StringLocalizer["warps:none", new {Warp = searchTerm}]);

            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, $"warps.{searchTerm}") == PermissionGrantResult.Deny)
                throw new UserFriendlyException(m_StringLocalizer["warps:no_permission", new {Warp = searchTerm}]);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            var warp = warpsData[searchTerm];

            double? cooldown = await m_CooldownManager.OnCooldownAsync(uPlayer, "warps", searchTerm, warp.Cooldown);
            if (cooldown.HasValue)
                throw new UserFriendlyException(m_StringLocalizer["warps:cooldown", new {Time = cooldown, Warp = searchTerm}]);
            
            // Delay warping so that they cannot escape combat
            int delay = m_Configuration.GetValue<int>("teleportation:delay");
            bool cancelOnMove = m_Configuration.GetValue<bool>("teleportation:cancelOnMove");
            bool cancelOnDamage = m_Configuration.GetValue<bool>("teleportation:cancelOnDamage");
            
            // Tell the player of the delay and not to move
            await uPlayer.PrintMessageAsync(m_StringLocalizer["warps:success", new {Warp = searchTerm, Time = delay}]);

            bool success = await m_TeleportService.TeleportAsync(uPlayer, new TeleportOptions(m_PluginAccessor.Instance, delay, cancelOnMove, cancelOnDamage));

            if (!success)
                throw new UserFriendlyException(m_StringLocalizer["teleport:canceled"]);
            
            await UniTask.SwitchToMainThread();

            uPlayer.Player.Player.teleportToLocation(warp.Location.ToUnityVector3(),
                uPlayer.Player.Player.transform.eulerAngles.y);
        }
    }
}