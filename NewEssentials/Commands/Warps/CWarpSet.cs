using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using NewEssentials.Models;
using OpenMod.API.Commands;
using OpenMod.API.Persistence;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.Warps
{
    [Command("set")]
    [CommandParent(typeof(CWarpRoot))]
    [CommandDescription("Save a warp at your location")]
    [CommandSyntax("<name>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CWarpSet : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IDataStore m_DataStore;
        private readonly IPluginAccessor<NewEssentials> m_PluginAccessor;
        private const string WarpsKey = "warps";

        public CWarpSet(IStringLocalizer stringLocalizer,
            IDataStore dataStore,
            IPluginAccessor<NewEssentials> pluginAccessor,
            IServiceProvider serviceProvider) :
            base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_DataStore = dataStore;
            m_PluginAccessor = pluginAccessor;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);

            var warpData = await m_DataStore.LoadAsync<WarpsData>(WarpsKey);
            string newWarpName = Context.Parameters[0];

            if (warpData.Warps.ContainsKey(newWarpName))
                throw new UserFriendlyException(m_StringLocalizer["warps:set:exists", new {Warp = newWarpName}]);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            var newWarpLocation = uPlayer.Player.Player.transform.position.ToSerializableVector3();
            
            warpData.Warps.Add(newWarpName, newWarpLocation);
            await m_DataStore.SaveAsync(WarpsKey, warpData);
            m_PluginAccessor.Instance.RegisterNewWarpPermission(newWarpName);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["warps:set:success", new {Warp = newWarpName}]);
        }
    }
}