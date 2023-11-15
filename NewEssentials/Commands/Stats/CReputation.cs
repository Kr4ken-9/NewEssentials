using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.API.User;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Commands.Stats
{
    [Command("reputation")]
    [CommandAlias("rep")]
    [CommandDescription("Give yourself or another player reputation (can be negative)")]
    [CommandSyntax("<amount> [player]")]
    //[RegisterCommandPermission("give", Description = "Give reputation to players")]
    public class CReputation : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserParser m_UserParser;

        public CReputation(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider, IUserParser userParser) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
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
                uPlayer.Player.Player.skills.askRep(additionReputation);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["reputation:success", new {Reputation = additionReputation.ToString()}]);
            }
            else
            {
                string nestedPermission = "give";
                if (await CheckPermissionAsync(nestedPermission) == PermissionGrantResult.Deny)
                    throw new NotEnoughPermissionException(Context, nestedPermission);
                
                string searchTerm = Context.Parameters[1];
                UnturnedUser usr = await m_UserParser.ParseUserAsync(searchTerm);
                if (usr == null)
                    throw new UserFriendlyException(m_StringLocalizer["general:invalid_player",
                        new {Player = searchTerm}]);
                SteamPlayer player = usr.Player.SteamPlayer;

                await UniTask.SwitchToMainThread();
                player.player.skills.askRep(additionReputation);
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["reputation:gave",
                    new
                    {
                        Player = player.playerID.characterName, Reputation = additionReputation.ToString()
                    }]);
            }
        }
    }
}