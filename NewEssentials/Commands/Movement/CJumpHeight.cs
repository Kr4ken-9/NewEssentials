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
    [Command("jumpheight")]
    [CommandAlias("jh")]
    [CommandSyntax("[user] <jumpHeight>")]
    [CommandDescription("Change your or other player jump height. 0 = Freeze, 1 = Normal")]
    [RegisterCommandPermission("other", Description = "Allows to change jump height of other player")]
    public class CJumpHeight : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserParser m_UserParser;

        public CJumpHeight(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer, IUserParser userParser) : base(serviceProvider)
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
                throw new NotEnoughPermissionException(Context, "other");

            if (targetActor == null)
                return;
            
            var jump = await Context.Parameters.GetAsync<float>(Context.Parameters.Count - 1);

            await UniTask.SwitchToMainThread();
            targetActor.Player.Player.movement.sendPluginJumpMultiplier(jump);

            if (Context.Actor != targetActor)
            {
                await PrintAsync(m_StringLocalizer["jumpheight:instigator", new
                {
                    User = targetActor,
                    Jump = jump
                }]);
            }
            await targetActor.PrintMessageAsync(m_StringLocalizer["jumpheight:target", new { Jump = jump }]);
        }
    }
}
