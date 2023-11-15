using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Players;
using NewEssentials.API.User;
using NewEssentials.Memory;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Commands.Movement
{
    [Command("freeze")]
    [CommandDescription("Freeze or unfreeze a player")]
    [CommandSyntax("<player>")]
    public class CFreeze : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IPlayerFreezer m_PlayerFreezer;
        private readonly IUserParser m_UserParser;

        public CFreeze(IStringLocalizer stringLocalizer, IPlayerFreezer playerFreezer,
            IServiceProvider serviceProvider, IUserParser userParser) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_PlayerFreezer = playerFreezer;
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);

            string searchTerm = Context.Parameters[0];
            ReferenceBoolean b = new ReferenceBoolean();
            UnturnedUser usr = await m_UserParser.TryParseUserAsync(searchTerm, b);
            if (!b)
                throw new UserFriendlyException(m_StringLocalizer["general:invalid_player", new {Player = searchTerm}]);
            SteamPlayer player = usr.Player.SteamPlayer;
            if (m_PlayerFreezer.IsPlayerFrozen(player))
            {
                m_PlayerFreezer.UnfreezePlayer(player);
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["freeze:unfrozen",
                    new {Player = player.playerID.characterName}]);
            }
            else
            {
                m_PlayerFreezer.FreezePlayer(player, player.player.transform.position);
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["freeze:frozen",
                    new {Player = player.playerID.characterName}]);
            }
        }
    }
}