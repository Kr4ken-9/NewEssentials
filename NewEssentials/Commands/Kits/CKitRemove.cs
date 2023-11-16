using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Configuration;
using OpenMod.API.Commands;
using OpenMod.API.Persistence;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;

namespace NewEssentials.Commands.Kits
{
    [Command("remove")]
    [CommandParent(typeof(CKitRoot))]
    [CommandDescription("Remove a kit by name")]
    [CommandSyntax("<name>")]
    public class CKitRemove : UnturnedCommand
    {
        private readonly IDataStore m_DataStore;
        private readonly IStringLocalizer m_StringLocalizer;
        private const string KitsKey = "kits";

        public CKitRemove(IDataStore dataStore, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_DataStore = dataStore;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);
            
            string kitName = Context.Parameters[0].ToUpperInvariant();
            KitsData kitsData = await m_DataStore.LoadAsync<KitsData>(KitsKey);
            
            if (!kitsData.ContainsKey(kitName))
                throw new UserFriendlyException(m_StringLocalizer["kits:create:exists", new {Kit = kitName}]);

            kitsData.Remove(kitName);
            await m_DataStore.SaveAsync(KitsKey, kitsData);
            await Context.Actor.PrintMessageAsync(m_StringLocalizer["kits:removed", new {Kit = kitName}]);
        }
    }
}