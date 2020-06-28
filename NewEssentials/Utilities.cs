using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using SDG.Unturned;

namespace NewEssentials
{
    public static class Utilities
    {
        public static void MaxAllSkills(this PlayerSkills playerSkills, bool kunii = false)
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
                
                // No achievements lol
                playerSkills.channel.send("tellSkills", playerSkills.channel.owner.playerID.steamID, ESteamPacket.UPDATE_RELIABLE_BUFFER, speciality, newLevels);
            }
        }

        public static bool GetItem(string searchTerm, out ItemAsset item)
        {
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