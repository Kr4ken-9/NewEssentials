using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;
using NewEssentials.API.User;
using OpenMod.API.Commands;

namespace NewEssentials.Commands
{
    [Command("requesturl")]
    [CommandAlias("requrl")]
    [CommandDescription("Request player to open URL")]
    [CommandSyntax("<player> <url> [message]")]
    public class CRequestUrl : UnturnedCommand
    {

        private readonly IUserParser m_UserParser;
        public CRequestUrl(IServiceProvider serviceProvider, IUserParser userParser) : base(serviceProvider)
        {
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters is { Count: < 2 or > 3 })
                throw new CommandWrongUsageException(Context);
            
            var user = await m_UserParser.ParseUserAsync(Context.Parameters[0]);
            if (user == null)
            {
                throw new UserFriendlyException("Can't find that user!");
                return; //TODO: add localisations
            }
            var url = Context.Parameters[1];
            var message = Context.Parameters.Count >= 2 ? Context.Parameters[2] : string.Empty;

            await UniTask.SwitchToMainThread();

            user.Player.Player.sendBrowserRequest(message, url);
        }
    }
}
