using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.API;
using OpenMod.API.Permissions;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using UnityEngine;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.TPA
{
    [UsedImplicitly]
    [Command("tpa")]
    [CommandDescription("Send a teleport request to another player")]
    [CommandSyntax("<player>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CTPA : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfiguration m_Configuration;
        private readonly ITPAManager m_TpaManager;

        public CTPA(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer,
            IConfiguration configuration, ITPAManager tpaManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
            m_TpaManager = tpaManager;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.tpa";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            string recipientName = Context.Parameters[0];

            if (!PlayerTool.tryGetSteamPlayer(recipientName, out SteamPlayer recipient))
                throw new UserFriendlyException(m_StringLocalizer["tpa:invalid_recipient", new {Recipient = recipientName}]);

            ulong requesterSteamID = uPlayer.SteamId.m_SteamID;
            ulong recipientSteamID = recipient.playerID.steamID.m_SteamID;
            
            //TODO: deny TPA requests to self

            if (m_TpaManager.IsRequestOpen(recipientSteamID, requesterSteamID))
                throw new UserFriendlyException(m_StringLocalizer["already_requested"]);

            int requestLifetime = m_Configuration.GetValue<int>("tpa:expiration") * 1000;
            
            m_TpaManager.OpenNewRequest(recipientSteamID, requesterSteamID, requestLifetime);

            await UniTask.SwitchToMainThread();
            await uPlayer.PrintMessageAsync(m_StringLocalizer["tpa:success", new {Recipient = recipient.playerID.characterName}]);
            ChatManager.serverSendMessage(m_StringLocalizer["tpa:delivered", new {Requester = uPlayer.DisplayName}], Color.white, toPlayer: recipient);
        }
    }
}