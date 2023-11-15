using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NewEssentials.Network;
using NewEssentials.User;
using OpenMod.API.Eventing;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Unturned.Players.Life.Events;
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Unturned;

namespace NewEssentials.Events;

public class UnturnedPlayerSpawnedEventListener : IEventListener<UnturnedPlayerSpawnedEvent>
{
    #region Static Reflection
    private static readonly FieldInfo SkillsInfo;

    static UnturnedPlayerSpawnedEventListener()
    {
        SkillsInfo = typeof(PlayerSkills).GetField("_skills", BindingFlags.Instance | BindingFlags.NonPublic);
    }
    #endregion

    private readonly IUserDataStore m_UserDataStore;

    public UnturnedPlayerSpawnedEventListener(IUserDataStore userDataStore)
    {
        m_UserDataStore = userDataStore;
    }

    public async Task HandleEventAsync(object sender, UnturnedPlayerSpawnedEvent @event)
    {
        await UniTask.SwitchToMainThread();
        UserData data = await m_UserDataStore.GetUserDataAsync(@event.Player.EntityInstanceId, KnownActorTypes.Player);
        if (data?.Data == null || !data.Data.ContainsKey("skillSet"))
            return; //implicitly does not have the permissions or capacity for keep skills
        Skill[][] skillSet = (Skill[][]) data.Data["skillSet"];
        SkillsInfo.SetValue(@event.Player.Player.skills.skills, data.Data["skillSet"]);
        NetworkMethods.SendMultipleSkillLevels.Invoke(@event.Player.Player.skills.GetNetId(),
            ENetReliability.Reliable,
            @event.Player.Player.channel.GetOwnerTransportConnection(),
            writer => WriteSkillLevels(writer, skillSet));
    }
    
    //Reimplementation
    private static void WriteSkillLevels(NetPakWriter writer, IReadOnlyList<Skill[]> skills)
    {
        for (int index = 0; index < skills.Count; ++index)
        {
            foreach (Skill skill in skills[index])
                writer.WriteUInt8(skill.level);
        }
    }
}