using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NewEssentials.Configuration.Serializable;

namespace NewEssentials.Configuration;

//TODO: integrate this abstraction with the persistence system
[Serializable]
public abstract class EnumerableData<T> : IEnumerable<KeyValuePair<string, T>> where T : class, ISerializable
{
    protected abstract Dictionary<string, T> Data { get; set; }
    
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

    public void Add(string name, T element)
    {
        Data.Add(name, element);
    }

    public void Remove(string name)
    {
        Data.Remove(name);
    }
    
    public uint Count => (uint) Data.Keys.Count;
    public IReadOnlyCollection<string> Keys => Data.Keys;
    
    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
    {
        return Data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}