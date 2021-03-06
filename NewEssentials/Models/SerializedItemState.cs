using OpenMod.Extensions.Games.Abstractions.Items;

namespace NewEssentials.Models
{
    public class SerializedItemState : IItemState
    {
        public double ItemQuality { get; }
        public double ItemDurability { get; }
        public double ItemAmount { get; }
        public byte[] StateData { get; }

        public SerializedItemState(double itemQuality, double itemDurability, double itemAmount, byte[] stateData)
        {
            ItemQuality = itemQuality;
            ItemDurability = itemDurability;
            ItemAmount = itemAmount;
            StateData = stateData;
        }
    }
}