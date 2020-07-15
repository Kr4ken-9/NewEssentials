using System;
using Cysharp.Threading.Tasks;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;

namespace NewEssentials.Commands
{
    [Command("effect")]
    [CommandAlias("eff")]
    [CommandDescription("Spawn an effect on your position")]
    public class CEffect : UnturnedCommand
    {

        private readonly IPermissionChecker _permission;
        
        public CEffect(IServiceProvider serviceProvider, IPermissionChecker checker) : base(serviceProvider)
        {
            _permission = checker;
        }

        protected override async UniTask OnExecuteAsync()
        {
            //TODO: Add support for other players, configurable allowed effects?

            if (await _permission.CheckPermissionAsync(Context.Actor, "effect") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "You do not have permission to execute this command!");

            ushort effectId = await Context.Parameters.GetAsync<ushort>(0);

            UnturnedUser usr = (UnturnedUser) Context.Actor;

            await UniTask.SwitchToMainThread();
            
            EffectManager.sendEffect(effectId, usr.SteamId, usr.Player.transform.position);
        }
    }
}