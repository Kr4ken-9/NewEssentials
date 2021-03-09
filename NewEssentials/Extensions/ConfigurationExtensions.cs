using Microsoft.Extensions.Configuration;

namespace NewEssentials.Extensions
{
    public static class ConfigurationExtensions
    {
        public static bool GetItemAmount(this IConfiguration configuration, ushort inputAmount, out ushort finalAmount)
        {
            finalAmount = inputAmount;

            if (!configuration.GetValue<bool>("items:enableamountlimit"))
                return true;

            var maxAmount = configuration.GetValue<ushort>("items:maxspawnamount");
            if (finalAmount <= maxAmount)
                return true;

            finalAmount = maxAmount;

            return configuration.GetValue<bool>("items:silentamountlimit");
        }
    }
}