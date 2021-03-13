using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

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

            UnturnedUser uPlayer = (UnturnedUser)Context.Actor;
            await UniTask.SwitchToMainThread();

            var currentVehicle = uPlayer.Player.Player.movement.getVehicle();
            if (currentVehicle != null)
            {
                if (!RefuelVehicle(currentVehicle))
                    throw new UserFriendlyException(m_StringLocalizer["refuel:vehicle:no_fuel"]);

                await PrintAsync(m_StringLocalizer["refuel:vehicle:success"]);
                return;
            }

            PlayerLook look = uPlayer.Player.Player.look;
            RaycastInfo raycast = DamageTool.raycast(new(look.aim.position, look.aim.forward), 100f, RayMasks.DAMAGE_SERVER);

            if (raycast.vehicle != null)
            {
                if (!RefuelVehicle(raycast.vehicle))
                    throw new UserFriendlyException(m_StringLocalizer["refuel:vehicle:no_fuel"]);

                await PrintAsync(m_StringLocalizer["refuel:vehicle:success"]);
                return;
            }

            if (raycast.transform == null)
            {
                throw new UserFriendlyException(m_StringLocalizer["refuel:none"]);
            }

            var interactable = raycast.transform.GetComponent<Interactable>();
            if (interactable != null)
            {
                if (interactable is InteractableGenerator generator)
                {
                    if (!generator.isRefillable)
                    {
                        throw new UserFriendlyException(m_StringLocalizer["refuel:object:no_fuel", new { Object = generator.name }]);
                    }

                    generator.askFill(generator.capacity);
                    BarricadeManager.sendFuel(raycast.transform, generator.fuel);

                    await PrintAsync(m_StringLocalizer["refuel:object:generator"]);
                    return;
                }
                else if (interactable is InteractableOil oil)
                {
                    if (!oil.isRefillable)
                    {
                        throw new UserFriendlyException(m_StringLocalizer["refuel:object:no_fuel", new { Object = oil.name }]);
                    }

                    oil.askFill(oil.capacity);
                    BarricadeManager.sendFuel(raycast.transform, oil.fuel);

                    await PrintAsync(m_StringLocalizer["refuel:object:oil"]);
                    return;
                }
                else if (interactable is InteractableTank { source: ETankSource.FUEL } tank)
                {
                    if (!tank.isRefillable)
                    {
                        throw new UserFriendlyException(m_StringLocalizer["refuel:object:no_fuel", new { Object = tank.name }]);
                    }

                    BarricadeManager.updateTank(raycast.transform, tank.capacity);

                    await PrintAsync(m_StringLocalizer["refuel:object:tank"]);
                    return;
                }
                else if (interactable is InteractableObjectResource { objectAsset: { interactability: EObjectInteractability.FUEL } } @object)
                {
                    if (!@object.isRefillable)
                    {
                        throw new UserFriendlyException(m_StringLocalizer["refuel:object:no_fuel", new { Object = @object.name }]);
                    }

                    ObjectManager.updateObjectResource(interactable.transform, @object.capacity, true);

                    // todo
                    await PrintAsync(m_StringLocalizer["refuel:object:object"]);
                    return;
                }
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