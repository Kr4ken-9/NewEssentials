using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.API.User;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Commands.Stats
{
    [Command("experience")]
    [CommandAlias("exp")]
    [CommandDescription("Give yourself or another player experience (must be positive)")]
    [CommandSyntax("<amount> [player]")]
    //[RegisterCommandPermission("give", Description = "Give experience to players")]
    public class CExperience : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserParser m_UserParser;

        public CExperience(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider, IUserParser userParser) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 2)
                throw new UserFriendlyException(m_StringLocalizer["experience:toomanyargs"]);
            if (Context.Parameters.Length < 1)
                throw new UserFriendlyException(m_StringLocalizer["experience:no_amount"]);
            
            uint additionalExperience = await Context.Parameters.GetAsync<uint>(0);
            if (Context.Parameters.Length == 1)
            {
                if (Context.Actor.Type == KnownActorTypes.Console)
                    throw new CommandWrongUsageException(Context);

                UnturnedUser uPlayer = (UnturnedUser) Context.Actor;

                await UniTask.SwitchToMainThread();
                uPlayer.Player.Player.skills.askAward(additionalExperience);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["experience:success", new {Experience = additionalExperience.ToString()}]);
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
                player.player.skills.askAward(additionalExperience);
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["experience:gave",
                    new {Player = player.playerID.characterName, Experience = additionalExperience.ToString()}]);
            }
        }
    }
}
