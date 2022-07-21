using UnityEngine;

namespace GravityLayer.Wearables
{
    public interface IWearable
    {
        public string Title { get; }
        public Texture2D PreviewImage { get; }
        public string ModelUrl { get; }
    }
}
