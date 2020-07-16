using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;

namespace NewEssentials.API.Chat
{
    [Service]
    public interface IBroadcastingService
    {
        public bool IsActive { get; set; }
        public UniTask StartBroadcast(int duration, string msg);
    }
}