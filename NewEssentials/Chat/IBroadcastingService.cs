using System.Threading.Tasks;
using OpenMod.API.Ioc;

namespace NewEssentials.Chat
{
    [Service]
    public interface IBroadcastingService
    {
        public bool IsActive { get; set; }
        public Task StartBroadcast(int duration, string msg);
    }
}