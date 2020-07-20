using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Players;
using NewEssentials.Extensions;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Commands.TPA
{
    [Command("tpaccept")]
    [CommandAlias("tpac")]
    [CommandDescription("Accept a teleport request")]
    [CommandSyntax("[player]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CTPAccept : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ITeleportRequestManager m_TpaRequestManager;

        public CTPAccept(IStringLocalizer stringLocalizer, ITeleportRequestManager tpaRequestManager, IServiceProvider serviceProvider) :
            base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_TpaRequestManager = tpaRequestManager;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser)Context.Actor;
            ulong recipientSteamID = uPlayer.SteamId.m_SteamID;

            SteamPlayer requester;
            switch (Context.Parameters.Length)
            {
                case 0:
                    if (!m_TpaRequestManager.IsRequestOpen(recipientSteamID))
                        throw new UserFriendlyException(m_StringLocalizer["tpa:no_requests"]);

                    ulong firstRequester = m_TpaRequestManager.AcceptRequest(recipientSteamID);
                    requester = PlayerTool.getSteamPlayer(firstRequester);

                    if (requester == null)
                        throw new UserFriendlyException(m_StringLocalizer["tpa:disconnected",
                            new { Requester = firstRequester.ToString() }]);
                    break;
                case 1:
                    string requesterName = Context.Parameters[0];
                    if (!PlayerTool.tryGetSteamPlayer(requesterName, out requester))
                        throw new UserFriendlyException(m_StringLocalizer["tpa:invalid_recipient",
                            new { Recipient = requesterName }]);

                    if (!m_TpaRequestManager.IsRequestOpen(recipientSteamID, requester.playerID.steamID.m_SteamID))
                        throw new UserFriendlyException(m_StringLocalizer["tpa:no_requests_from",
                            new { Requester = requester.playerID.characterName }]);
                    break;
                default:
                    throw new UserFriendlyException("This is a placeholder so that we can reassure the compiler that requester will never be null.");
            }

            await requester.player.TeleportToLocationAsync(uPlayer.Player.transform.position);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["tpa:accepted_self",
                new { Requester = requester.playerID.characterName }]);

            ChatManager.serverSendMessage(m_StringLocalizer["tpa:accepted_other", new { Recipient = uPlayer.DisplayName }], Palette.SERVER, toPlayer: requester, useRichTextFormatting: true);
        }
    }
}