using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;

namespace NewEssentials.API.Chat
{
    [Service]
    public interface IBroadcastingService
    {
        /// <summary>
        /// If a message is currently being broadcasted
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Service must be configured with this method to work properly.
        /// </summary>
        /// <param name="effectID">Broadcast effect ID</param>
        /// <param name="repeatingMessages">Messages to cycle through and repeat</param>
        /// <param name="repeatingInterval">Interval between messages in milliseconds</param>
        /// <param name="repeatingDuration">Duration that messages last in milliseconds</param>
        public void Configure(ushort effectID, string[] repeatingMessages, int repeatingInterval, int repeatingDuration);
        
        /// <summary>
        /// Broadcast a message to all Unturned players
        /// </summary>
        /// <param name="duration">Duration in milliseconds of the broadcast</param>
        /// <param name="msg">Message to be broadcasted</param>
        public UniTask StartBroadcast(int duration, string msg);
    }
}