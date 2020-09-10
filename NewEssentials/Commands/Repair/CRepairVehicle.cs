using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Commands.Repair
{
    [Command("vehicle")]
    [CommandAlias("v")]
    [CommandParent(typeof(CRepairRoot))]
    [CommandDescription("Repair your current vehicle or one you're looking at")]
    [CommandActor(typeof(UnturnedUser))]
    public class CRepairVehicle : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CRepairVehicle(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            var currentVehicle = uPlayer.Player.Player.movement.getVehicle();

            if (currentVehicle != null)
            {
                await UniTask.SwitchToMainThread();
                RepairVehicle(currentVehicle);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["repair:vehicle:current"]);
            }
            else
            {
                PlayerLook look = uPlayer.Player.Player.look;
                RaycastInfo raycast = DamageTool.raycast(new Ray(look.aim.position, look.aim.forward),
                    100f, RayMasks.VEHICLE);

                if (raycast.vehicle == null)
                    throw new UserFriendlyException(m_StringLocalizer["repair:vehicle:none"]);

                await UniTask.SwitchToMainThread();
                RepairVehicle(raycast.vehicle);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["repair:vehicle:looking"]);
            }
        }

        private void RepairVehicle(InteractableVehicle vehicle)
        {
            if (!vehicle.usesHealth)
                return;

            // method that compares maxHealth to vehicle.health
            // not really necessary but Nelson included it so why not
            if (vehicle.isRepaired)
                return;

            ushort maxHealth = vehicle.asset.health;
            vehicle.health = maxHealth;
            VehicleManager.sendVehicleHealth(vehicle, maxHealth);
        }
    }
}