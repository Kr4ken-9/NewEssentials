using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.API.User;
using NewEssentials.Unturned;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Commands.Vehicles
{
    [Command("givevehicle")]
    [CommandAlias("gv")]
    [CommandDescription("Spawn a vehicle for another player")]
    [CommandSyntax("<player> <name>/<id>")]
    public class CGiveVehicle : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserParser m_UserParser;

        public CGiveVehicle(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider, IUserParser userParser) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 2)
                throw new CommandWrongUsageException(Context);

            string playerSearchTerm = Context.Parameters[0];
            UnturnedUser usr = await m_UserParser.ParseUserAsync(playerSearchTerm);
            if (usr == null)
                throw new UserFriendlyException(m_StringLocalizer["vehicle:invalid_player", new {Player = playerSearchTerm}]);
            string vehicleSearchTerm = Context.Parameters[1];
            if (!UnturnedAssetHelper.GetVehicle(vehicleSearchTerm, out VehicleAsset vehicle))
                throw new UserFriendlyException(m_StringLocalizer["vehicle:invalid",
                    new {Vehicle = vehicleSearchTerm}]);

            await UniTask.SwitchToMainThread();
            if (VehicleTool.giveVehicle(usr.Player.Player, vehicle.id))
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["vehicle:success_given",
                    new {Vehicle = vehicle.vehicleName, Player = usr.Player.SteamPlayer.playerID.characterName}]);
            else
                throw new UserFriendlyException(m_StringLocalizer["vehicle:failure"]);
        }
    }
}