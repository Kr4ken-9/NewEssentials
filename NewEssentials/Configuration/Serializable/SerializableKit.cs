using System;

namespace NewEssentials.Configuration.Serializable
{
    [Serializable]
    public class SerializableKit : ISerializable
    {
        public SerializableItem[] SerializableItems { get; set; }
        public int Cooldown { get; set; }
        
        public SerializableKit()
        {
            
        }

        public SerializableKit(SerializableItem[] serializableItems, int cooldown)
        {
            SerializableItems = serializableItems;
            Cooldown = cooldown;
        }
    }
}