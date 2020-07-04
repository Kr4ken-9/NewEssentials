using OpenMod.API.Ioc;

namespace NewEssentials.API
{
    /// <summary>
    /// Manages Private Messages
    /// </summary>
    [Service]
    public interface IPrivateMessageManager
    {
        /// <summary>
        /// Records the last user to message another user
        /// </summary>
        /// <param name="recipient">SteamID of the recipient</param>
        /// <param name="sender">SteamID of the sender</param>
        void RecordLastMessager(ulong recipient, ulong sender);

        /// <summary>
        /// Gets the last messager of user
        /// </summary>
        /// <param name="recipient">SteamID of the recipient</param>
        /// <returns>SteamID of the messager</returns>
        ulong? GetLastMessager(ulong recipient);
    }
}