using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Core.Users;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands
{
    [UsedImplicitly]
    [Command("reputation")]
    [CommandAlias("rep")]
    [CommandDescription("Give yourself or another player reputation (can be negative)")]
    [CommandSyntax("<amount> [player]")]
    public class CReputation : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CReputation(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.rep";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length > 2 || Context.Parameters.Length < 1)
                throw new CommandWrongUsageException(Context);

            //TODO: throw UserFriendlyException on bad input
            int additionReputation = await Context.Parameters.GetAsync<int>(0);
            if (Context.Parameters.Length == 1)
            {
                if (Context.Actor.Type == KnownActorTypes.Console)
                    throw new CommandWrongUsageException(Context);

                UnturnedUser uPlayer = (UnturnedUser) Context.Actor;

                await UniTask.SwitchToMainThread();
                uPlayer.Player.skills.askRep(additionReputation);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["reputation:success", new {Reputation = additionReputation.ToString()}]);
            }
            else
            {
                string nestedPermission = "newess.rep.give";
                if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, nestedPermission) == PermissionGrantResult.Deny)
                    throw new NotEnoughPermissionException(Context, nestedPermission);
                
                string searchTerm = Context.Parameters[1];
                if (!PlayerTool.tryGetSteamPlayer(searchTerm, out SteamPlayer player))
                    throw new UserFriendlyException(m_StringLocalizer["general:invalid_player",
                        new {Player = searchTerm}]);

                await UniTask.SwitchToMainThread();
                player.player.skills.askRep(additionReputation);
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["reputation:gave",
                    new {Player = player.playerID.characterName, Reputation = additionReputation.ToString()}]);
            }
        }
    }
}