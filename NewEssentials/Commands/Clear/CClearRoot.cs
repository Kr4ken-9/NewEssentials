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

        protected override async UniTask OnExecuteAsync()
        {
            string permission = "newess.clear";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            throw new CommandWrongUsageException(Context);
        }
    }
}