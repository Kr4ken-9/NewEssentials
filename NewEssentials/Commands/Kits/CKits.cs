using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NewEssentials.Models;
using OpenMod.API.Permissions;
using OpenMod.API.Persistence;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Commands;

namespace NewEssentials.Commands.Kits
{
    [Command("kits")]
    [CommandDescription("List the kits available to you")]
    public class CKits : UnturnedCommand
    {
        private readonly IDataStore m_DataStore;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IPermissionChecker m_PermissionChecker;
        private const string KitsKey = "kits";

        public CKits(IDataStore dataStore, IStringLocalizer stringLocalizer, IPermissionChecker permissionChecker, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_DataStore = dataStore;
            m_StringLocalizer = stringLocalizer;
            m_PermissionChecker = permissionChecker;
        }

        protected override async UniTask OnExecuteAsync()
        {
            KitsData kitsData = await m_DataStore.LoadAsync<KitsData>(KitsKey);

            if (kitsData.Kits.Count == 0)
            {
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["kits:none"]);
                return;
            }

            StringBuilder kitsBuilder = new StringBuilder();
            if (Context.Actor.Type == KnownActorTypes.Console)
            {
                foreach (var pair in kitsData.Kits) 
                    kitsBuilder.Append($"{pair.Key}, ");

                // Remove trailing ", "
                kitsBuilder.Remove(kitsBuilder.Length - 2, 2);
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["kits:kits", new {List = kitsBuilder.ToString()}]);
                return;
            }

            foreach (var pair in kitsData.Kits)
            {
                if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, $"kits.kit.{pair.Key}") ==
                    PermissionGrantResult.Grant ||
                    await m_PermissionChecker.CheckPermissionAsync(Context.Actor, $"kits.kit.give.{pair.Key}") ==
                    PermissionGrantResult.Grant)
                {
                    kitsBuilder.Append($"{pair.Key}, ");
                }
            }
            
            // Remove trailing ", "
            kitsBuilder.Remove(kitsBuilder.Length - 2, 2);
            await Context.Actor.PrintMessageAsync(m_StringLocalizer["kits:kits", new {List = kitsBuilder.ToString()}]);
        }
    }
}