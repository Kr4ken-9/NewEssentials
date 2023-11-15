using System;

namespace NewEssentials.Configuration.Serializable
{
    [Serializable]
    public class SerializableWarp : ISerializable
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