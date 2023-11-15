using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NewEssentials.API.Items;
using NewEssentials.User;
using OpenMod.API.Ioc;
using OpenMod.API.Permissions;
using OpenMod.API.Users;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Priority = OpenMod.API.Prioritization.Priority;

namespace NewEssentials.Items;

[ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
public class ItemBlacklistController : IItemBlacklistController, IDisposable
{
    private readonly IUserManager m_UserManager;
    private readonly IConfiguration m_Configuration;
    private readonly IPermissionChecker m_PermissionChecker;
    
    public ItemBlacklistController(IUserManager userManager, IConfiguration configuration, IPermissionChecker permissionChecker)
    {
        m_UserManager = userManager;
        m_Configuration = configuration;
        m_PermissionChecker = permissionChecker;
        ItemManager.onTakeItemRequested += OnTakeItemRequested;
    }

    private void OnTakeItemRequested(Player player, byte x, byte y, uint instanceid, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemdata, ref bool shouldallow)
    {
        IUser usr = m_UserManager.ToUserAsync(player).Result;

        shouldallow = !m_Configuration.GetValue<ushort[]>("items:itemBlacklist").Contains(itemdata.item.id) ||
                      m_PermissionChecker.CheckPermissionAsync(usr, "itemblacklist.exempt").Result == PermissionGrantResult.Grant;

        if (!shouldallow)
            usr.PrintMessageAsync($"You are not permitted to have {itemdata.item.GetAsset().FriendlyName}", System.Drawing.Color.Red);
    }

    public async Task CheckUserInventoryAsync(UnturnedUser user)
    {
        if (await m_PermissionChecker.CheckPermissionAsync(user, "itemblacklist.exempt") == PermissionGrantResult.Grant)
            return;
        
        ushort[] ids = m_Configuration.GetValue<ushort[]>("items:itemBlacklist");

        foreach (ushort u in ids)
        {
            List<InventorySearch> s = user.Player.Player.inventory.search(u, true, true);
            foreach (InventorySearch i in s)
                user.Player.Player.inventory.sendDropItem(i.page, i.jar.x, i.jar.y);
            if (s.Count > 0)
                await user.PrintMessageAsync(
                    $"Dropped {s.Count} of blacklisted item {s.First().GetAsset().FriendlyName}", Color.Red);
        }
        
    }

    public void Dispose()
    {
        ItemManager.onTakeItemRequested -= OnTakeItemRequested;
    }
}