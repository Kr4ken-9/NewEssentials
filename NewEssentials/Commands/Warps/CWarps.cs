using System;
using System.Text;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.Configuration;
using OpenMod.API.Permissions;
using OpenMod.API.Persistence;
using OpenMod.Unturned.Commands;

namespace NewEssentials.Commands.Warps
{
    [Command("warps")]
    [CommandDescription("List all saved warps")]
    public class CWarps : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IDataStore m_DataStore;
        private readonly IPermissionChecker m_PermissionChecker;
        private const string WarpsKey = "warps";

        public CWarps(IStringLocalizer stringLocalizer,
            IDataStore dataStore,
            IPermissionChecker permissionChecker,
            IServiceProvider serviceProvider) : base(
            serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_DataStore = dataStore;
            m_PermissionChecker = permissionChecker;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 0)
                throw new CommandWrongUsageException(Context);

            var warpsData = await m_DataStore.LoadAsync<WarpsData>(WarpsKey);
            string warps = "none";
            
            if (warpsData.Warps.Count != 0)
            {
                StringBuilder warpsBuilder = new StringBuilder();
                foreach (string warp in warpsData.Warps.Keys)
                {
                    if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, $"warps.{warp}") ==
                        PermissionGrantResult.Grant)
                    {
                        warpsBuilder.Append(warp + ", ");
                    }
                }

                if (warpsBuilder.Length != 0)
                {
                    warpsBuilder.Remove(warpsBuilder.Length - 2, 1);
                    warps = warpsBuilder.ToString();
                }
            }

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["warps:list", new {Warps = warps}]);
        }
    }
}