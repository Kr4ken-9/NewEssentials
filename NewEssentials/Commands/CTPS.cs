using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Unturned.Commands;
using SDG.Unturned;
using System;

namespace NewEssentials.Commands
{
    [Command("tps")]
    [CommandDescription("Shows TPS")]
    public class CTPS : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CTPS(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override UniTask OnExecuteAsync()
        {
            var tps = Provider.debugTPS;

            return PrintAsync(m_StringLocalizer["tps:tps", new { Ticks = tps, IsConsole = Context.Actor is ConsoleActor }])
                .AsUniTask(false);
        }
    }
}
