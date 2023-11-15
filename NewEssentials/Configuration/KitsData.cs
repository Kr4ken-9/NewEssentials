using System;
using System.Collections.Generic;
using NewEssentials.Configuration.Serializable;

namespace NewEssentials.Configuration
{
    [Serializable]
    public class KitsData
    {
        public Dictionary<string, SerializableKit> Kits { get; set; }
    }
}