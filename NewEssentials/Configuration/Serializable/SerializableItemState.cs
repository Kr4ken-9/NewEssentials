using OpenMod.Extensions.Games.Abstractions.Items;

namespace NewEssentials.Configuration.Serializable
{
    public class SerializableItemState : IItemState, ISerializable
    {
        public double ItemQuality { get; }
        public double ItemDurability { get; }
        public double ItemAmount { get; }
        public byte[] StateData { get; }

        public SerializableItemState(double itemQuality, double itemDurability, double itemAmount, byte[] stateData)
        {
            ItemQuality = itemQuality;
            ItemDurability = itemDurability;
            ItemAmount = itemAmount;
            StateData = stateData;
        }
    }
}