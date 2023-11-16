using System;
using System.Collections.Generic;
using NewEssentials.Configuration.Serializable;
using YamlDotNet.Serialization;

namespace NewEssentials.Configuration
{
    [Serializable]
    public class KitsData : EnumerableData<SerializableKit>
    {
        [YamlMember(Alias = "Kits")]
        protected sealed override Dictionary<string, SerializableKit> Data { get; set; }

        public KitsData()
        {
            
        }

        public KitsData(Dictionary<string, SerializableKit> data)
        {
            Data = data;
        }
        
    }
}