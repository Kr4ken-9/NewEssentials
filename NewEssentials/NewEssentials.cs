using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.Core.Plugins;
using OpenMod.Unturned.Plugins;

[assembly: PluginMetadata("NewEssentials", Author="Kr4ken", DisplayName="New Essentials")]

namespace NewEssentials
{
    [UsedImplicitly]
    public class NewEssentials : OpenModUnturnedPlugin
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ILogger<NewEssentials> m_Logger;
        private readonly IConfiguration m_Configuration;

        public NewEssentials(IStringLocalizer stringLocalizer, ILogger<NewEssentials> logger,
            IConfiguration configuration, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_Logger = logger;
            m_Configuration = configuration;
        }

        protected override async UniTask OnLoadAsync()
        {
            await UniTask.SwitchToThreadPool();

            m_Logger.LogInformation(m_Configuration.GetValue<bool>("kits")
                ? m_StringLocalizer["kits:enabled"]
                : m_StringLocalizer["kits:disabled"]);
        }
    }
}