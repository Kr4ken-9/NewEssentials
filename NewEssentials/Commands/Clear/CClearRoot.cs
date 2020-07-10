using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Commands;

namespace NewEssentials.Commands.Clear
{
    [Command("clear")]
    [CommandDescription("description")]
    [CommandSyntax("<items/vehicles/inventory>")]
    public class CClearRoot : UnturnedCommand
    {
        private readonly IPermissionChecker m_PermissionChecker;

        public CClearRoot(IPermissionChecker permissionChecker, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
        }

        protected override UniTask OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }
    }
}