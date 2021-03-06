using System;
using System.Collections.Generic;

namespace NewEssentials.Models
{
    [Serializable]
    public class WarpsData
    {
        public Dictionary<string, SerializableWarp> Warps { get; set; }
    }
}