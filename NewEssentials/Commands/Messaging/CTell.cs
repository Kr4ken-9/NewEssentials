using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.Messaging
{
    [Command("tell")]
    [CommandAlias("pm")]
    [CommandDescription("Send a player a private message")]
    [CommandSyntax("<player> <message>")]
    public class CTell : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly UnturnedUserDirectory m_UnturnedUserDirectory;

        public CTell(IStringLocalizer stringLocalizer,
            UnturnedUserDirectory unturnedUserDirectory,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UnturnedUserDirectory = unturnedUserDirectory;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length < 2)
                throw new CommandWrongUsageException(Context);

            string recipientName = Context.Parameters[0];
            UnturnedUser recipient = m_UnturnedUserDirectory.FindUser(recipientName, UserSearchMode.FindByName);

            if (recipient == null)
                throw new UserFriendlyException(m_StringLocalizer["tell:invalid_recipient", new {Recipient = recipientName}]);

            recipient.Session.SessionData["lastMessager"] = Context.Actor.Type == KnownActorTypes.Player
                ? Context.Actor.Id
                : Context.Actor.Type;
            var message = string.Join(" ", Context.Parameters);

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["tell:sent",
                new { Recipient = recipient.DisplayName, Message = message }]);

            await recipient.PrintMessageAsync(m_StringLocalizer["tell:received", new {Sender = Context.Actor.DisplayName, Message = message}]);
        }
    }
}