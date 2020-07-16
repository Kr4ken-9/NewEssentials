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
        /// Broadcast a message to all Unturned players
        /// </summary>
        /// <param name="duration">Duration in milliseconds of the broadcast</param>
        /// <param name="msg">Message to be broadcasted</param>
        public UniTask StartBroadcast(int duration, string msg);
    }
}