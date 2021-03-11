using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Extensions.Games.Abstractions.Players;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Commands;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewEssentials.Commands.Clear
{
    [Command("vehicles")]
    [CommandAlias("vehicle")]
    [CommandAlias("v")]
    [CommandParent(typeof(CClearRoot))]
    [CommandDescription("Clears all vehicles or only empty vehicles")]
    [CommandSyntax("[clearEmpty] [radius]")]
    public class CClearVehicles : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CClearVehicles(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 2)
                throw new CommandWrongUsageException(Context);

            if (Context.Parameters.Length == 0)
            {
                await UniTask.SwitchToMainThread();

                ushort count = ClearVehiclesExceptTrain(VehicleManager.vehicles.ToList());

                await PrintAsync(m_StringLocalizer["clear:vehicles", new { Count = count }]);
                return;
            }

            var clearEmpty = Context.Parameters.Count >= 1 && await Context.Parameters.GetAsync<bool>(0);
            var radius = Context.Parameters.Count == 2 ? await Context.Parameters.GetAsync<float>(1) : 0f;
            var center = Context.Actor is IPlayerUser playerUser ? playerUser.Player.Transform.Position : new(0, 0, 0);

            await UniTask.SwitchToMainThread();

            List<InteractableVehicle> vehicles;
            if (radius > 0)
            {
                vehicles = new();
                VehicleManager.getVehiclesInRadius(center.ToUnityVector(), radius * radius, vehicles);
            }
            else
            {
                vehicles = VehicleManager.vehicles.ToList();
            }

            if (clearEmpty)
            {
                vehicles = vehicles.Where(x => x.isEmpty).ToList();
            }

            await UniTask.SwitchToMainThread();

            var counter = ClearVehiclesExceptTrain(vehicles);

            await PrintAsync(m_StringLocalizer[$"clear:vehicles{(clearEmpty ? "_empty" : string.Empty)}", new { counter }]);
        }

        private ushort ClearVehiclesExceptTrain(IEnumerable<InteractableVehicle> vehicles)
        {
            ushort counter = 0;
            foreach (var vehicle in vehicles)
            {
                if (vehicle.asset.engine != EEngine.TRAIN)
                {
                    counter++;
                    VehicleManager.askVehicleDestroy(vehicle);
                }
            }
            return counter;
        }
    }
}