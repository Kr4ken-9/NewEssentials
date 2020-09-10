using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using UnityEngine;

namespace NewEssentials.Commands.Movement
{
    [Command("position")]
    [CommandAlias("pos")]
    [CommandDescription("Print the coordinates of your current position.")]
    [CommandActor(typeof(UnturnedUser))]
    public class CPosition : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CPosition(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            await UniTask.SwitchToMainThread();
            UnturnedUser user = (UnturnedUser) Context.Actor;
            Vector3 position = user.Player.Player.transform.position;

            await user.PrintMessageAsync(m_StringLocalizer["position",
                new {X = position.x, Y = position.y, Z = position.z}]);
        }
    }
}