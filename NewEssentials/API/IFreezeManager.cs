using OpenMod.API.Ioc;
using UnityEngine;

namespace NewEssentials.API
{
    /// <summary>
    /// Manages freezed players
    /// </summary>
    [Service]
    public interface IFreezeManager
    {
        /// <summary>
        /// Freeze a player in place
        /// </summary>
        /// <param name="player">SteamID of the player to freeze</param>
        /// <param name="position">Position to constrain player to</param>
        void FreezePlayer(ulong player, Vector3 position);

        /// <summary>
        /// Unfreeze a frozen player
        /// </summary>
        /// <param name="player">SteamID of the frozen player</param>
        void UnfreezePlayer(ulong player);

        /// <summary>
        /// Check if a player is frozen
        /// </summary>
        /// <param name="player">SteamID of the player to check</param>
        bool IsPlayerFrozen(ulong player);
    }
}