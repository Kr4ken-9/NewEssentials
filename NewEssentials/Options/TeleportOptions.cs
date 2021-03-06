using OpenMod.API;

namespace NewEssentials.Options
{
    public class TeleportOptions
    {
        public IOpenModComponent Instance;
        
        public int Delay;

        public bool CancelOnMove;

        public bool CancelOnDamage;

        public TeleportOptions(IOpenModComponent instance, int delay, bool cancelOnMove, bool cancelOnDamage)
        {
            Instance = instance;
            Delay = delay;
            CancelOnMove = cancelOnMove;
            CancelOnDamage = cancelOnDamage;
        }
    }
}