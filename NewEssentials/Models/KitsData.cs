﻿using System;
using System.Collections.Generic;

namespace NewEssentials.Models
{
    [Serializable]
    public class KitsData
    {
        public Dictionary<string, SerializableKit> Kits { get; set; }
    }
}