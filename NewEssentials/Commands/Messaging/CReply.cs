using System;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.API;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Core.Users;
using SDG.Unturned;
using UnityEngine;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.Messaging
{
    [UsedImplicitly]
    [Command("reply")]
    [CommandAlias("r")]
    [CommandDescription("Reply to the last user to private message you")]
    [CommandSyntax("<mesage>")]
    public class CReply : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IPrivateMessageManager m_PrivateMessageManager;

        public CReply(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer,
            IPrivateMessageManager privateMessageManager, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
            m_PrivateMessageManager = privateMessageManager;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.reply";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length < 1)
                throw new CommandWrongUsageException(Context);

            ulong originalRecipientSteamID = 1337;
            if (Context.Actor.Type == KnownActorTypes.Player)
                originalRecipientSteamID = ulong.Parse(Context.Actor.Id);

            ulong? lastMessagerSteamID = m_PrivateMessageManager.GetLastMessager(originalRecipientSteamID);
            if (!lastMessagerSteamID.HasValue)
                throw new UserFriendlyException(m_StringLocalizer["reply:lonely"]);

            //TODO: Remove disconnected users from manager
            SteamPlayer lastMessager = PlayerTool.getSteamPlayer(lastMessagerSteamID.Value);
            if (lastMessager == null)
                throw new UserFriendlyException(m_StringLocalizer["reply:disconnected",
                    new {Messager = lastMessagerSteamID.Value.ToString()}]);
            
            m_PrivateMessageManager.RecordLastMessager(lastMessagerSteamID.Value, originalRecipientSteamID);
            
            StringBuilder messageBuilder = new StringBuilder();
            for(int i = 0; i < Context.Parameters.Length; i++)
                messageBuilder.Append(await Context.Parameters.GetAsync<string>(i) + " ");

            string completeMessage = messageBuilder.ToString();
            await UniTask.SwitchToMainThread();
            
            await Context.Actor.PrintMessageAsync(m_StringLocalizer["tell:sent",
                new {Recipient = lastMessager.playerID.characterName, Message = completeMessage}]);
            
            ChatManager.serverSendMessage(
                m_StringLocalizer["tell:received", new {Sender = Context.Actor.DisplayName, Message = completeMessage}],
                Color.white,
                toPlayer: lastMessager);
        }
    }
}