using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NewEssentials.Models;
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

        public static void ClearInventory(this Player player)
        {
            Items[] items = player.inventory.items;
            for (byte b = 0; b < PlayerInventory.PAGES; b++)
            {
                if (b == PlayerInventory.AREA)
                    continue;
                
                items[b].Clear();

                if (b < PlayerInventory.SLOTS)
                    player.equipment.sendSlot(b);
            }

            PlayerClothing clothing = player.clothing;
            clothing.sendSwapBackpack(255, 0, 0);
            items[PlayerInventory.SLOTS].Clear();
            
            clothing.sendSwapGlasses(255, 0, 0);
            items[PlayerInventory.SLOTS].Clear();
            
            clothing.sendSwapHat(255, 0, 0);
            items[PlayerInventory.SLOTS].Clear();
            
            clothing.sendSwapMask(255, 0, 0);
            items[PlayerInventory.SLOTS].Clear();
            
            clothing.sendSwapPants(255, 0, 0);
            items[PlayerInventory.SLOTS].Clear();
            
            clothing.sendSwapShirt(255, 0, 0);
            items[PlayerInventory.SLOTS].Clear();

            clothing.sendSwapVest(255, 0, 0);
            items[PlayerInventory.SLOTS].Clear();
        }

        public static void Clear(this Items item)
        {
            for (byte b = 0; b < item.getItemCount(); b++)
                item.removeItem(b);
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        public static bool teleportToLocation(this Player player, Vector3 position) =>
            player.teleportToLocation(position, player.transform.eulerAngles.y);

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        public static void teleportToLocationUnsafe(this Player player, Vector3 position) =>
            player.teleportToLocationUnsafe(position, player.transform.eulerAngles.y);
    }
}