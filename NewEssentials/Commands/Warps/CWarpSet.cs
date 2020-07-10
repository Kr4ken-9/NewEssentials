using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.Models;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Persistence;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.Warps
{
    [Command("set")]
    [CommandParent(typeof(CWarpRoot))]
    [CommandDescription("Save a warp at your location")]
    [CommandSyntax("<name>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CWarpSet : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IDataStore m_DataStore;
        private const string WarpsKey = "warps";

        public CWarpSet(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer,
            IDataStore dataStore, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
            m_DataStore = dataStore;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.warp.set";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);

            var warpData = await m_DataStore.LoadAsync<WarpsData>(WarpsKey);
            string newWarpName = Context.Parameters[0];

            if (warpData.Warps.ContainsKey(newWarpName))
                throw new UserFriendlyException(m_StringLocalizer["warps:set:exists", new {Warp = newWarpName}]);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            var newWarpLocation = uPlayer.Player.transform.position.ToSerializableVector3();
            
            warpData.Warps.Add(newWarpName, newWarpLocation);
            await m_DataStore.SaveAsync(WarpsKey, warpData);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["warps:set:success", new {Warp = newWarpName}]);
        }
    }
}