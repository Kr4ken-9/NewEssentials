using System;
using System.Drawing;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Players;
using NewEssentials.Extensions;
using NewEssentials.Options;
using OpenMod.API.Commands;
using OpenMod.API.Plugins;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.TPA
{
    [Command("tpaccept")]
    [CommandAlias("tpac")]
    [CommandDescription("Accept a teleport request")]
    [CommandSyntax("[player]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CTPAccept : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly UnturnedUserDirectory m_UserDirectory;
        private readonly ITeleportRequestManager m_TpaRequestManager;
        private readonly IConfiguration m_Configuration;
        private readonly IPluginAccessor<NewEssentials> m_PluginAccessor;
        private readonly ITeleportService m_TeleportService;

        public CTPAccept(IStringLocalizer stringLocalizer,
            UnturnedUserDirectory userDirectory,
            ITeleportRequestManager tpaRequestManager,
            IConfiguration configuration,
            IPluginAccessor<NewEssentials> pluginAccessor,
            ITeleportService teleportService,
            IServiceProvider serviceProvider) :
            base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserDirectory = userDirectory;
            m_TpaRequestManager = tpaRequestManager;
            m_Configuration = configuration;
            m_PluginAccessor = pluginAccessor;
            m_TeleportService = teleportService;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser)Context.Actor;
            ulong recipientSteamID = uPlayer.SteamId.m_SteamID;

            UnturnedUser requester;
            switch (Context.Parameters.Length)
            {
                case 0:
                    if (!m_TpaRequestManager.IsRequestOpen(recipientSteamID))
                        throw new UserFriendlyException(m_StringLocalizer["tpa:accept:no_requests"]);

                    ulong firstRequester = m_TpaRequestManager.AcceptRequest(recipientSteamID);
                    requester = m_UserDirectory.FindUser(firstRequester.ToString(), UserSearchMode.FindById);

                    if (requester == null)
                        throw new UserFriendlyException(m_StringLocalizer["tpa:accept:disconnected",
                            new {Requester = firstRequester.ToString()}]);

                    break;
                case 1:
                    string requesterName = Context.Parameters[0];
                    requester = m_UserDirectory.FindUser(requesterName, UserSearchMode.FindByName);
                    
                    if (requester == null)
                        throw new UserFriendlyException(m_StringLocalizer["tpa:invalid_recipient", new { Recipient = requesterName }]);

                    if (!m_TpaRequestManager.IsRequestOpen(recipientSteamID, requester.SteamId.m_SteamID))
                        throw new UserFriendlyException(m_StringLocalizer["tpa:accept:no_requests_from",
                            new { Requester = requester.DisplayName }]);
                    break;
                default:
                    throw new UserFriendlyException("This is a placeholder so that we can reassure the compiler that requester will never be null.");
            }

            int delay = m_Configuration.GetValue<int>("teleportation:delay");
            bool cancelOnMove = m_Configuration.GetValue<bool>("teleportation:cancelOnMove");
            bool cancelOnDamage = m_Configuration.GetValue<bool>("teleportation:cancelOnDamage");
            
            await uPlayer.PrintMessageAsync(m_StringLocalizer["tpa:accept:accepted_self", new { Requester = requester.DisplayName, Time = delay }]);

            requester.PrintMessageAsync(m_StringLocalizer["tpa:accept:accepted_other", new {Recipient = uPlayer.DisplayName, Time = delay}]); 
            
            bool successful = await m_TeleportService.TeleportAsync(requester,
                new TeleportOptions(m_PluginAccessor.Instance, delay, cancelOnMove, cancelOnDamage));

            if (!successful)
            {
                requester.PrintMessageAsync(m_StringLocalizer["teleport:canceled"], Color.DarkRed);
                throw new UserFriendlyException(m_StringLocalizer["teleport:canceled"]);
            }

            await requester.Player.Player.TeleportToLocationAsync(uPlayer.Player.Player.transform.position);
        }
    }
}