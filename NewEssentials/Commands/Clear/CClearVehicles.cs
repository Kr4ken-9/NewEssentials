using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using SDG.Unturned;
using UnityEngine;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.Clear
{
    [UsedImplicitly]
    [Command("vehicles")]
    [CommandAlias("vehicle")]
    [CommandAlias("v")]
    [CommandParent(typeof(CClearRoot))]
    [CommandDescription("Clears all vehicles or only empty vehicles")]
    [CommandSyntax("[empty]")]
    public class CClearVehicles : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CClearVehicles(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.clear.vehicles";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length > 1)
                throw new CommandWrongUsageException(Context);

            if (Context.Parameters.Length == 0)
            {
                await UniTask.SwitchToMainThread();

                byte counter = BombVehicles(VehicleManager.vehicles);

                await Context.Actor.PrintMessageAsync(m_StringLocalizer["clear:vehicles"]);
            }
            else
            {
                string notEmpty = Context.Parameters[0];
                if(!notEmpty.Equals("empty", StringComparison.InvariantCultureIgnoreCase))
                    throw new CommandWrongUsageException(Context);
                
                await UniTask.SwitchToMainThread();

                byte counter = BombVehicles(VehicleManager.vehicles.Where(v => v.isEmpty));

                await Context.Actor.PrintMessageAsync(m_StringLocalizer["clear:vehicles_empty", new {Count = counter}]);
            }
        }

        private byte BombVehicles(IEnumerable<InteractableVehicle> vehicles)
        {
            byte counter = 0;
            foreach (var vehicle in vehicles)
            {
                vehicle.transform.position = new Vector3(0, 0, 0);
                VehicleManager.sendVehicleExploded(vehicle);
                counter++;
            }

            return counter;
        }
    }
}