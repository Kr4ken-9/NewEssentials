using NewEssentials.Models;
using SV3 = System.Numerics.Vector3;
using UV3 = UnityEngine.Vector3;

namespace NewEssentials.Extensions
{
    public static class VectorExtensions
    {
        public static SerializableVector3 ToSerializableVector(this UV3 vector)
            => new(vector.x, vector.y, vector.z);

        public static SerializableVector3 ToSerializableVector(this SV3 vector)
            => new(vector.X, vector.Y, vector.Z);
    }
}