using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.API.User;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using CommandWrongUsageException = OpenMod.Core.Commands.CommandWrongUsageException;

namespace NewEssentials.Commands
{
    [Command("investigate")]
    [CommandDescription("Get a player's steamid or the owner's steamid of a barricade or structure you are looking at.")]
    [CommandSyntax("[player]")]
    public class CInvestigate : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserParser m_UserParser;

        public CInvestigate(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider, IUserParser userParser) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            if (Context.Parameters.Length == 1)
            {
                string searchTerm = Context.Parameters[0];
                IUser potentialUser =
                    await m_UserParser.ParseUserAsync(searchTerm);

                if (potentialUser == null)
                    throw new UserFriendlyException(m_StringLocalizer["general:invalid_player",
                        new {Player = searchTerm}]);

                await Context.Actor.PrintMessageAsync(m_StringLocalizer["investigate:success",
                    new {Player = searchTerm, ID = potentialUser.Id}]);
                    
                return;
            }

            if (Context.Actor is not UnturnedUser uPlayer)
                throw new CommandWrongUsageException(Context);

            PlayerLook look = uPlayer.Player.Player.look;
            RaycastInfo raycast = DamageTool.raycast(new Ray(look.aim.position, look.aim.forward), 256f,
                RayMasks.DAMAGE_SERVER);

            Transform raycastTransform = raycast.transform;

            if (raycastTransform == null)
                throw new UserFriendlyException(m_StringLocalizer["investigate:no_object"]);

            CSteamID ownerSteamID = CSteamID.Nil;
            if (raycast.vehicle != null) 
                ownerSteamID = raycast.vehicle.lockedOwner;

            InteractableBed bed = raycastTransform.GetComponentInChildren<InteractableBed>();
            if (bed != null) 
                ownerSteamID = bed.owner;

            InteractableDoor door = raycastTransform.GetComponentInChildren<InteractableDoor>();
            if (door != null)
                ownerSteamID = door.owner;

            InteractableStorage storage = raycastTransform.GetComponentInChildren<InteractableStorage>();
            if (storage != null)
                ownerSteamID = storage.owner;
            
            

            await uPlayer.PrintMessageAsync(GetObjectOwner(ownerSteamID));
        }

        private string GetObjectOwner(CSteamID steamID) => steamID == CSteamID.Nil
            ? m_StringLocalizer["investigate:no_owner"]
            : m_StringLocalizer["investigate:success_object", new {ID = steamID.m_SteamID.ToString()}];
    }
}