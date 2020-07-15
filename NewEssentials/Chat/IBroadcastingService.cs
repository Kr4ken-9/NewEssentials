using System.Threading.Tasks;

namespace NewEssentials.Chat
{
    public interface IBroadcastingService
    {
        public bool IsActive { get; set; }
        public Task StartBroadcast(int duration, string msg);
    }
}