using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace NewEssentials.Commands
{
    [Command("effect")]
    [CommandAlias("eff")]
    [CommandDescription("Spawn an effect on your position")]
    [CommandSyntax("<id> [player]")]
    public class CEffect : UnturnedCommand
    {

        private readonly IPermissionChecker _permission;
        
        public CEffect(IServiceProvider serviceProvider, IPermissionChecker checker) : base(serviceProvider)
        {
            _permission = checker;
        }

        protected override async UniTask OnExecuteAsync()
        {
            //TODO: Add support for configurable allowed effects?
            
            if (await _permission.CheckPermissionAsync(Context.Actor, "effect") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "You do not have permission to execute this command!");

            UnturnedUser usr = (UnturnedUser) Context.Actor;
            Player target;

            
            if (Context.Parameters.Length > 1 && Context.Parameters.Length < 2)
            {
                target = Provider.clients.FirstOrDefault(p =>
                    p.player.name.Contains(Context.Parameters.GetAsync<string>(1).Result))?.player;
                
                if (target == null)
                    throw new CommandWrongUsageException("Could not find player");
            }
            else if (Context.Parameters.Length == 1)
                target = ((UnturnedUser) Context.Actor).Player;
            else
                throw new CommandWrongUsageException("Unexpected amount of parameters");


            if (target == null)
                return;
            
            ushort effectId = await Context.Parameters.GetAsync<ushort>(0);
            
            await UniTask.SwitchToMainThread();
            
            EffectManager.sendEffect(effectId, target.channel.owner.playerID.steamID, target.transform.position);
            
            await usr.PrintMessageAsync(
                $"Successfully gave effect {effectId} to {target.channel.owner.playerID.characterName}");
        }
    }
}