using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;

namespace NewEssentials.Commands.Clear
{
    [Command("clear")]
    [CommandDescription("description")]
    [CommandSyntax("<items/vehicles/inventory>")]
    public class CClearRoot : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;

        public CClearRoot(IPermissionChecker permissionChecker, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.clear";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            throw new CommandWrongUsageException(Context);
        }
    }
}