using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.Vehicles
{
    [Command("vehicle")]
    [CommandAlias("v")]
    [CommandDescription("Spawn a vehicle")]
    [CommandSyntax("<name>/<id>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CVehicle : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CVehicle(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.vehicle";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);

            string vehicleSearchTerm = Context.Parameters[0];
            if (!Utilities.GetVehicle(vehicleSearchTerm, out VehicleAsset vehicle))
                throw new UserFriendlyException(m_StringLocalizer["vehicle:invalid",
                    new {Vehicle = vehicleSearchTerm}]);

            await UniTask.SwitchToMainThread();
            if (VehicleTool.giveVehicle(((UnturnedUser) Context.Actor).Player, vehicle.id))
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["vehicle:success",
                    new {Vehicle = vehicle.vehicleName}]);
            else
                throw new UserFriendlyException(m_StringLocalizer["vehicle:failure"]);
        }
    }
}