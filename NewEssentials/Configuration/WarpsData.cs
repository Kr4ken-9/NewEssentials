using System;
using System.Collections.Generic;
using System.Linq;
using NewEssentials.Configuration.Serializable;

namespace NewEssentials.Configuration
{
    [Serializable]
    public class WarpsData
    {
        public Dictionary<string, SerializableWarp> Warps { get; set; }

        public SerializableWarp this[string anyCase]
        {
            get
            {
                try
                {
                    return Warps[Warps.Keys.First(k => string.Equals(k, anyCase, StringComparison.CurrentCultureIgnoreCase))];
                }
                catch (Exception e)
                {
                    //Any usages will throw for us if this returns null
                    return null;
                }
            }
        }

        public bool ContainsWarp(string name)
        {
            return Warps.Keys.Any(w => string.Equals(w, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}