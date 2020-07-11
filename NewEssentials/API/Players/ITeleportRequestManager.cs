using Microsoft.Extensions.Localization;
using OpenMod.API.Ioc;

namespace NewEssentials.API.Players
{
    /// <summary>
    /// Manages TPA requests
    /// </summary>
    [Service]
    public interface ITeleportRequestManager
    {
        /// <summary>
        /// Checks if any requests to the recipient are pending
        /// </summary>
        /// <param name="recipient">SteamID of the recipient</param>
        bool IsRequestOpen(ulong recipient);
        
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

        /// <summary>
        /// Accepts the first pending request and returns the requester
        /// </summary>
        /// <param name="recipient">SteamID of the recipient</param>
        /// <returns>SteamID of the requester</returns>
        ulong AcceptRequest(ulong recipient);

        /// <summary>
        /// Accepts the pending request by the requester
        /// </summary>
        /// <param name="recipient">SteamID of the recipient</param>
        /// <param name="requester">SteamID of the requester</param>
        void AcceptRequest(ulong recipient, ulong requester);

        void SetLocalizer(IStringLocalizer stringLocalizer);
    }
}