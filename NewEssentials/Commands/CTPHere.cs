using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;
using NewEssentials.API.User;
using NewEssentials.Unturned;
using OpenMod.UnityEngine.Extensions;

namespace NewEssentials.Commands
{
    [Command("tphere")]
    [CommandDescription("Tp a player to you")]
    [CommandSyntax("<player>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CTPHere : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserParser m_UserParser;

        public CTPHere(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer, IUserParser userParser) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);

            var searchTerm = Context.Parameters[0];
            var user = await m_UserParser.ParseUserAsync(searchTerm);

            if (user == null) 
                throw new UserFriendlyException(m_StringLocalizer["general:invalid_player", new { Player = searchTerm }]);

            var callingPlayer = (UnturnedUser) Context.Actor;

            await UniTask.SwitchToMainThread();
            await user.Player.Player.TeleportToLocationAsync(callingPlayer.Player.Transform.Position.ToUnityVector());

            await user.PrintMessageAsync(m_StringLocalizer["tphere:successful_tp", new {Player = user.DisplayName}]);
            await user.PrintMessageAsync(m_StringLocalizer["tphere:successful_tp_other", new {Player = user.DisplayName}]);
        }
    }
}
