using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NewEssentials.API.Chat;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using Color = System.Drawing.Color;

namespace NewEssentials.Commands.Chat
{
    [Command("superbroadcast")] 
    [CommandAlias("sb")] 
    [CommandAlias("sbroadcast")] 
    [CommandDescription("Broadcasts via a UI effect")]
    [CommandSyntax("<\"message\"> <seconds>")]
    public class CSuperBroadcast : UnturnedCommand
    {

        private readonly IBroadcastingService m_Broadcasting;
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_Localizer;
        
        public CSuperBroadcast(IServiceProvider serviceProvider, IBroadcastingService broadcasting, IConfiguration config, IStringLocalizer localizer) : base(serviceProvider)
        {
            m_Broadcasting = broadcasting;
            m_Configuration = config;
            m_Localizer = localizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            /*if (Context.Parameters.Length < 1 || Context.Parameters.Length > 2)
                throw new CommandWrongUsageException("/superbroadcast \"your text here\" <time>"); */
            

            if (m_Broadcasting.IsActive)
            {
                await Context.Actor.PrintMessageAsync( m_Localizer["broadcasting:is_active"], Color.Red);
                return;
            }

            //TODO: Input sanitization
            switch (Context.Parameters.Length)
            {
                case 1:
                    await m_Broadcasting.StartBroadcast(m_Configuration.GetValue<int>("broadcasting:defaultBroadcastDuration") * 1000, await Context.Parameters.GetAsync<string>(0));
                    break;
                case 2:
                {
                    /*if (!float.TryParse(Context.Parameters[1], out float time))
                        throw new CommandParameterParseException("Incorrect parameter", "time", typeof(float)); */
                    
                    int limit = m_Configuration.GetValue<int>("broadcasting:broadcastTimeLimit");
                    int time = await Context.Parameters.GetAsync<int>(1) * 1000;
                    
                    if (limit > 0 && time > limit)
                    {
                        await Context.Actor.PrintMessageAsync(m_Localizer["broadcasting:too_long"], Color.Red);
                        return;
                    }

                    await Context.Actor.PrintMessageAsync(m_Localizer["broadcasting:success"], Color.Green);
                    await m_Broadcasting.StartBroadcast(time, Context.Parameters[0]);
                    break;
                }
            }
        }
    }
}