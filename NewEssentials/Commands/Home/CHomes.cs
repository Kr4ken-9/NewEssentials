using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;
using System.Collections.Generic;
using System.Text;
using NewEssentials.API.User;
using OpenMod.API.Permissions;
using OpenMod.Core.Users;

namespace NewEssentials.Commands.Home
{
    [Command("homes")]
    [CommandDescription("List your or another user's saved homes")]
    [CommandSyntax("[user]")]
    public class CHomes : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserDataStore m_UserDataStore;
        private readonly IUserParser m_UserParser;

        public CHomes(IStringLocalizer stringLocalizer,
            IUserDataStore userDataStore,
            IServiceProvider serviceProvider, IUserParser userParser) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserDataStore = userDataStore;
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);
            
            if (Context.Parameters.Length == 0 && Context.Actor.Type != KnownActorTypes.Player)
                throw new CommandWrongUsageException(Context);
            
            if (Context.Parameters.Length == 1 && await CheckPermissionAsync("others") == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, "others");

            UnturnedUser uPlayer = Context.Parameters.Length == 0
                ? (UnturnedUser) Context.Actor
                : await m_UserParser.ParseUserAsync(Context.Parameters[0]);

            if (uPlayer == null)
                throw new UserFriendlyException(m_StringLocalizer["commands:failed_player", new {Player = Context.Parameters[0]}]);
            
            UserData userData = await m_UserDataStore.GetUserDataAsync(uPlayer.Id, uPlayer.Type);

            if (!userData.Data.ContainsKey("homes"))
                throw new UserFriendlyException(m_StringLocalizer["home:no_home"]);

            var homes = (Dictionary<object, object>) userData.Data["homes"];
            if (homes.Count == 0)
                throw new UserFriendlyException(m_StringLocalizer["home:no_home"]);

            var stringBuilder = new StringBuilder();
            foreach (var pair in homes)
                stringBuilder.Append($"{pair.Key}, ");

            stringBuilder.Remove(stringBuilder.Length - 2, 2);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["home:list", new { User = uPlayer.DisplayName, Homes = stringBuilder.ToString() }]);
        }
    }
}