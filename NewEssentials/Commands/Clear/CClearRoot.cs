using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;

namespace NewEssentials.Commands.Clear
{
    [Command("clear")]
    [CommandDescription("description")]
    [CommandSyntax("<items/vehicles/inventory>")]
    public class CClearRoot : UnturnedCommand
    {
        public CClearRoot(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override UniTask OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }
    }
}