using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;
using OpenMod.API.Users;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.API.Players
{
    [Service]
    public interface IAfkChecker
    {
        /// <summary>
        /// Service must be configured with this property to work properly.
        /// </summary>
        /// <param name="timeout">Time, in seconds, an Unturned player can be AFK before being kicked</param>
        public void Configure(int timeout);
    }
}