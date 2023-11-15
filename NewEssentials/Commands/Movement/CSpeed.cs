using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Core.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;
using NewEssentials.API.User;

namespace NewEssentials.Commands.Movement
{
    [Command("speed")]
    [CommandDescription("Change your or other player movement speed. 0 = Freeze, 1 = Normal")]
    [CommandSyntax("[user] <speed>")]
    [RegisterCommandPermission("other", Description = "Allows to change speed of other player")]
    public class CSpeed : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserParser m_UserParser;

        public CSpeed(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer, IUserParser userParser) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var targetActor = Context.Parameters.Count == 2
                ? await m_UserParser.ParseUserAsync(Context.Parameters[0])
                : Context.Actor as UnturnedUser;

            if (targetActor != Context.Actor && await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
            {
                throw new NotEnoughPermissionException(Context, "other");
            }

            if (targetActor == null)
            {
                return;
            }

            var speed = await Context.Parameters.GetAsync<float>(Context.Parameters.Count - 1);

            await UniTask.SwitchToMainThread();
            targetActor.Player.Player.movement.sendPluginSpeedMultiplier(speed);

            if (Context.Actor != targetActor)
            {
                await PrintAsync(m_StringLocalizer["speed:instigator", new
                {
                    User = targetActor,
                    Speed = speed
                }]);
            }
            await targetActor.PrintMessageAsync(m_StringLocalizer["speed:target", new { Speed = speed }]);
        }
    }
}
