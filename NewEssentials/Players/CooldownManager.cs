using Microsoft.Extensions.DependencyInjection;
using NewEssentials.API.Players;
using OpenMod.API.Ioc;
using OpenMod.API.Permissions;
using OpenMod.API.Prioritization;
using OpenMod.Core.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewEssentials.Players
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Transient, Priority = Priority.Normal)]
    public class CooldownManager : ICooldownManager
    {
        private readonly IPermissionChecker m_PermissionChecker;

        private const string CooldownsDataKey = "cooldowns";

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

            var key = $"{sessionKey}.cooldowns.{cooldownName}";

            var now = DateTime.Now;

            var cooldowns = new Dictionary<string, DateTime>();

            switch (await user.GetPersistentDataAsync<object>(CooldownsDataKey))
            {
                case Dictionary<string, DateTime> dict:
                    cooldowns = dict;
                    break;
                case Dictionary<object, object> dict:
                    cooldowns = dict.Select(x =>
                            new KeyValuePair<string, DateTime>(x.Key.ToString(), DateTime.Parse(x.Value.ToString())))
                        .ToDictionary(x => x.Key, y => y.Value);
                    break;
                /*case Dictionary<string, string> dict:
                    cooldowns = dict.Select(x =>
                            new KeyValuePair<string, DateTime>(x.Key, DateTime.Parse(x.Value)))
                        .ToDictionary(x => x.Key, y => y.Value);
                    break;*/
            }

            if (cooldowns.TryGetValue(key, out var lastUse))
            {
                var secondsSinceLastUse = (now - lastUse).TotalSeconds;

                if (secondsSinceLastUse < cooldown)
                    return Math.Round(cooldown - secondsSinceLastUse, 2);

                cooldowns[key] = now;
            }
            else
                cooldowns.Add(key, now);
            
            await user.SavePersistentDataAsync(CooldownsDataKey, cooldowns);
            return null;
        }
    }
}