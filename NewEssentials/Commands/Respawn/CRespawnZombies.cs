using System;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.Unturned.Commands;
using SDG.Unturned;

namespace NewEssentials.Commands.Respawn
{
    [Command("zombies")]
    [CommandAlias("zombie")]
    [CommandAlias("z")]
    [CommandParent(typeof(CRespawnRoot))]
    [CommandDescription("Respawn all dead zombies")]
    public class CRespawnZombies : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly FieldInfo m_LastDead;
        private readonly FieldInfo m_LastWave;

        public CRespawnZombies(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_LastDead = typeof(Zombie).GetField("_lastDead", BindingFlags.NonPublic | BindingFlags.Instance);
            m_LastWave = typeof(ZombieManager).GetField("lastWave", BindingFlags.NonPublic | BindingFlags.Static);
        }

        protected override async UniTask OnExecuteAsync()
        {
            await Context.Actor.PrintMessageAsync(m_StringLocalizer["general:experimental"]);
            
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            m_LastWave.SetValue(null, 0f);
            
            int counter = 0;
            for (int i = 0; i < LevelNavigation.bounds.Count; i++)
            {
                var region = ZombieManager.regions[i];
                foreach (var zombie in region.zombies.Where(z => z.isDead))
                {
                    m_LastDead.SetValue(zombie, 0f);
                    counter++;
                }
            }

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["respawn:zombies", new {Count = counter}]);
        }
    }
}