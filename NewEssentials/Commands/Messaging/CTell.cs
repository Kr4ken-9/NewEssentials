using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
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

        public CTell(IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length < 2)
                throw new CommandWrongUsageException(Context);

            string recipientName = Context.Parameters[0];
            var recipient = await Context.Parameters.GetAsync<UnturnedUser>(0);

            if (recipient == null)
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
