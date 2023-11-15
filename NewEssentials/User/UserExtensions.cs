using System;
using System.Threading.Tasks;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Unturned.Players;

namespace NewEssentials.User;

public static class UserExtensions
{
    public static async Task<IUser> ToUserAsync(this IUserProvider prov, UnturnedPlayer pla)
    {
        IUser usr = await prov.FindUserAsync(KnownActorTypes.Player,
            pla.EntityInstanceId, UserSearchMode.FindById); //? i want to kms
        if (usr == null)
            throw new Exception($"Player {pla.SteamPlayer.playerID.playerName} not registered as a user");
        return usr;
    }
}