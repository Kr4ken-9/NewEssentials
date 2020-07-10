using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.Vehicles
{
    [Command("givevehicle")]
    [CommandAlias("gv")]
    [CommandDescription("Spawn a vehicle for another player")]
    [CommandSyntax("<player> <name>/<id>")]
    public class CGiveVehicle : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CGiveVehicle(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.vehicle.give";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length != 2)
                throw new CommandWrongUsageException(Context);

            string playerSearchTerm = Context.Parameters[0];
            if (!PlayerTool.tryGetSteamPlayer(playerSearchTerm, out SteamPlayer recipient))
                throw new UserFriendlyException(m_StringLocalizer["vehicle:invalid_player", new {Player = playerSearchTerm}]);

            string vehicleSearchTerm = Context.Parameters[1];
            if (!Utilities.GetVehicle(vehicleSearchTerm, out VehicleAsset vehicle))
                throw new UserFriendlyException(m_StringLocalizer["vehicle:invalid",
                    new {Vehicle = vehicleSearchTerm}]);

            await UniTask.SwitchToMainThread();
            if (VehicleTool.giveVehicle(recipient.player, vehicle.id))
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["vehicle:success_given",
                    new {Vehicle = vehicle.vehicleName, Player = recipient.playerID.characterName}]);
            else
                throw new UserFriendlyException(m_StringLocalizer["vehicle:failure"]);
        }
    }
}