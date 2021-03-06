using System;
using System.Threading.Tasks;
using NewEssentials.API.Players;
using NewEssentials.Options;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.Core.Helpers;
using OpenMod.Extensions.Games.Abstractions.Players;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Players
{
    [ServiceImplementation]
    public class TeleportService : ITeleportService
    {
        private readonly IEventBus m_EventBus;

        public TeleportService(IEventBus eventBus)
        {
            m_EventBus = eventBus;
        }
        
        public async Task<bool> TeleportAsync(UnturnedUser user, TeleportOptions options)
        {
            var cancelled = false;

            IDisposable sub = NullDisposable.Instance;
            if (options.CancelOnDamage)
            {
                sub = m_EventBus.Subscribe<IPlayerDamagedEvent>(options.Instance, (sp, sender, e) =>
                {
                    if (Equals(e.Player, user.Player))
                        cancelled = true;

                    return Task.CompletedTask;
                });
            }

            var lastMove = (TimeSpan) user.Session.SessionData["lastMovement"];
            
            await Task.Delay(new TimeSpan(0, 0, options.Delay));
            sub.Dispose();

            if (options.CancelOnMove)
            {
                var latestMove = (TimeSpan) user.Session.SessionData["lastMovement"];
                if (lastMove != latestMove)
                    cancelled = true;
            }

            return !cancelled;
        }
    }
}