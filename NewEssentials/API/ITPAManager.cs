using Microsoft.Extensions.Localization;
using OpenMod.API.Ioc;

namespace NewEssentials.API
{
    /// <summary>
    /// Manages TPA requests
    /// </summary>
    [Service]
    public interface ITPAManager
    {
        /// <summary>
        /// Checks if a request to the recipient has already been opened
        /// </summary>
        /// <param name="recipient">SteamID of the recipient</param>
        /// <param name="requester">SteamID of the requester</param>
        bool IsRequestOpen(ulong recipient, ulong requester);

        /// <summary>
        /// Opens a new TPA request from the requester to the recipient
        /// </summary>
        /// <param name="recipient">SteamID of the recipient</param>
        /// <param name="requester">SteamID of the requester</param>
        /// <param name="requestLifetime">Time, in milliseconds, before request expires</param>
        void OpenNewRequest(ulong recipient, ulong requester, int requestLifetime);

        void SetLocalizer(IStringLocalizer stringLocalizer);
    }
}