using System.Collections.Generic;
using SDG.Unturned;
using SV3 = System.Numerics.Vector3;
using UV3 = UnityEngine.Vector3;

namespace NewEssentials.Configuration.Serializable;

public static class SerializableExtensions
{
    public static SerializableVector3 ToSerializableVector(this UV3 vector)
        => new(vector.x, vector.y, vector.z);
    public static SerializableVector3 ToSerializableVector(this SV3 vector)
        => new(vector.X, vector.Y, vector.Z);
    public static SerializableItem[] ToSerializableItems(this PlayerClothing clothing)
    {
        var serializableItems = new List<SerializableItem>();

        if (clothing.hat != 0)
            serializableItems.Add(new SerializableItem(clothing.hat.ToString(), clothing.hatState, 1, 100, clothing.hatQuality));

        if (clothing.glasses != 0)
            serializableItems.Add(new SerializableItem(clothing.glasses.ToString(), clothing.glassesState, 1, 100, clothing.glassesQuality));

        if (clothing.mask != 0)
            serializableItems.Add(new SerializableItem(clothing.mask.ToString(), clothing.maskState, 1, 100, clothing.maskQuality));

        if (clothing.shirt != 0)
            serializableItems.Add(new SerializableItem(clothing.shirt.ToString(), clothing.shirtState, 1, 100, clothing.shirtQuality));

        if (clothing.vest != 0)
            serializableItems.Add(new SerializableItem(clothing.vest.ToString(), clothing.vestState, 1, 100, clothing.vestQuality));

        if (clothing.backpack != 0)
            serializableItems.Add(new SerializableItem(clothing.backpack.ToString(), clothing.backpackState, 1, 100, clothing.backpackQuality));

        if (clothing.pants != 0)
            serializableItems.Add(new SerializableItem(clothing.pants.ToString(), clothing.pantsState, 1, 100, clothing.pantsQuality));

        return serializableItems.ToArray();
    }
}