using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace NewEssentials.Models
{
    [Serializable]
    public class WarpsData
    {
        public Dictionary<string, SerializableVector3> Warps { get; set; }
    }
}