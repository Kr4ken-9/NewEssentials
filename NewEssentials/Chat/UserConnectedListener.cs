using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Users.Events;

namespace NewEssentials.Chat
{
    public class UserConnectedListener : IEventListener<UnturnedUserConnectedEvent>
    {
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly UnturnedUserDirectory m_UnturnedUserDirectory;
        private readonly bool m_JoinMessages;

        public UserConnectedListener(IConfiguration configuration,
            IStringLocalizer stringLocalizer,
            UnturnedUserDirectory unturnedUserDirectory)
        {
            m_Configuration = configuration;
            m_StringLocalizer = stringLocalizer;
            m_UnturnedUserDirectory = unturnedUserDirectory;

            m_JoinMessages = m_Configuration.GetValue<bool>("connections:joinMessages");
        }
        
        public async Task HandleEventAsync(object? sender, UnturnedUserConnectedEvent @event)
        {
            if (!m_JoinMessages)
                return;

            string userName = @event.User.DisplayName;
            
            Parallel.ForEach(m_UnturnedUserDirectory.GetOnlineUsers(),
                x => x.PrintMessageAsync(m_StringLocalizer["connections:connected", new {User = userName}]));
        }
    }
}