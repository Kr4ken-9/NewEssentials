using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;

namespace NewEssentials.Chat
{
    [Service]
    public interface IBroadcastingService
    {
        public bool IsActive { get; set; }
        public UniTask StartBroadcast(int duration, string msg);
    }
}