using System;
using System.Collections.Generic;
using System.Linq;
using NewEssentials.Configuration.Serializable;

namespace NewEssentials.Configuration;

//TODO: integrate this abstraction with the persistence system
public abstract class EnumerableData<T> where T : class, ISerializable
{
    public abstract Dictionary<string, T> Data { get; set; }
    
    public T this[string anyCase]
    {
        get
        {
            try
            {
                return Data[Data.Keys.First(k => string.Equals(k, anyCase, StringComparison.CurrentCultureIgnoreCase))];
            }
            catch (Exception e)
            {
                //Any usages will throw for us if this returns null
                return (T) null;
            }
        }
    }
    
    public bool ContainsKey(string name)
    {
        return Data.Keys.Any(w => string.Equals(w, name, StringComparison.CurrentCultureIgnoreCase));
    }
}