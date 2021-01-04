using System;
using Cysharp.Threading.Tasks;
using OpenMod.API.Persistence;
using OpenMod.Unturned.Commands;

namespace NewEssentials.Commands.Kits
{
    public class CKits : UnturnedCommand
    {
        private readonly IDataStore m_DataStore;

        public CKits(IDataStore dataStore, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_DataStore = dataStore;
        }

        protected override async UniTask OnExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}