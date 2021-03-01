using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.Home
{
    [Command("delete")]
    [CommandAlias("-")]
    [CommandAlias("remove")]
    [CommandDescription("Delete a saved home for yourself or another user")]
    [CommandSyntax("<name> [user]")]
    [CommandParent(typeof(CHome))]
    public class CDeleteHome : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserDataStore m_UserDataStore;
        private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;

        public CDeleteHome(IStringLocalizer stringLocalizer,
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
            if (Context.Parameters.Length is < 1 || Context.Parameters.Length is > 2)
                throw new CommandWrongUsageException(Context);
            
            if (Context.Parameters.Length == 1 && Context.Actor.Type != KnownActorTypes.Player)
                throw new CommandWrongUsageException(Context);
            
            if (Context.Parameters.Length == 2 && await CheckPermissionAsync("others") == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, "others");

            UnturnedUser uPlayer = Context.Parameters.Length == 1
                ? (UnturnedUser) Context.Actor
                : m_UnturnedUserDirectory.FindUser(Context.Parameters[0], UserSearchMode.FindByName);
            
            if (uPlayer == null)
                throw new UserFriendlyException(m_StringLocalizer["commands:failed_player", new {Player = Context.Parameters[0]}]);
            
            UserData userData = await m_UserDataStore.GetUserDataAsync(uPlayer.Id, uPlayer.Type);
            
            if (!userData.Data.ContainsKey("homes"))
                throw new UserFriendlyException(m_StringLocalizer["home:no_home"]);

            var homes = (Dictionary<object, object>) userData.Data["homes"];
            string homeName = Context.Parameters[0];

            if (!homes.ContainsKey(homeName))
                throw new UserFriendlyException(m_StringLocalizer["home:invalid_home", new {Home = homeName}]);

            homes.Remove(homeName);
            userData.Data["homes"] = homes;

            await m_UserDataStore.SetUserDataAsync(userData);
            await uPlayer.PrintMessageAsync(m_StringLocalizer["home:delete", new {Home = homeName}]);
        }
    }
}