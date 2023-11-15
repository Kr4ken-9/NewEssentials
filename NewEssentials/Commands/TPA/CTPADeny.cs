using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Players;
using NewEssentials.API.User;
using NewEssentials.Memory;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Commands.TPA
{
    [Command("tpadeny")]
    [CommandAlias("tpad")]
    [CommandDescription("Deny a teleport request")]
    [CommandSyntax("[player]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CTPADeny : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ITeleportRequestManager m_TpaRequestManager;
        private readonly IUserParser m_UserParser;

        public CTPADeny(IStringLocalizer stringLocalizer, ITeleportRequestManager tpaRequestManager, IServiceProvider serviceProvider, IUserParser userParser) :
            base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_TpaRequestManager = tpaRequestManager;
            m_UserParser = userParser;
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
                    ReferenceBoolean b = new ReferenceBoolean();
                    UnturnedUser usr = await m_UserParser.TryParseUserAsync(requesterName, b);
                    if (!b)
                        throw new UserFriendlyException(m_StringLocalizer["tpa:invalid_recipient",
                            new { Recipient = requesterName }]);
                    requester = usr.Player.SteamPlayer;
                    if (!m_TpaRequestManager.IsRequestOpen(recipientSteamID, requester.playerID.steamID.m_SteamID))
                        throw new UserFriendlyException(m_StringLocalizer["tpa:no_requests_from",
                            new { Requester = requester.playerID.characterName }]);
                    break;
                default:
                    throw new UserFriendlyException("This is a placeholder so that we can reassure the compiler that requester will never be null.");
            }

            //TODO: Change name to be less misleading.
            m_TpaRequestManager.AcceptRequest(recipientSteamID, requester.playerID.steamID.m_SteamID);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["tpa:denied_self",
                new { Requester = requester.playerID.characterName }]);

            await UniTask.SwitchToMainThread();
            ChatManager.serverSendMessage(m_StringLocalizer["tpa:denied_other",
                new { Recipient = uPlayer.DisplayName }], Color.red, toPlayer: requester, useRichTextFormatting: true);
        }
    }
}