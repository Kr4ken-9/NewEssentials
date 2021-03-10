using Cysharp.Threading.Tasks;
using OpenMod.Core.Helpers;
using OpenMod.Extensions.Games.Abstractions.Items;
using OpenMod.Unturned.Items;
using System;
using System.Linq;

namespace NewEssentials.Extensions
{
    public static class IItemDirectoryExtension
    {
        public static async UniTask<UnturnedItemAsset?> FindByNameOrIdAsync(this IItemDirectory directory, string input)
        {
            var assets = (await directory.GetItemAssetsAsync())
                .OfType<UnturnedItemAsset>();

            await UniTask.SwitchToThreadPool();

            if (!assets.Any())
            {
                return null;
            }

            var itemAsset = assets.FirstOrDefault(x => x.ItemAssetId.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (itemAsset != null)
            {
                return itemAsset;
            }

            itemAsset = assets
                .Where(z => input.Split(' ')
                  .All(x => z.ItemName.Trim().Split(' ')
                    .Any(y => y.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0)))
                .OrderBy(x => x.ItemAsset.id)
                .FirstOrDefault();

            if (itemAsset != null)
            {
                return itemAsset;
            }

            // https://github.com/openmod/openmod/blob/main/extensions/OpenMod.Extensions.Games.Abstractions/Items/ItemDirectoryExtensions.cs#L29
            var maxDistance = int.MaxValue;

            assets = assets
                .Where(z => input.Split(' ')
                  .Any(x => z.ItemName.Trim().Split(' ')
                    .Any(y => y.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0)));

            foreach (var (asset, distance) in from asset in assets
                                              let trimName = asset.ItemName.Trim()
                                              let distance = StringHelper.LevenshteinDistance(trimName, input)
                                              select (asset, distance))
            {
                if (distance == 0)
                {
                    itemAsset = asset;
                    break;
                }

                if (distance < maxDistance)
                {
                    itemAsset = asset;
                    maxDistance = distance;
                }
            }

            return itemAsset;
        }
    }
}
