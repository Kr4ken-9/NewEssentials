using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Players;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Commands.TPA
{
    [Command("tpa")]
    [CommandDescription("Send a teleport request to another player")]
    [CommandSyntax("<player>")]
    [CommandActor(typeof(UnturnedUser))]
    public class CTPA : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfiguration m_Configuration;
        private readonly ITeleportRequestManager m_TpaRequestManager;

        public CTPA(IStringLocalizer stringLocalizer, IConfiguration configuration, ITeleportRequestManager tpaRequestManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
            m_TpaRequestManager = tpaRequestManager;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            string recipientName = Context.Parameters[0];

            // TODO: Switch to UnturnedUserDirectory
            if (!PlayerTool.tryGetSteamPlayer(recipientName, out SteamPlayer recipient)){
                if(recipientName == "a") { //tpa a? I mean you could theoretically just make the player execute "/tpaccept" instead, but oh well fair enough.
                    ulong requesterSteamID = uPlayer.SteamId.m_SteamID;
                    if (m_TpaRequestManager.IsRequestOpen(requesterSteamID)) {
                        ulong firstRequester = m_TpaRequestManager.AcceptRequest(requesterSteamID);
                        UnturnedUser requester = m_UserDirectory.FindUser(firstRequester.ToString(), UserSearchMode.FindById);
                        if(requester != null) {
                            int delay = m_Configuration.GetValue<int>("teleportation:delay");
                            bool cancelOnMove = m_Configuration.GetValue<bool>("teleportation:cancelOnMove");
                            bool cancelOnDamage = m_Configuration.GetValue<bool>("teleportation:cancelOnDamage");
                            await uPlayer.PrintMessageAsync(m_StringLocalizer["tpa:accept:accepted_self", new { Requester = requester.DisplayName, Time = delay }]);
                            requester.PrintMessageAsync(m_StringLocalizer["tpa:accept:accepted_other", new {Recipient = uPlayer.DisplayName, Time = delay}]);
                            bool successful = await m_TeleportService.TeleportAsync(requester,new TeleportOptions(m_PluginAccessor.Instance, delay, cancelOnMove, cancelOnDamage));
                            if (!successful){
                                requester.PrintMessageAsync(m_StringLocalizer["teleport:canceled"], Color.DarkRed);
                                throw new UserFriendlyException(m_StringLocalizer["teleport:canceled"]);
                            }
                            await requester.Player.Player.TeleportToLocationAsync(uPlayer.Player.Player.transform.position);
                        }
                    }
                } // a draft! This may not be 100% correct, but well they'll probably add spacesupported alias in OpenMod soon anyway
                throw new UserFriendlyException(m_StringLocalizer["tpa:invalid_recipient", new {Recipient = recipientName}]);
            }

            ulong requesterSteamID = uPlayer.SteamId.m_SteamID;
            ulong recipientSteamID = recipient.playerID.steamID.m_SteamID;

            if (requesterSteamID == recipientSteamID)
                throw new UserFriendlyException(m_StringLocalizer["tpa:self"]);

            if (m_TpaRequestManager.IsRequestOpen(recipientSteamID, requesterSteamID))
                throw new UserFriendlyException(m_StringLocalizer["already_requested"]);

            int requestLifetime = m_Configuration.GetValue<int>("tpa:expiration") * 1000;
            
            m_TpaRequestManager.OpenNewRequest(recipientSteamID, requesterSteamID, requestLifetime);

            await UniTask.SwitchToMainThread();
            await uPlayer.PrintMessageAsync(m_StringLocalizer["tpa:success", new {Recipient = recipient.playerID.characterName}]);
            ChatManager.serverSendMessage(m_StringLocalizer["tpa:delivered", new {Requester = uPlayer.DisplayName}], Color.white, toPlayer: recipient);
        }
    }
}
