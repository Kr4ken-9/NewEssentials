﻿using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Commands.Movement;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Core.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;
using NewEssentials.API.User;

namespace NewEssentials.Commands
{
    [Command("salvagespeed")]
    [CommandAlias("sspeed")]
    [CommandDescription("Change your or other player salvage speed.")]
    [CommandSyntax("[user] <salvageSpeed>")]
    [RegisterCommandPermission("other", Description = "Allows to change salvage speed of other player")]
    public class CSalvageSpeed : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserParser m_UserParser;

        public CSalvageSpeed(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer, IUserParser userParser) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserParser = userParser;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var targetActor = Context.Parameters.Count == 2
                ? await m_UserParser.ParseUserAsync(Context.Parameters[0])
                : Context.Actor as UnturnedUser;

            if (targetActor != Context.Actor && await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            
            if (targetActor == null)
                return;
            
            var salvage = await Context.Parameters.GetAsync<float>(Context.Parameters.Count - 1);

            await UniTask.SwitchToMainThread();
            targetActor.Player.Player.interact.sendSalvageTimeOverride(salvage);

            if (Context.Actor != targetActor)
            {
                await PrintAsync(m_StringLocalizer["salvageSpeed:instigator", new
                {
                    User = targetActor,
                    SalvageSpeed = salvage
                }]);
            }
            await targetActor.PrintMessageAsync(m_StringLocalizer["salvageSpeed:target", new { SalvageSpeed = salvage }]);
        }
    }
}
