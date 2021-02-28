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
        private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;

        public CHomes(IStringLocalizer stringLocalizer,
            IUserDataStore userDataStore,
            IUnturnedUserDirectory unturnedUserDirectory,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserDataStore = userDataStore;
            m_UnturnedUserDirectory = unturnedUserDirectory;
        }

        protected override async UniTask OnExecuteAsync()
        {
            switch (Context.Parameters.Length)
            {
                case > 1:
                    throw new CommandWrongUsageException(Context);
                case 0 when Context.Actor.Type != KnownActorTypes.Player:
                    throw new CommandWrongUsageException(Context);
                case 1 when await CheckPermissionAsync("others") == PermissionGrantResult.Deny:
                    throw new NotEnoughPermissionException(Context, "others");
            }

            UnturnedUser uPlayer = Context.Parameters.Length == 0
                ? (UnturnedUser) Context.Actor
                : m_UnturnedUserDirectory.FindUser(Context.Parameters[0], UserSearchMode.FindByName);
            
            UserData userData = await m_UserDataStore.GetUserDataAsync(uPlayer.Id, uPlayer.Type);

            if (!userData.Data.ContainsKey("homes"))
                throw new UserFriendlyException(m_StringLocalizer["home:no_home"]);

            var stringBuilder = new StringBuilder();
            foreach (var pair in (Dictionary<object, object>)userData.Data["homes"])
                stringBuilder.Append($"{pair.Key}, ");

            stringBuilder.Remove(stringBuilder.Length - 2, 1);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["home:list", new { User = uPlayer.DisplayName, Homes = stringBuilder.ToString() }]);
        }
    }
}