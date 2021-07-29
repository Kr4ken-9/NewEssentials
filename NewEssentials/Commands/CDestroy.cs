using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Framework.Utilities;
using SDG.Unturned;
using System;
using UnityEngine;

namespace NewEssentials.Commands
{
    [Command("destroy")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandDescription("Destroys the barricade, structure, or vehicle that you are looking at.")]
    public class CDestroy : UnturnedCommand
    {
        public CDestroy(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            var user = (UnturnedUser)Context.Actor;
            var look = user.Player.Player.look;

            await UniTask.SwitchToMainThread();

            if (!PhysicsUtility.raycast(new(look.getEyesPosition(), look.aim.forward),
                out var hit, 8f, RayMasks.BARRICADE | RayMasks.STRUCTURE | RayMasks.VEHICLE))
            {
                return;
            }

            var vehicle = hit.collider.GetComponent<InteractableVehicle>();
            if (vehicle != null)
            {
                VehicleManager.askVehicleDestroy(vehicle);
                return;
            }

            if (BarricadeManager.tryGetInfo(hit.transform, out var x, out var y, out var plant, out var index, out var region))
            {
                BarricadeManager.destroyBarricade(region, x, y, plant, index);
                return;
            }

            if (StructureManager.tryGetInfo(hit.transform, out x, out y, out index, out var structureRegion))
            {
                StructureManager.destroyStructure(structureRegion, x, y, index, Vector3.zero);
                return;
            }
        }
    }
}
