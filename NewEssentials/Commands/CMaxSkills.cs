using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands
{
    [UsedImplicitly]
    [Command("maxskills")]
    [CommandDescription("Gives a player max skills")]
    public class CMaxSkills : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CMaxSkills(IPermissionChecker permissionChecker,
            IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            await UniTask.SwitchToMainThread();
            if (Context.Actor is ConsoleActor)
            {
                throw new CommandWrongUsageException(m_StringLocalizer["commands:playeronly"]);
            }

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, "newess.maxskills") ==
                PermissionGrantResult.Deny)
            {
                throw new NotEnoughPermissionException("newess.maxskills", m_StringLocalizer);
            }

            PlayerSkills playerSkills = uPlayer.Player.skills;

            for (byte speciality = 0; speciality < playerSkills.skills.Length; speciality++)
            {
                Skill[] skills = playerSkills.skills[speciality];
                byte[] newLevels = new byte[skills.Length];

                for (byte index = 0; index < skills.Length; index++)
                {
                    Skill skill = skills[index];

                    if (skill.level != skill.max)
                        skill.setLevelToMax();

                    newLevels[index] = skill.level;
                }
                
                // No achievements lol
                playerSkills.channel.send("tellSkills", uPlayer.SteamId, ESteamPacket.UPDATE_RELIABLE_BUFFER, speciality, newLevels);
            }

            await uPlayer.PrintMessageAsync(m_StringLocalizer["maxskills:granted"]);
        }
    }
}