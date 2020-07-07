using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.Repair
{
    [UsedImplicitly]
    [Command("vehicle")]
    [CommandAlias("v")]
    [CommandParent(typeof(CRepairRoot))]
    [CommandDescription("Repair your current vehicle or one you're looking at")]
    [CommandActor(typeof(UnturnedUser))]
    public class CRepairVehicle : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CRepairVehicle(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.repair.vehicle";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);
            
            
        }
    }
}