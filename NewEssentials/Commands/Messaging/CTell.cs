using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.API;
using OpenMod.API.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Commands.Messaging
{
    [Command("tell")]
    [CommandAlias("pm")]
    [CommandDescription("Send a player a private message")]
    [CommandSyntax("<player> <message>")]
    public class CTell : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IPrivateMessageStore m_PrivateMessageManager;

        public CTell(IStringLocalizer stringLocalizer, IPrivateMessageStore privateMessageManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_PrivateMessageManager = privateMessageManager;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length < 2)
                throw new CommandWrongUsageException(Context);

            string recipientName = Context.Parameters[0];
            if (!PlayerTool.tryGetSteamPlayer(recipientName, out SteamPlayer recipient))
                throw new UserFriendlyException(m_StringLocalizer["tell:invalid_recipient",
                    new { Recipient = recipientName }]);

            ulong senderSteamID = 1337;
            if (Context.Actor.Type == KnownActorTypes.Player)
                senderSteamID = ulong.Parse(Context.Actor.Id);

            m_PrivateMessageManager.RecordLastMessager(recipient.playerID.steamID.m_SteamID, senderSteamID);
            var message = string.Join(" ", Context.Parameters);

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["tell:sent",
                new { Recipient = recipient.playerID.characterName, Message = message }]);

            await UniTask.SwitchToMainThread();
            ChatManager.serverSendMessage(
                m_StringLocalizer["tell:received", new { Sender = Context.Actor.DisplayName, Message = message }],
                Color.white,
                toPlayer: recipient);
        }
    }
}