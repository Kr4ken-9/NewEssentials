using NewEssentials.Models;
using UnityEngine;

namespace NewEssentials.Extensions
{
    public static class VectorExtensions
    {
        public static SerializableVector3 ToSerializableVector3(this Vector3 vector3) => new SerializableVector3(vector3.x, vector3.y, vector3.z);

        public static Vector3 ToUnityEngineVector3(this System.Numerics.Vector3 vector3) => new Vector3(vector3.X, vector3.Y, vector3.Z);
    }
}