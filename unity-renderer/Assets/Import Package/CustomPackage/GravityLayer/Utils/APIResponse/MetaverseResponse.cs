using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GravityLayer.Utils.APIResponse.MetaverseResponse
{
    [Serializable]
    public struct WardrobeResult
    {
        public WardrobeData[] products;
    }

    [Serializable]
    public struct WardrobeData
    {
        public string _id;
        public string name;
        public string description;
        public string[] images;
        public ProductMetadata[] metadata;
    }

    [Serializable]
    public struct ProductMetadata
    {
        public string metaverseId;
        public string modelUrl;
        public string previewImage;
        public MetadataAttributes[] attributes;
    }

    [Serializable]
    public struct MetadataAttributes
    {
        public string name;
        public string value;
    }
}
