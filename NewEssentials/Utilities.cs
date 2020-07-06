using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NewEssentials.Models;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials
{
    public static class Utilities
    {
        public static async Task MaxAllSkills(this PlayerSkills playerSkills, bool kunii = false)
        {
            for (byte speciality = 0; speciality < playerSkills.skills.Length; speciality++)
            {
                Skill[] skills = playerSkills.skills[speciality];
                byte[] newLevels = new byte[skills.Length];

                for (byte index = 0; index < skills.Length; index++)
                {
                    Skill skill = skills[index];

                    if (kunii)
                        skill.level = byte.MaxValue;
                    else
                        skill.setLevelToMax();

                    newLevels[index] = skill.level;
                }

                await UniTask.SwitchToMainThread();
                // No achievements lol
                playerSkills.channel.send("tellSkills", playerSkills.channel.owner.playerID.steamID, ESteamPacket.UPDATE_RELIABLE_BUFFER, speciality, newLevels);
                await Task.Yield();
            }
        }
        
        public static SerializableVector3 ToSerializableVector3(this Vector3 vector3) => new SerializableVector3(vector3.x, vector3.y, vector3.z);

        public static bool GetItem(string searchTerm, out ItemAsset item)
        {
            if (string.IsNullOrEmpty(searchTerm.Trim()))
            {
                item = null;
                return false;
            }
            
            if (!ushort.TryParse(searchTerm, out ushort id))
            {
                item = Assets.find(EAssetType.ITEM).Cast<ItemAsset>().Where(i => !string.IsNullOrEmpty(i.itemName))
                    .OrderBy(i => i.itemName.Length).FirstOrDefault(i =>
                        i.itemName.ToUpperInvariant().Contains(searchTerm.ToUpperInvariant()));

                return item != null;
            }

            item = (ItemAsset) Assets.find(EAssetType.ITEM, id);
            return item != null;
        }

        public static bool GetVehicle(string searchTerm, out VehicleAsset vehicle)
        {
            if (string.IsNullOrEmpty(searchTerm.Trim()))
            {
                vehicle = null;
                return false;
            }

            if (!ushort.TryParse(searchTerm, out ushort id))
            {
                vehicle = Assets.find(EAssetType.VEHICLE).Cast<VehicleAsset>()
                    .Where(v => !string.IsNullOrEmpty(v.vehicleName)).OrderBy(v => v.vehicleName.Length)
                    .FirstOrDefault(v => v.vehicleName.ToUpperInvariant().Contains(searchTerm.ToUpperInvariant()));

                return vehicle != null;
            }

            vehicle = (VehicleAsset) Assets.find(EAssetType.VEHICLE, id);
            return vehicle != null;
        }

        public static bool GetItemAmount(byte inputAmount, IConfiguration configuration, out byte finalAmount)
        {
            finalAmount = inputAmount;

            if (!configuration.GetValue<bool>("items:enableamountlimit"))
                return true;
            
            byte maxAmount = configuration.GetValue<byte>("items:maxspawnamount");
            if (finalAmount <= maxAmount)
                return true;

            finalAmount = maxAmount;
            
            return configuration.GetValue<bool>("items:silentamountlimit");
        }
    }
}