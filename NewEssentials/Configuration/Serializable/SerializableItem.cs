using System;

namespace NewEssentials.Configuration.Serializable
{
    [Serializable]
    public class SerializableItem : ISerializable
    {
        public string ID { get; set; }
        public byte[] State { get; set; }
        public double Amount { get; set; }
        public double Durability { get; set; }
        public double Quality { get; set; }

        public SerializableItem()
        {
            
        }

        public SerializableItem(string id, byte[] state, double amount, double durability, double quality)
        {
            ID = id;
            State = state;
            Amount = amount;
            Durability = durability;
            Quality = quality;
        }
    }
}