using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NewEssentials.API;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Commands.Messaging
{
    [Command("reply")]
    [CommandAlias("r")]
    [CommandDescription("Reply to the last user to private message you")]
    [CommandSyntax("<mesage>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CReply : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;
        private readonly ILogger<NewEssentials> m_Logger;

        public CReply(IStringLocalizer stringLocalizer,
            IUnturnedUserDirectory unturnedUserDirectory,
            ILogger<NewEssentials> logger,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UnturnedUserDirectory = unturnedUserDirectory;
            m_Logger = logger;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1)
                throw new CommandWrongUsageException(Context);

            var uPlayer = Context.Actor as UnturnedUser;

            if (!uPlayer.Session.SessionData.ContainsKey("lastMessager"))
                throw new UserFriendlyException(m_StringLocalizer["reply:lonely"]);

            string lastMessagerID = (string)uPlayer.Session.SessionData["lastMessager"];
            bool consoleMessage = lastMessagerID == "console";
            var message = string.Join(" ", Context.Parameters);

            if (!consoleMessage)
            {
                var lastMessager = m_UnturnedUserDirectory.FindUser(lastMessagerID, UserSearchMode.FindById);
                if (lastMessager == null)
                    throw new UserFriendlyException(m_StringLocalizer["reply:disconnected",
                        new {Messager = lastMessagerID}]);

                lastMessager.Session.SessionData["lastMessager"] = uPlayer.Id;
                
                await lastMessager.PrintMessageAsync(m_StringLocalizer["tell:received",
                    new {Sender = Context.Actor.DisplayName, Message = message}]);
                
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["tell:sent",
                    new { Recipient = lastMessager.DisplayName, Message = message }]);

                return;
            }
            
            await Context.Actor.PrintMessageAsync(m_StringLocalizer["tell:sent",
                new { Recipient = "Console", Message = message }]);

            m_Logger.LogInformation(m_StringLocalizer["tell:received",
                new {Sender = Context.Actor.DisplayName, Message = message}]);
        }
    }
}