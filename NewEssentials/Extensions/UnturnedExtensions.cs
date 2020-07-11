using Cysharp.Threading.Tasks;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Extensions
{
    public static class UnturnedExtensions
    {
        public static async UniTask MaxAllSkillsAsync(this PlayerSkills playerSkills, bool kunii = false)
        {
            await UniTask.SwitchToMainThread();
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

                // No achievements lol
                playerSkills.channel.send("tellSkills", playerSkills.channel.owner.playerID.steamID, ESteamPacket.UPDATE_RELIABLE_BUFFER, speciality, newLevels);
            }
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
            {
                item.removeItem(b);
            }
        }

        public static async UniTask<bool> TeleportToLocationAsync(this Player player, Vector3 position)
        {
            await UniTask.SwitchToMainThread();
            return player.teleportToLocation(position, player.transform.eulerAngles.y);
        }

        public static async UniTask TeleportToLocationUnsafeAsync(this Player player, Vector3 position)
        {
            await UniTask.SwitchToMainThread();
            player.teleportToLocationUnsafe(position, player.transform.eulerAngles.y);
        }
    }
}