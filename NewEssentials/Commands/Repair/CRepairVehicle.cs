using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Users;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.Repair
{
    [UsedImplicitly]
    [Command("vehicle")]
    [CommandAlias("v")]
    [CommandParent(typeof(CRepairRoot))]
    [CommandDescription("Repair your current vehicle or one you're looking at")]
    [CommandActor(typeof(UnturnedUser))]
    public class CRepairVehicle : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CRepairVehicle(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.repair.vehicle";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            var currentVehicle = uPlayer.Player.movement.getVehicle();

            if (currentVehicle != null)
            {
                await UniTask.SwitchToMainThread();
                RepairVehicle(currentVehicle);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["repair:vehicle:current"]);
            }
            else
            {
                PlayerLook look = uPlayer.Player.look;
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