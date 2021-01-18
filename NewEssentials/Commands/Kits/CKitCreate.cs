using System;
using Cysharp.Threading.Tasks;
using OpenMod.API.Persistence;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;

namespace NewEssentials.Commands.Kits
{
    [Command("create")]
    [CommandParent(typeof(CKitRoot))]
    [CommandDescription("Create a kit based on the items in your inventory")]
    [CommandSyntax("<name> [cooldown]")]
    public class CKitCreate : UnturnedCommand
    {
        private readonly IDataStore m_DataStore;

        public CKitCreate(IDataStore dataStore, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_DataStore = dataStore;
        }

        protected override async UniTask OnExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}