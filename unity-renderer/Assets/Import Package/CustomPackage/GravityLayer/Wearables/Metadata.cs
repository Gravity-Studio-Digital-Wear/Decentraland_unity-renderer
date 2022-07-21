using System.Collections.Generic;
using UnityEngine;

namespace GravityLayer.Wearables
{
    public class Metadata
    {
        public Texture2D PreviewImage { get; private set; }
        public string ModelUrl { get; private set; }

        public Dictionary<string, string> Attributes;

        public Metadata(Texture2D previewImage, string modelUrl, Dictionary<string,string> attributes = null)
        {
            PreviewImage = previewImage;
            ModelUrl = modelUrl;
            Attributes = attributes;
        }
    }
}
