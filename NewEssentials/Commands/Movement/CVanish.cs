using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Commands.Movement
{
    [Command("vanish")]
    [CommandDescription("Become invisible")]
    [CommandSyntax("[player]")]
    public class CVanish : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CVanish(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            bool vanished;
            PlayerLook look;
            PlayerMovement movement;
            if (Context.Parameters.Length == 0)
            {
                if (!(Context.Actor is UnturnedUser uPlayer))
                    throw new CommandWrongUsageException(Context);

                movement = uPlayer.Player.Player.movement;
                look = uPlayer.Player.Player.look;
                vanished = !movement.canAddSimulationResultsToUpdates;
                if (vanished)
                {
                    await uPlayer.PrintMessageAsync(m_StringLocalizer["vanish:unvanished"]);
                    movement.updates.Add(new PlayerStateUpdate(movement.real, look.angle, look.rot));
                }
                else
                {
                    await uPlayer.PrintMessageAsync(m_StringLocalizer["vanish:vanished"]);
                }

                uPlayer.Player.Player.movement.canAddSimulationResultsToUpdates = vanished;
                return;
            }

            string searchTerm = Context.Parameters[0];
            if (!PlayerTool.tryGetSteamPlayer(searchTerm, out SteamPlayer target))
                throw new UserFriendlyException(m_StringLocalizer["general:invalid_player", new {Player = searchTerm}]);

            await UniTask.SwitchToMainThread();
            
            string targetName = target.playerID.characterName;
            movement = target.player.movement;
            look = target.player.look;
            vanished = !movement.canAddSimulationResultsToUpdates;
            if (vanished)
            {
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["vanish:unvanished_other", new {Player = targetName}]);
                ChatManager.serverSendMessage(m_StringLocalizer["vanish:unvanished"], Color.white, toPlayer: target);
                movement.updates.Add(new PlayerStateUpdate(movement.real, look.angle, look.rot));
            }
            else
            {
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["vanish:vanished_other", new {Player = targetName}]);
                ChatManager.serverSendMessage(m_StringLocalizer["vanish:vanished"], Color.white, toPlayer: target);
            }

            target.player.movement.canAddSimulationResultsToUpdates = vanished;
        }
    }
}