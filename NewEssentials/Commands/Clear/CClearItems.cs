﻿using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Permissions;
using SDG.Unturned;
using UnityEngine;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands.Clear
{
    [Command("items")]
    [CommandAlias("item")]
    [CommandParent(typeof(CClearRoot))]
    [CommandDescription("Clears all items")]
    public class CClearItems : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfiguration m_Configuration;

        public CClearItems(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer,
            IConfiguration configuration, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.clear.items";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length > 0)
                throw new CommandWrongUsageException(Context);

            await UniTask.SwitchToMainThread();
            int counter = BombAllItems();
            await Context.Actor.PrintMessageAsync(m_StringLocalizer["clear:items", new {Count = counter}]);
        }

        private int BombAllItems()
        {
            int counter = 0;
            for (byte b = 0; b < Regions.WORLD_SIZE; b += 1)
            {
                for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 += 1)
                {
                    var itemRegion = ItemManager.regions[b, b2];
                    itemRegion.lastRespawn = Time.realtimeSinceStartup;
                    counter += itemRegion.drops.Count;
                    
                    ItemManager.askClearRegionItems(b, b2);
                }
            }

            return counter;
        }
    }
}