using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using NewEssentials.Managers;
using OpenMod.API.Permissions;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.TPA
{
    [UsedImplicitly]
    [Command("tpa")]
    [CommandDescription("Send a teleport request to another player")]
    [CommandSyntax("<player>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CTPA : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserDataStore m_UserDataStore;
        private readonly TPAManager m_TpaManager;
        
        public CTPA(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer,
            IUserDataStore userDataStore, TPAManager tpaManager, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
            m_UserDataStore = userDataStore;
            m_TpaManager = tpaManager;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.tpa";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            string recipientName = Context.Parameters[0];

            if (!PlayerTool.tryGetSteamPlayer(recipientName, out SteamPlayer recipient))
                throw new UserFriendlyException(m_StringLocalizer["tpa:invalid_recipient", new {Recipient = recipientName}]);
            
            
        }
    }
}