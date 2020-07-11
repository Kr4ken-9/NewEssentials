using NewEssentials.Models;
using UnityEngine;

namespace NewEssentials.Extensions
{
    public static class VectorExtensions
    {
        public static SerializableVector3 ToSerializableVector3(this Vector3 vector3) => new SerializableVector3(vector3.x, vector3.y, vector3.z);
    }
}