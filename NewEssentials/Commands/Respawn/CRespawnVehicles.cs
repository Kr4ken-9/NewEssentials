using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.Unturned.Commands;
using SDG.Unturned;

namespace NewEssentials.Commands.Respawn
{
    [Command("vehicles")]
    [CommandAlias("vehicle")]
    [CommandAlias("v")]
    [CommandParent(typeof(CRespawnRoot))]
    [CommandDescription("Respawns all vehicles instantly")]
    public class CRespawnVehicles : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CRespawnVehicles(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            await UniTask.SwitchToMainThread();
            VehicleManager.askVehicleDestroyAll();
            await Context.Actor.PrintMessageAsync(m_StringLocalizer["respawn:vehicles"]);
        }
    }
}