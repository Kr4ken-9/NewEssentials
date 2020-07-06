using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.Home
{
    [UsedImplicitly]
    [Command("homes")]
    [CommandDescription("List your saved homes")]
    [CommandActor(typeof(UnturnedUser))]
    public class CHomes : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserDataStore m_UserDataStore;

        public CHomes(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer,
            IUserDataStore userDataStore, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
            m_UserDataStore = userDataStore;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.home.list";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            UserData userData = await m_UserDataStore.GetUserDataAsync(uPlayer.Id, uPlayer.Type);
            
            if (!userData.Data.ContainsKey("homes"))
                throw new UserFriendlyException(m_StringLocalizer["home:no_home"]);
            
            var stringBuilder = new StringBuilder();
            foreach (var pair in (Dictionary<object, object>) userData.Data["homes"])
                stringBuilder.Append($"{pair.Key}, ");

            stringBuilder.Remove(stringBuilder.Length - 2, 1);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["home:list", new {Homes = stringBuilder.ToString()}]);
        }
    }
}