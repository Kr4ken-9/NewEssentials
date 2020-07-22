using OpenMod.API.Ioc;

namespace NewEssentials.API.Players
{
    [Service]
    public interface IGodManager
    {
        /// <summary>
        /// Turns a player into a God if they are mortal or vice versa
        /// </summary>
        /// <param name="steamID">SteamID of the player to become a God or a mortal</param>
        /// <returns>True if the player is now a God or false if they are now a mortal</returns>
        public bool ToggleGod(ulong steamID);
    }
}