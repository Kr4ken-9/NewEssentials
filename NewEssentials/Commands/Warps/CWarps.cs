using System;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.Models;
using OpenMod.API.Permissions;
using OpenMod.API.Persistence;

namespace NewEssentials.Commands.Warps
{
    [Command("warps")]
    [CommandDescription("List all saved warps")]
    public class CWarps : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IDataStore m_DataStore;
        private const string WarpsKey = "warps";

        public CWarps(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer,
            IDataStore dataStore, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
            m_DataStore = dataStore;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.warp.list";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length > 0)
                throw new CommandWrongUsageException(Context);

            var warpsData = await m_DataStore.LoadAsync<WarpsData>(WarpsKey);
            string warps = "";
            
            StringBuilder warpsBuilder = new StringBuilder();
            if (warpsData.Warps.Count == 0)
                warps = "none";
            else
            {
                foreach (string warp in warpsData.Warps.Keys)
                    warpsBuilder.Append(warp + ", ");

                warpsBuilder.Remove(warpsBuilder.Length - 2, 1);
                warps = warpsBuilder.ToString();
            }

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["warps:list", new {Warps = warps}]);
        }
    }
}