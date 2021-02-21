using System;

namespace NewEssentials.Models
{
    [Serializable]
    public class SerializableKit
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