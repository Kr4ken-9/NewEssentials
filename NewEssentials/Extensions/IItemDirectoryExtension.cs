using Cysharp.Threading.Tasks;
using OpenMod.Core.Helpers;
using OpenMod.Extensions.Games.Abstractions.Items;
using OpenMod.Unturned.Items;
using System;
using System.Linq;

namespace NewEssentials.Extensions
{
    // https://github.com/openmod/openmod/blob/main/extensions/OpenMod.Extensions.Games.Abstractions/Items/ItemDirectoryExtensions.cs
    public static class IItemDirectoryExtension
    {
        public static async UniTask<UnturnedItemAsset?> FindByNameOrIdAsync(this IItemDirectory directory, string input)
        {
            var assets = (await directory.GetItemAssetsAsync())
                .OfType<UnturnedItemAsset>()
                .OrderBy(x => x.ItemAsset.id);

            if (!assets.Any())
            {
                return null;
            }

            var item = assets.FirstOrDefault(x => x.ItemAssetId.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                return item;
            }

            var matches = assets.Where(x => x.ItemName.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0);

            var minDist = int.MaxValue;

            foreach (var asset in matches)
            {
                var distance = StringHelper.LevenshteinDistance(input, asset.ItemName);

                // There's no lower distance
                if (distance == 0)
                    return asset;

                if (item == null || distance < minDist)
                {
                    item = asset;
                    minDist = distance;
                }
            }

            return item;
        }
    }
}
