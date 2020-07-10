using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
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
            
            //TODO: Not working lmao
            var currentVehicle = uPlayer.Player.movement.getVehicle();
            if (currentVehicle != null)
            {
                await UniTask.SwitchToMainThread();
                if (!RefuelVehicle(currentVehicle))
                    throw new UserFriendlyException(m_StringLocalizer["refuel:vehicle:no_fuel"]);

                await uPlayer.PrintMessageAsync(m_StringLocalizer["refuel:vehicle:success"]);
                return;
            }
            
            PlayerLook look = uPlayer.Player.look;
            RaycastInfo raycast = DamageTool.raycast(new Ray(look.aim.position, look.aim.forward),
                100f, RayMasks.VEHICLE | RayMasks.BARRICADE);

            if (raycast.vehicle != null)
            {
                await UniTask.SwitchToMainThread();
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
                await UniTask.SwitchToMainThread();
                
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
                await UniTask.SwitchToMainThread();
                
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

                await UniTask.SwitchToMainThread();
                BarricadeManager.updateTank(raycast.transform, tank.capacity);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["refuel:object:tank"]);
                return;
            }

            throw new UserFriendlyException(m_StringLocalizer["refuel:none"]);
        }

        private bool RefuelVehicle(InteractableVehicle vehicle)
        {
            if (!vehicle.isRefillable)
                return false;

            vehicle.fuel = vehicle.asset.fuel;
            VehicleManager.sendVehicleFuel(vehicle, vehicle.fuel);
            return true;
        }
    }
}