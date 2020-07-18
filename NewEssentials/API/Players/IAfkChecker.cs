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
        /// <summary>
        /// Updates the last time a user moved
        /// </summary>
        /// <param name="user">User that moved</param>
        public UniTask UpdateUser(IUser user);
        
        /// <summary>
        /// Updates the last time an Unturned player moved
        /// </summary>
        /// <param name="player">Unturned Player that moved</param>
        public UniTask UpdatePlayer(Player player);
    }
}