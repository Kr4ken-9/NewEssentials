using System;
using System.Threading.Tasks;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.User;

public static class UserExtensions
{
    public static async Task<UnturnedUser> ToUserAsync(this IUserManager prov, UnturnedPlayer pla)
    {
        if (await prov.FindUserAsync(KnownActorTypes.Player,
                pla.EntityInstanceId, UserSearchMode.FindById) is not UnturnedUser usr)
            throw new Exception($"Player {pla.SteamPlayer.playerID.characterName} not registered as a user");
        return usr;
    }
    
    public static async Task<UnturnedUser> ToUserAsync(this IUserManager prov, Player pla)
    {
        if (await prov.FindUserAsync(KnownActorTypes.Player,
                pla.channel.owner.playerID.characterName, UserSearchMode.FindByName) is not UnturnedUser usr)
            throw new Exception($"Player {pla.channel.owner.playerID.characterName} not registered as a user");
        return usr;
    }
}