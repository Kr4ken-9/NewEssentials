using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;
using OpenMod.API.Users;
using SDG.Unturned;

namespace NewEssentials.API.Players
{
    
    [Service]
    public interface IAfkChecker
    {
        public UniTask UpdateUser(IUser user);
        public Task UpdatePlayer(Player player);
    }
}