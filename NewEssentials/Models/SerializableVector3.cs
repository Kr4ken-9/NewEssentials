using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace NewEssentials.Models
{
    [Serializable]
    public class SerializableVector3
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public SerializableVector3()
        {
        }

        public SerializableVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 ToUnityVector3() => new Vector3(X, Y, Z);

        // This whole thing is retarded.
        public static SerializableVector3 GetSerializableVector3FromUserData(Dictionary<object, object> userData, string indexName = null)
        {
            Dictionary<object, object> vector = indexName != null ? (Dictionary<object, object>)userData[indexName] : userData;

            float x = float.Parse(vector["x"].ToString(), CultureInfo.InvariantCulture);
            float y = float.Parse(vector["y"].ToString(), CultureInfo.InvariantCulture);
            float z = float.Parse(vector["z"].ToString(), CultureInfo.InvariantCulture);

            return new SerializableVector3(x, y, z);
        }
    }
}