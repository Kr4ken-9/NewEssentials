using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace NewEssentials.Commands
{
    [Command("requesturl")]
    [CommandAlias("requrl")]
    [CommandDescription("Request player to open URL")]
    [CommandSyntax("<player> <url> [message]")]
    public class CRequestUrl : UnturnedCommand
    {
        public CRequestUrl(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters is { Count: < 2 or > 3 })
            {
                throw new CommandWrongUsageException(Context);
            }

            var user = await Context.Parameters.GetAsync<UnturnedUser>(0);
            var url = Context.Parameters[1];
            var message = Context.Parameters.Count >= 2 ? Context.Parameters[2] : string.Empty;

            await UniTask.SwitchToMainThread();

            user.Player.Player.sendBrowserRequest(message, url);
        }
    }
}
