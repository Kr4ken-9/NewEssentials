using System.Linq;
using SDG.Unturned;

namespace NewEssentials.Unturned
{
    public static class UnturnedAssetHelper
    {
        public static bool GetItem(string searchTerm, out ItemAsset item)
        {
            if (string.IsNullOrEmpty(searchTerm.Trim()))
            {
                item = null;
                return false;
            }

            if (!ushort.TryParse(searchTerm, out ushort id))
            {
                item = Assets.find(EAssetType.ITEM).Cast<ItemAsset>().Where(i => !string.IsNullOrEmpty(i.itemName))
                    .OrderBy(i => i.itemName.Length).FirstOrDefault(i =>
                        i.itemName.ToUpperInvariant().Contains(searchTerm.ToUpperInvariant()));

                return item != null;
            }

            item = (ItemAsset)Assets.find(EAssetType.ITEM, id);
            return item != null;
        }

        public static bool GetVehicle(string searchTerm, out VehicleAsset vehicle)
        {
            if (string.IsNullOrEmpty(searchTerm.Trim()))
            {
                vehicle = null;
                return false;
            }

            if (!ushort.TryParse(searchTerm, out ushort id))
            {
                vehicle = Assets.find(EAssetType.VEHICLE).Cast<VehicleAsset>()
                    .Where(v => !string.IsNullOrEmpty(v.vehicleName)).OrderBy(v => v.vehicleName.Length)
                    .FirstOrDefault(v => v.vehicleName.ToUpperInvariant().Contains(searchTerm.ToUpperInvariant()));

                return vehicle != null;
            }

            vehicle = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
            return vehicle != null;
        }
    }
}