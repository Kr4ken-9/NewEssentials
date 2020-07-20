using Microsoft.Extensions.Localization;
using OpenMod.API.Ioc;

namespace NewEssentials.API.Players
{
    [Service]
    public interface IAfkChecker
    {
        /// <summary>
        /// Service must be configured with this property to work properly.
        /// </summary>
        /// <param name="timeout">Time, in seconds, an Unturned player can be AFK before being kicked</param>
        /// <param name="announceAFK">Whether or not to announce when a player becomes AFK</param>
        /// <param name="kickAFK">Whether or not to kick players who become AFK</param>
        /// <param name="stringLocalizer">For localization</param>
        public void Configure(int timeout, bool announceAFK, bool kickAFK, IStringLocalizer stringLocalizer);
    }
}