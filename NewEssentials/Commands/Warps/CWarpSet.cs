using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Extensions;
using NewEssentials.Models;
using OpenMod.API.Commands;
using OpenMod.API.Persistence;
using OpenMod.API.Plugins;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace NewEssentials.Commands.Warps
{
    [Command("set")]
    [CommandParent(typeof(CWarpRoot))]
    [CommandDescription("Save a warp at your location")]
    [CommandSyntax("<name> [cooldown]")]
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
            if (Context.Parameters.Length < 1 || Context.Parameters.Length > 2)
                throw new CommandWrongUsageException(Context);

            int cooldown = Context.Parameters.Length == 2 ? await Context.Parameters.GetAsync<int>(1) : 0;

            var warpData = await m_DataStore.LoadAsync<WarpsData>(WarpsKey);
            string newWarpName = Context.Parameters[0];

            if (warpData.Warps.ContainsKey(newWarpName))
                throw new UserFriendlyException(m_StringLocalizer["warps:set:exists", new { Warp = newWarpName }]);

            UnturnedUser uPlayer = (UnturnedUser)Context.Actor;
            var newWarpLocation = uPlayer.Player.Transform.Position.ToSerializableVector();

            warpData.Warps.Add(newWarpName, new SerializableWarp(cooldown, newWarpLocation));
            await m_DataStore.SaveAsync(WarpsKey, warpData);
            m_PluginAccessor.Instance.RegisterNewWarpPermission(newWarpName);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["warps:set:success", new { Warp = newWarpName }]);
        }
    }
}