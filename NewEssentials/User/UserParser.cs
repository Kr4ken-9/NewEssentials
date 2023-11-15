using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NewEssentials.API.User;
using NewEssentials.Memory;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.User;

[ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.High)]
public class UserParser : IUserParser
{
    private readonly IUserManager m_UserProvider;

    public UserParser(IUserManager userProvider)
    {
        m_UserProvider = userProvider;
    }

    //TODO: add option to use isInSameGroupAs
    public async Task<UnturnedUser> ParseUserAsync(string input, UnturnedUser u = null)
    {
        input = input.ToLower();
        IEnumerable<UnturnedUser> users = (await m_UserProvider.GetUsersAsync(KnownActorTypes.Player))
            .OfType<UnturnedUser>();
        
        UnturnedUser result = null;
        
        foreach (UnturnedUser usr in users)
        {
            if (input == usr.SteamId.m_SteamID.ToString())
                return usr;
            //only group members should know the private name, so configuration perhaps should be implemented for this where possible and untedious
            bool shouldCheckCharacterSide = !(u != null && !u.Player.SteamPlayer.isMemberOfSameGroupAs(usr.Player.SteamPlayer));
            if (shouldCheckCharacterSide && Compare(usr.Player.SteamPlayer.playerID.nickName.ToLower(), input)) 
                return usr; //it is easily this person (group side check succeeded)
            if (!CheckSteamSide(usr.Player.SteamPlayer, input, out bool definite))
                continue;
            if (definite) 
                return usr;
            result = usr;
        }
        
        return result;
    }

    private static bool CheckSteamSide(SteamPlayer p, string input, out bool forDefinite)
    {
        forDefinite = false;
        
        if (Compare(p.playerID.characterName.ToLower(), input))
        {
            forDefinite = true;
            return true;
        }

        return Compare(p.playerID.playerName.ToLower(), input);
    }

    public async Task<UnturnedUser> TryParseUserAsync(string input, ReferenceBoolean b, UnturnedUser actor = null)
    {
        UnturnedUser result = await ParseUserAsync(input, actor);
        b.Value = result != null;
        return result;
    }

    private static bool Compare(string input, string name)
    {
        //length comparison so that names that fit part of the input that aren't intended by the user aren't selected too randomly
        //TODO: test this constant, it might be better as 1 but i think in most cases it should be fine as this
        return name.Contains(input) || (input.Contains(name) && (input.Length - 2 <= name.Length));
    }
}