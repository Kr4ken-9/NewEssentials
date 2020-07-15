using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;

namespace NewEssentials.Commands.Respawn
{
    [Command("respawn")]
    [CommandDescription("Respawn objects/entities")]
    [CommandSyntax("<vehicles/animals>")]
    public class CRespawnRoot : UnturnedCommand
    {
        public CRespawnRoot(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }
    }
}