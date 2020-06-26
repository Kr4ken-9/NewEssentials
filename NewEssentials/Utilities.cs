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
    }
}