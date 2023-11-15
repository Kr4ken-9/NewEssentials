using System;

namespace NewEssentials.System;

public class ReferenceBoolean : IEquatable<bool>
{
    protected bool Equals(ReferenceBoolean other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is bool b)
            return Value == b;
        return obj is ReferenceBoolean rb && Equals(rb);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public bool Value { get; set; }

    public static implicit operator bool(ReferenceBoolean bol)
    {
        return bol.Value;
    }

    public static implicit operator ReferenceBoolean(bool b)
    {
        return new ReferenceBoolean {Value = b};
    }
    
    public static bool operator ==(ReferenceBoolean b1, bool b2)
    {
        if (b1 == null)
            return false;
        return b1.Value == b2;
    }

    public static bool operator !=(ReferenceBoolean b1, bool b2)
    {
        return !(b1 == b2);
    }
    
    public static bool operator ==(ReferenceBoolean b1, ReferenceBoolean b2)
    {
        if (b1 == null || b2 == null)
            return false;
        return b1.Value == b2.Value;
    }

    public static bool operator !=(ReferenceBoolean b1, ReferenceBoolean b2)
    {
        return !(b1 == b2);
    }

    public bool Equals(bool other)
    {
        return Value == other;
    }
}