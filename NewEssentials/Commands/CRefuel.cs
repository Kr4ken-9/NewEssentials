using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Commands
{
    [Command("refuel")]
    [CommandDescription("Refuel the object you're looking at or current vehicle")]
    [CommandActor(typeof(UnturnedUser))]
    public class CRefuel : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CRefuel(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            await UniTask.SwitchToMainThread();

            var currentVehicle = uPlayer.Player.Player.movement.getVehicle();
            if (currentVehicle != null)
            {
                if (!RefuelVehicle(currentVehicle))
                    throw new UserFriendlyException(m_StringLocalizer["refuel:vehicle:no_fuel"]);

                await uPlayer.PrintMessageAsync(m_StringLocalizer["refuel:vehicle:success"]);
                return;
            }
            
            PlayerLook look = uPlayer.Player.Player.look;
            RaycastInfo raycast = DamageTool.raycast(new Ray(look.aim.position, look.aim.forward),
                100f, RayMasks.VEHICLE | RayMasks.BARRICADE);

            if (raycast.vehicle != null)
            {
                if (!RefuelVehicle(raycast.vehicle))
                    throw new UserFriendlyException(m_StringLocalizer["refuel:vehicle:no_fuel"]);

                await uPlayer.PrintMessageAsync(m_StringLocalizer["refuel:vehicle:success"]);
                return;
            }

            if (raycast.transform == null)
                throw new UserFriendlyException(m_StringLocalizer["refuel:none"]);

            var generator = raycast.transform.GetComponentInChildren<InteractableGenerator>();
            if (generator != null)
            {
                if (!generator.isRefillable)
                    throw new UserFriendlyException(m_StringLocalizer["refuel:object:no_fuel",
                        new {Object = generator.name}]);
                
                generator.askFill(generator.capacity);
                
                BarricadeManager.sendFuel(raycast.transform, generator.fuel);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["refuel:object:generator"]);
                return;
            }

            var oil = raycast.transform.GetComponentInChildren<InteractableOil>();
            if (oil != null)
            {
                if (!oil.isRefillable)
                    throw new UserFriendlyException(m_StringLocalizer["refuel:object:no_fuel",
                        new {Object = oil.name}]);
                
                oil.askFill(oil.capacity);
                
                BarricadeManager.sendFuel(raycast.transform, oil.fuel);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["refuel:object:oil"]);
                return;
            }
            
            var tank = raycast.transform.GetComponentInChildren<InteractableTank>();
            if (tank != null)
            {
                if (!tank.isRefillable)
                    throw new UserFriendlyException(
                        m_StringLocalizer["refuel:object:no_fuel", new {Object = tank.name}]);

                BarricadeManager.updateTank(raycast.transform, tank.capacity);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["refuel:object:tank"]);
                return;
            }

            throw new UserFriendlyException(m_StringLocalizer["refuel:none"]);
        }

        private bool RefuelVehicle(InteractableVehicle vehicle)
        {
            // vehicle.isRefillable returns false if the vehicle is driven
            if (!vehicle.usesFuel || vehicle.fuel >= vehicle.asset.fuel || vehicle.isExploded)
                return false;

            vehicle.fuel = vehicle.asset.fuel;
            VehicleManager.sendVehicleFuel(vehicle, vehicle.fuel);
            return true;
        }
    }
}