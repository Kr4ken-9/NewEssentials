using OpenMod.API.Ioc;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.API.Players
{
    /// <summary>
    /// Manages freezed players
    /// </summary>
    [Service]
    public interface IPlayerFreezer
    {
        /// <summary>
        /// Freeze a player in place
        /// </summary>
        /// <param name="player">SteamID of the player to freeze</param>
        /// <param name="position">Position to constrain player to</param>
        void FreezePlayer(SteamPlayer player, Vector3 position);

        /// <summary>
        /// Unfreeze a frozen player
        /// </summary>
        /// <param name="player">SteamID of the frozen player</param>
        void UnfreezePlayer(SteamPlayer player);

        /// <summary>
        /// Check if a player is frozen
        /// </summary>
        /// <param name="player">SteamID of the player to check</param>
        bool IsPlayerFrozen(SteamPlayer player);
    }
}