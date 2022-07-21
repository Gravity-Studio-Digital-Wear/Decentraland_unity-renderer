using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using GravityLayer.Utils;
using GravityLayer.Utils.APIResponse.MetaverseResponse;

namespace GravityLayer.Wearables
{
    public class WardrobeByUser : IWardrobe
    {
        public List<IWearable> Wearables { get; private set; }

        private IFetchWearablesByAddress _APIWrapper;

        public WardrobeByUser(IFetchWearablesByAddress APIWrapper)
        {
            Wearables = new List<IWearable>();
            _APIWrapper = APIWrapper;
        }

        public async Task FetchInteroperableWearables(string address)
        {
            string jsonString = await _APIWrapper.GetNFTsByAddress(address);
            await FillWardrobeFromJsonString(jsonString);
        }

        async Task FillWardrobeFromJsonString(string jsonString)
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
