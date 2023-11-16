using System;
using System.Collections.Generic;
using System.Linq;
using NewEssentials.Configuration.Serializable;
using YamlDotNet.Serialization;

namespace NewEssentials.Configuration
{
    [Serializable]
    public class WarpsData : EnumerableData<SerializableWarp>
    {
        [YamlMember(Alias = "Warps")]
        protected sealed override Dictionary<string, SerializableWarp> Data { get; set; }
    }
}