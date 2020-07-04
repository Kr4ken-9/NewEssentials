using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.API;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.TPA
{
    [UsedImplicitly]
    [Command("tpaccept")]
    [CommandAlias("tpac")]
    [CommandDescription("Accepts a teleport request")]
    [CommandSyntax("[player]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CTPAccept : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ITPAManager m_TpaManager;

        public CTPAccept(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, ITPAManager tpaManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
            m_TpaManager = tpaManager;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.tpa.accept";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            ulong recipientSteamID = uPlayer.SteamId.m_SteamID;

            SteamPlayer requester;
            switch (Context.Parameters.Length)
            {
                case 0:
                    if (!m_TpaManager.IsRequestOpen(recipientSteamID))
                        throw new UserFriendlyException(m_StringLocalizer["tpa:no_requests"]);

                    ulong firstRequester = m_TpaManager.AcceptRequest(recipientSteamID);
                    requester = PlayerTool.getSteamPlayer(firstRequester);

                    if (requester == null)
                        throw new UserFriendlyException(m_StringLocalizer["tpa:disconnected",
                            new {Requester = firstRequester.ToString()}]);
                    break;
                case 1:
                    string requesterName = Context.Parameters[0];
                    if (!PlayerTool.tryGetSteamPlayer(requesterName, out requester))
                        throw new UserFriendlyException(m_StringLocalizer["tpa:invalid_recipient",
                            new {Recipient = requesterName}]);

                    if (!m_TpaManager.IsRequestOpen(recipientSteamID, requester.playerID.steamID.m_SteamID))
                        throw new UserFriendlyException(m_StringLocalizer["tpa:no_requests_from",
                            new {Requester = requester.playerID.characterName}]);
                    break;
                default:
                    throw new UserFriendlyException("This is a placeholder so that we can reassure the compiler that requester will never be null.");
            }
            
            await UniTask.SwitchToMainThread();
            requester.player.teleportToLocation(uPlayer.Player.transform.position, requester.player.look.yaw);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["tpa:accepted",
                new {Requester = requester.playerID.characterName}]);

            ChatManager.serverSendMessage(m_StringLocalizer["tpa:granted",
                new {Recipient = uPlayer.DisplayName}], Palette.SERVER, toPlayer: requester);
        }
    }
}