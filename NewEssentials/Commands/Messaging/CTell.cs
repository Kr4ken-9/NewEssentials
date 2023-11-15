using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.API.User;
using NewEssentials.System;
using OpenMod.API.Commands;
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
        private readonly IUserParser m_UserParser;

        public CTell(IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider, IUserParser userParser) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length < 2)
                throw new CommandWrongUsageException(Context);

            ReferenceBoolean b = new ReferenceBoolean();
            string recipientName = Context.Parameters[0];
            var recipient = await m_UserParser.TryParseUserAsync(recipientName, b);

            if (!b)
                throw new UserFriendlyException(m_StringLocalizer["tell:invalid_recipient", new {Recipient = recipientName}]);

            recipient.Session.SessionData["lastMessager"] = Context.Actor.Type == KnownActorTypes.Player
                ? Context.Actor.Id
                : Context.Actor.Type;
            var message = string.Join(" ", Context.Parameters.Skip(1));

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["tell:sent",
                new { Recipient = recipient.DisplayName, Message = message }]);

            await recipient.PrintMessageAsync(m_StringLocalizer["tell:received", new {Sender = Context.Actor.DisplayName, Message = message}]);
        }
    }
}
