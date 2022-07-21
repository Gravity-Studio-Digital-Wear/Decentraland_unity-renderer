using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GravityLayer.Utils.APIResponse.UserResponse
{
    [Serializable]
    public struct LoginResponse
    {
        public string address;
        public string challenge;
        public string token;
    }

    [Serializable]
    public struct WardrobeResult
    {
        public ProductData[] result;
    }

    [Serializable]
    public struct ProductData
    {

        public WardrobeData product;
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
