using Cysharp.Threading.Tasks;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Core.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace NewEssentials.Commands
{
    [Command("salvagetime")]
    [CommandAlias("stime")]
    [CommandDescription("Change your or other player salvage time.")]
    [CommandSyntax("[user] <salvageTime>")]
    [RegisterCommandPermission("other", Description = "Allows to change salvage time of other player")]
    public class CSalvageTime : UnturnedCommand
    {
        public CSalvageTime(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            var targetActor = Context.Parameters.Count == 2
                ? await Context.Parameters.GetAsync<UnturnedUser>(0)
                : Context.Actor as UnturnedUser;

            if (targetActor != Context.Actor && await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
            {
                throw new NotEnoughPermissionException(Context, "other");
            }

            if (targetActor == null)
            {
                return;
            }

            var salvage = await Context.Parameters.GetAsync<float>(Context.Parameters.Count - 1);

            await UniTask.SwitchToMainThread();
            targetActor.Player.Player.interact.sendSalvageTimeOverride(salvage);
        }
    }
}
