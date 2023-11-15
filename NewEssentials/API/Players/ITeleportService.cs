using System.Threading.Tasks;
using NewEssentials.Configuration;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Users;

namespace NewEssentials.API.Players
{
    /// <summary>
    /// Handles teleporting through commands and cancelling if necessary
    /// </summary>
    [Service]
    public interface ITeleportService
    {
        /// <summary>
        /// Adds a delay to teleportation and cancels where necessary
        /// </summary>
        /// <param name="user">User which is teleporting</param>
        /// <param name="options">Includes delay, cancel on move, and cancel on damage</param>
        public Task<bool> TeleportAsync(UnturnedUser user, TeleportOptions options);
    }
}