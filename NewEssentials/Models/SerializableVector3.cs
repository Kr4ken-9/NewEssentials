using System.Collections.Generic;
using OpenMod.API.Users;
using UnityEngine;

namespace NewEssentials.Models
{
    public class SerializableVector3
    {
        public float X { get; set; }
        
        public float Y { get; set; }
        
        public float Z { get; set; }

        public SerializableVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 ToUnityVector3() => new Vector3(X, Y, Z);

        // This whole thing is retarded.
        public static SerializableVector3 GetSerializableVector3FromUserData(Dictionary<object, object> homes, string homeName)
        {
            var home = (Dictionary<object, object>)homes[homeName];

            float x = float.Parse((string) home["x"]);
            float y = float.Parse((string) home["y"]);
            float z = float.Parse((string) home["z"]);
            
            return new SerializableVector3(x, y, z);
        }
    }
}