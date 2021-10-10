using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Players;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Commands
{
    [Command("god")]
    [CommandDescription("Become invincible")]
    [CommandSyntax("[player]")]
    public class CGod : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IGodManager m_GodManager;

        public CGod(IStringLocalizer stringLocalizer, IGodManager godManager, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_GodManager = godManager;
        }
        
        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            bool isGod;
            if (Context.Parameters.Length == 0)
            {
                if (!(Context.Actor is UnturnedUser uPlayer))
                    throw new CommandWrongUsageException(Context);

                isGod = m_GodManager.ToggleGod(uPlayer.SteamId.m_SteamID);

                if (isGod)
                    await uPlayer.PrintMessageAsync(m_StringLocalizer["god:god"]);
                else
                    await uPlayer.PrintMessageAsync(m_StringLocalizer["god:mortal"]);

                return;
            }

            string searchTerm = Context.Parameters[0];
            if (!PlayerTool.tryGetSteamPlayer(searchTerm, out SteamPlayer target))
                throw new UserFriendlyException(m_StringLocalizer["general:invalid_player", new {Player = searchTerm}]);

            await UniTask.SwitchToMainThread();
            
            isGod = m_GodManager.ToggleGod(target.playerID.steamID.m_SteamID);
            if (isGod)
            {
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["god:god_other",
                    new {Player = target.playerID.characterName}]);
                
                ChatManager.serverSendMessage(m_StringLocalizer["god:god"], Color.white, toPlayer: target, useRichTextFormatting: true);
            }
            else
            {
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["god:mortal_other",
                    new {Player = target.playerID.characterName}]);
                
                ChatManager.serverSendMessage(m_StringLocalizer["god:mortal"], Color.white, toPlayer: target, useRichTextFormatting: true);
            }
        }
    }
}