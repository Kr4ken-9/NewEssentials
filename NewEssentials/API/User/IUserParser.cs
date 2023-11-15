using System.Threading.Tasks;
using NewEssentials.Memory;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Users;

namespace NewEssentials.API.User;

[Service]
public interface IUserParser
{
    public Task<UnturnedUser> ParseUserAsync(string input, UnturnedUser u = null);
    public Task<UnturnedUser> TryParseUserAsync(string input, ReferenceBoolean b, UnturnedUser u = null);
}