using System;

namespace NewEssentials.Models
{
    [Serializable]
    public class SerializableWarp
    {
        public int Cooldown { get; set; }
        
        public SerializableVector3 Location { get; set; }
        
        public SerializableWarp() {}

        public SerializableWarp(int cooldown, SerializableVector3 location)
        {
            Cooldown = cooldown;
            Location = location;
        }
    }
}