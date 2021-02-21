using System;
using System.Collections.Generic;

namespace NewEssentials.Models
{
    [Serializable]
    public class KitsData
    {
        public Dictionary<string, SerializableItem> Kits { get; set; }
    }
}