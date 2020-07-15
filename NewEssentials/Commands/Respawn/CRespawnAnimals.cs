using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Helpers;
using OpenMod.Unturned.Commands;
using SDG.Unturned;
using Random = UnityEngine.Random;

namespace NewEssentials.Commands.Respawn
{
    [Command("animals")]
    [CommandAlias("animal")]
    [CommandAlias("a")]
    [CommandParent(typeof(CRespawnRoot))]
    [CommandDescription("Respawn dead animals")]
    public class CRespawnAnimals : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly FieldInfo m_LastDead;

        public CRespawnAnimals(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_LastDead = typeof(Animal).GetField("_lastDead", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            int counter = 0;
            foreach (var animal in AnimalManager.animals.Where(a => a.isDead))
            {
                m_LastDead.SetValue(animal, 0f);
                counter++;
            }

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["respawn:animals", new {Count = counter}]);
        }
    }
}