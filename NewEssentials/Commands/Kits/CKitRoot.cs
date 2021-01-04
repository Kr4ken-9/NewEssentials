using System;
using Cysharp.Threading.Tasks;
using OpenMod.API.Persistence;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;

namespace NewEssentials.Commands.Kits
{
    [Command("kit")]
    [CommandDescription("Spawn a kit for yourself or another user")]
    [CommandSyntax("<name> [user]")]
    public class CKitRoot : UnturnedCommand
    {
        private readonly IDataStore m_DataStore;
        private const string KitsKey = "kits";

        public CKitRoot(IDataStore dataStore, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_DataStore = dataStore;
        }

        protected override async UniTask OnExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}