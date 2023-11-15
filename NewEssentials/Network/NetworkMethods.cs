using SDG.Unturned;

namespace NewEssentials.Network;

public static class NetworkMethods
{
    public static ClientInstanceMethod SendMultipleSkillLevels { get; } = ClientInstanceMethod.Get(typeof(PlayerSkills), "ReceiveMultipleSkillLevels");
}