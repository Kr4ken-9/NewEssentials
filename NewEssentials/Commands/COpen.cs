using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Framework.Utilities;
using SDG.Unturned;
using System;

namespace NewEssentials.Commands
{
    [Command("open")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandDescription("Force open door, storage, and vehicle")]
    public class COpen : UnturnedCommand
    {
        public COpen(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            var user = (UnturnedUser)Context.Actor;
            var look = user.Player.Player.look;

            await UniTask.SwitchToMainThread();
            if (!PhysicsUtility.raycast(new(look.getEyesPosition(), look.aim.forward),
                out var hit, 8f, RayMasks.BARRICADE | RayMasks.VEHICLE))
            {
                return;
            }

            var interactable = hit.collider.GetComponent<Interactable>();
            if (interactable is InteractableDoorHinge hinge and { door: not null })
            {
                if (BarricadeManager.ServerSetDoorOpen(hinge.door, !hinge.door.isOpen))
                {

                }
            }
            else if (interactable is InteractableStorage storage)
            {
                user.Player.Player.inventory.openStorage(storage);
            }
            else if (interactable is InteractableVehicle vehicle)
            {
                var shouldLock = !vehicle.isLocked;

                var owner = shouldLock ? user.SteamId : vehicle.lockedOwner;
                var group = shouldLock ? user.Player.Player.quests.groupID : vehicle.lockedGroup;

                VehicleManager.ServerSetVehicleLock(vehicle, owner, group, shouldLock);
            }
        }
    }
}
