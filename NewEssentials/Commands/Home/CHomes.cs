using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;

namespace NewEssentials.Commands.Home
{
    [Command("homes")]
    [CommandDescription("List your saved homes")]
    [CommandActor(typeof(UnturnedUser))]
    public class CHomes : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserDataStore m_UserDataStore;

        public CHomes(IStringLocalizer stringLocalizer, IUserDataStore userDataStore,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_UserDataStore = userDataStore;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            UserData userData = await m_UserDataStore.GetUserDataAsync(uPlayer.Id, uPlayer.Type);
            
            if (!userData.Data.ContainsKey("homes"))
                throw new UserFriendlyException(m_StringLocalizer["home:no_home"]);
            
            var stringBuilder = new StringBuilder();
            foreach (var pair in (Dictionary<object, object>) userData.Data["homes"])
                stringBuilder.Append($"{pair.Key}, ");

            stringBuilder.Remove(stringBuilder.Length - 2, 1);

            await uPlayer.PrintMessageAsync(m_StringLocalizer["home:list", new {Homes = stringBuilder.ToString()}]);
        }
    }
}