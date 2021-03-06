using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.Unturned.Commands;
using SDG.Unturned;

namespace NewEssentials.Commands
{
    [Command("pvp")]
    [CommandDescription("Toggle PVP on or off")]
    [CommandSyntax("[on/true/enable/off/false/disable]")]
    public class CPVP : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CPVP(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            string key;
            if (Context.Parameters.Length == 0)
            {
                Provider.isPvP = !Provider.isPvP;
                key = Provider.isPvP ? "pvp:enabled" : "pvp:disabled";

                await Context.Actor.PrintMessageAsync(m_StringLocalizer[key]);
                return;
            }

            switch (Context.Parameters[0].ToUpperInvariant())
            {
                case "ON":
                case "T":
                case "TRUE":
                case "E":
                case "ENABLE":
                case "ENABLED":
                    key = "pvp:enabled";
                    Provider.isPvP = true;
                    break;
                case "OFF":
                case "F":
                case "FALSE":
                case "D":
                case "DISABLE":
                case "DISABLED":
                    key = "pvp:disabled";
                    Provider.isPvP = false;
                    break;
                default:
                    throw new CommandWrongUsageException(Context);
            }

            await Context.Actor.PrintMessageAsync(m_StringLocalizer[key]);
        }
    }
}