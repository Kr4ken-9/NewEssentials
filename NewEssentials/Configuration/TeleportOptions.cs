using OpenMod.API;

namespace NewEssentials.Configuration
{
    public class TeleportOptions
    {
        public IOpenModComponent Instance { get; }
        public int Delay { get; }
        public bool CancelOnMove { get; }
        public bool CancelOnDamage { get; }

        public TeleportOptions(IOpenModComponent instance, int delay, bool cancelOnMove, bool cancelOnDamage)
        {
            Instance = instance;
            Delay = delay;
            CancelOnMove = cancelOnMove;
            CancelOnDamage = cancelOnDamage;
        }
    }
}