using GravityLayer.Utils;
using GravityLayer.Utils.APIResponse.MetaverseResponse;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GravityLayer.Wearables
{
    public class Stock : IStock
    {
        public List<IWearable> Wearables { get; private set; }

        MetaverseAPIWrapper _metaverseAPIWrapper;

        public Stock(MetaverseAPIWrapper wrapper)
        {
            Wearables = new List<IWearable>();
            _metaverseAPIWrapper = wrapper;
        }

        public async Task FetchAllInteroperableWearables()
        {
            string jsonString = await _metaverseAPIWrapper.GetNFTs();
            await FillStockFromJsonString(jsonString);
        }

        async Task FillStockFromJsonString(string jsonString)
        {
            Texture texture;
            Wearables.Clear();
            var jsonData = JsonUtility.FromJson<WardrobeResult>("{\"products\":" + jsonString + "}");
            foreach (var p in jsonData.products)
            {
                if ((p.metadata != null) & (p.metadata.Length > 0))
                {
                    texture = await Downloader.DownloadImage(p.metadata[0].previewImage);
                    Wearables.Add(new WearableBase(p.name, (Texture2D)texture, p.metadata[0].modelUrl));
                }
            }
        }
    }
}
