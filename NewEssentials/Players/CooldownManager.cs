using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NewEssentials.API.Players;
using OpenMod.API.Commands;
using OpenMod.API.Ioc;
using OpenMod.API.Permissions;
using OpenMod.API.Persistence;
using OpenMod.API.Prioritization;
using OpenMod.Core.Users;

namespace NewEssentials.Players
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Transient, Priority = Priority.Normal)]
    public class CooldownManager : ICooldownManager
    {
        private readonly IPermissionChecker m_PermissionChecker;

        public CooldownManager(IPermissionChecker permissionChecker)
        {
            m_PermissionChecker = permissionChecker;
        }
        
        public async Task<double?> OnCooldownAsync(UserBase user, string sessionKey, string cooldownName, int cooldown)
        {
            // If there is a non-zero cooldown and the user is not exempt from cooldowns
            // Then we will add a cooldown to them for this specific kit/warp/etc
            // Unless they are already under a cooldown, in which we will return the remaining cooldown
            if (cooldown == 0 || await m_PermissionChecker.CheckPermissionAsync(user, $"{sessionKey}.cooldowns.exempt") != PermissionGrantResult.Deny)
                return null;

            var userSessionData = user.Session.SessionData;
            
            TimeSpan rightThisVeryInstant = DateTime.Now.TimeOfDay;
            if (!userSessionData.ContainsKey($"{sessionKey}.cooldowns.{cooldownName}"))
            {
                userSessionData.Add($"{sessionKey}.cooldowns.{cooldownName}", rightThisVeryInstant);
                return null;
            }

            TimeSpan lastUse = (TimeSpan) userSessionData[$"{sessionKey}.cooldowns.{cooldownName}"];
            double secondsSinceLastUse = (rightThisVeryInstant - lastUse).TotalSeconds;
            double remainingSeconds = Math.Round(cooldown - secondsSinceLastUse, 2);

            if (secondsSinceLastUse < cooldown)
                return remainingSeconds;

            userSessionData[$"{sessionKey}.cooldowns.{cooldownName}"] = rightThisVeryInstant;
            return null;
        }
    }
}