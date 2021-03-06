using System.Threading.Tasks;
using OpenMod.API.Ioc;
using OpenMod.Core.Users;

namespace NewEssentials.API.Players
{
    /// <summary>
    /// Manages user cooldowns through session data
    /// </summary>
    [Service]
    public interface ICooldownManager
    {
        /// <summary>
        /// Determines whether a user is under a specific type of cooldown
        /// </summary>
        /// <param name="user">User to check for cooldown</param>
        /// <param name="sessionKey">What type of cooldown (warps, kits, etc)</param>
        /// <param name="cooldownName">Specific cooldown (name of warp, kit, etc)</param>
        /// <param name="cooldown">Default cooldown time</param>
        /// <returns>Remaining time if there is a cooldown</returns>
        public Task<double?> OnCooldownAsync(UserBase user, string sessionKey, string cooldownName, int cooldown);
    }
}