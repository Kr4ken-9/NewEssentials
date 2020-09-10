using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.Helpers;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Commands.Vehicles
{
    [Command("vehicle")]
    [CommandAlias("v")]
    [CommandDescription("Spawn a vehicle")]
    [CommandSyntax("<name>/<id>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CVehicle : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CVehicle(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);

            string vehicleSearchTerm = Context.Parameters[0];
            if (!UnturnedAssetHelper.GetVehicle(vehicleSearchTerm, out VehicleAsset vehicle))
                throw new UserFriendlyException(m_StringLocalizer["vehicle:invalid",
                    new {Vehicle = vehicleSearchTerm}]);

            await UniTask.SwitchToMainThread();
            if (VehicleTool.giveVehicle(((UnturnedUser) Context.Actor).Player.Player, vehicle.id))
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["vehicle:success",
                    new {Vehicle = vehicle.vehicleName}]);
            else
                throw new UserFriendlyException(m_StringLocalizer["vehicle:failure"]);
        }
    }
}