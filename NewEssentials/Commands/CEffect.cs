using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using Microsoft.Extensions.Localization;
using NewEssentials.API.User;
using OpenMod.API.Commands;

namespace NewEssentials.Commands
{
    [Command("effect")]
    [CommandAlias("eff")]
    [CommandDescription("Spawn an effect on your position")]
    [CommandSyntax("<id> [player]")]
    public class CEffect : UnturnedCommand
    {

        private readonly IUserParser m_UserParser;
        private readonly IStringLocalizer m_StringLocalizer;

        public CEffect(IServiceProvider serviceProvider, IUserParser userParser, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_UserParser = userParser;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            //TODO: Add support for configurable allowed effects?

            var targetActor = Context.Parameters.Count == 2
                ? await m_UserParser.ParseUserAsync(Context.Parameters[1])
                : Context.Actor as UnturnedUser;
            
            
            //TODO: add messages for the below
            
            if (targetActor == null)
                throw new UserFriendlyException(m_StringLocalizer["commands:failed_player"]);

            ushort effectId = await Context.Parameters.GetAsync<ushort>(0);

            await UniTask.SwitchToMainThread();

            //no one cares nelson stop ruining my day ty
#pragma warning disable CS0618 // Type or member is obsolete
            TriggerEffectParameters paras = new TriggerEffectParameters(effectId)
            {
                position = targetActor.Player.Player.transform.position
            };
#pragma warning restore CS0618 // Type or member is obsolete
            paras.SetRelevantPlayer(targetActor.Player.SteamPlayer);
            
            EffectManager.triggerEffect(paras);
            
            await Context.Actor.PrintMessageAsync(
                $"Successfully gave effect {effectId} to {targetActor.DisplayName}");
        }
    }
}