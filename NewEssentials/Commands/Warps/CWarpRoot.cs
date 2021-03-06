using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Players;
using NewEssentials.Models;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Persistence;
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
        private readonly ICooldownManager m_CooldownManager;
        private const string WarpsKey = "warps";

        public CWarpRoot(IStringLocalizer stringLocalizer,
            IDataStore dataStore,
            IPermissionChecker permissionChecker,
            ICooldownManager cooldownManager,
            IServiceProvider serviceProvider) :
            base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_DataStore = dataStore;
            m_PermissionChecker = permissionChecker;
            m_CooldownManager = cooldownManager;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);

            var warpsData = await m_DataStore.LoadAsync<WarpsData>(WarpsKey);
            string searchTerm = Context.Parameters[0];

            if (!warpsData.Warps.ContainsKey(searchTerm))
                throw new UserFriendlyException(m_StringLocalizer["warps:none", new {Warp = searchTerm}]);

            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, $"warps.{searchTerm}") == PermissionGrantResult.Deny)
                throw new UserFriendlyException(m_StringLocalizer["warps:no_permission", new {Warp = searchTerm}]);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            var warp = warpsData.Warps[searchTerm];

            double? cooldown = await m_CooldownManager.OnCooldownAsync(uPlayer, "warps", searchTerm, warp.Cooldown);
            if (cooldown.HasValue)
                throw new UserFriendlyException(m_StringLocalizer["warps:cooldown", new {Time = cooldown, Warp = searchTerm}]);
            
            await UniTask.SwitchToMainThread();

            uPlayer.Player.Player.teleportToLocation(warp.Location.ToUnityVector3(),
                uPlayer.Player.Player.transform.eulerAngles.y);
            
            await uPlayer.PrintMessageAsync(m_StringLocalizer["warps:success", new {Warp = searchTerm}]);
        }
    }
}