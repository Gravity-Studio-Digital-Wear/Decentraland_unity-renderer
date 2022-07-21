using GravityLayer.Utils;
using GravityLayer.Utils.APIResponse.MetaverseResponse;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GravityLayer.Wearables
{
    public class WearableServices
    {
        MetaverseAPIWrapper _metaverseAPIWrapper;

        public WearableServices(MetaverseAPIWrapper wrapper)
        {
            _metaverseAPIWrapper = wrapper;
        }

        public async Task<List<Metadata>> GetWearableMetadata(string contractId, string tokenId, string filter = "")
        {
            string jsonString = await _metaverseAPIWrapper.GetModelMetadataByNft(contractId, tokenId, filter);
            var jsonData = JsonUtility.FromJson<WardrobeData>("{\"metadata\":" + jsonString + "}");

            if ((jsonData.metadata == null) || (jsonData.metadata.Length == 0)) return null;

            List<Metadata> wearableMetadata = new List<Metadata>();
            foreach (var w in jsonData.metadata)
            {
                Texture texture = await Downloader.DownloadImage(w.previewImage);

                var attributes = new Dictionary<string, string>();
                foreach (var a in w.attributes)
                {
                    attributes.Add(a.name, a.value);
                }

                Metadata m = new Metadata((Texture2D)texture, w.modelUrl, attributes);

                wearableMetadata.Add(m);
            }
            return wearableMetadata;
        }
    }
}
