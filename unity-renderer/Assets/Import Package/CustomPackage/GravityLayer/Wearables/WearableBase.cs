using System;
using UnityEngine;
using System.Threading.Tasks;

namespace GravityLayer.Wearables
{
    public class WearableBase : IWearable
    {
        public string Title { get; private set; }
        public Texture2D PreviewImage { get; private set; }
        public string ModelUrl { get; private set; }
        public string MetaverseId { get; private set; }

        public WearableBase(string title, Texture2D preview, string modelUrl)
        {
            Title = title;
            PreviewImage = preview;
            ModelUrl = modelUrl;
        }
    }
}