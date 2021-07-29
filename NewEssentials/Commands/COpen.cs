using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
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
        private readonly IStringLocalizer m_StringLocalizer;

        public COpen(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
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
                BarricadeManager.ServerSetDoorOpen(hinge.door, !hinge.door.isOpen);
                return;
            }
            else if (interactable is InteractableStorage storage)
            {
                user.Player.Player.inventory.openStorage(storage);
                return;
            }
            else if (interactable is InteractableVehicle vehicle)
            {
                var shouldLock = !vehicle.isLocked;

                var owner = shouldLock ? user.SteamId : vehicle.lockedOwner;
                var group = shouldLock ? user.Player.Player.quests.groupID : vehicle.lockedGroup;

                VehicleManager.ServerSetVehicleLock(vehicle, owner, group, shouldLock);
                return;
            }

            await PrintAsync(m_StringLocalizer["open:invalid"]);
        }
    }
}
