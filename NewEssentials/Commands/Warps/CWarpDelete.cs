using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.Configuration;
using OpenMod.API.Commands;
using OpenMod.API.Persistence;
using OpenMod.Unturned.Commands;

namespace NewEssentials.Commands.Warps
{
    [Command("delete")]
    [CommandAlias("del")]
    [CommandParent(typeof(CWarpRoot))]
    [CommandDescription("Delete a saved warp")]
    [CommandSyntax("<name>")]
    public class CWarpDelete : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IDataStore m_DataStore;
        private const string WarpsKey = "warps";

        public CWarpDelete(IStringLocalizer stringLocalizer, IDataStore dataStore, IServiceProvider serviceProvider) :
            base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_DataStore = dataStore;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);

            var warpsData = await m_DataStore.LoadAsync<WarpsData>(WarpsKey);
            string searchTerm = Context.Parameters[0];

            if (!warpsData.Warps.ContainsKey(searchTerm))
                throw new UserFriendlyException(m_StringLocalizer["warps:none", new {Warp = searchTerm}]);

            warpsData.Warps.Remove(searchTerm);
            await m_DataStore.SaveAsync(WarpsKey, warpsData);
            await Context.Actor.PrintMessageAsync(m_StringLocalizer["warps:deleted", new {Warp = searchTerm}]);
        }
    }
}