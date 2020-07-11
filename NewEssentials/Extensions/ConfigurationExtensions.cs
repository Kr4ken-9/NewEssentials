using Microsoft.Extensions.Configuration;

namespace NewEssentials.Extensions
{
    public static class ConfigurationExtensions
    {
        public static bool GetItemAmount(this IConfiguration configuration, byte inputAmount, out byte finalAmount)
        {
            finalAmount = inputAmount;

            if (!configuration.GetValue<bool>("items:enableamountlimit"))
                return true;

            byte maxAmount = configuration.GetValue<byte>("items:maxspawnamount");
            if (finalAmount <= maxAmount)
                return true;

            finalAmount = maxAmount;

            return configuration.GetValue<bool>("items:silentamountlimit");
        }
    }
}