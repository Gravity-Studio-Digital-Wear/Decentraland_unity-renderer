using System;
using GravityLayer.Wearables;
using GravityLayer.Utils;
using System.Threading.Tasks;

namespace GravityLayer
{
    /// <summary>
    /// The MetaverseEntryPoint class contains all public points to deal with Gravity Layer API. It serves as an example.
    /// </summary>
    /// <remarks>
    /// Authentication is done by Metaverse API key (field `secret`).
    /// </remarks>
    public class MetaverseEntryPoint
    {
        public MetaverseAPIWrapper GLMetaverseAPIWrapper { get; private set; }
        public WardrobeByUser Wardrobe { get; private set; }
        public Stock Stock { get; private set; }
        public WearableServices WearableServices { get; private set; }

        public MetaverseEntryPoint(string apiUrl, string secret)
        {
            GLMetaverseAPIWrapper = new MetaverseAPIWrapper(apiUrl, secret);
            Wardrobe = new WardrobeByUser(GLMetaverseAPIWrapper);
            Stock = new Stock(GLMetaverseAPIWrapper);
            WearableServices = new WearableServices(GLMetaverseAPIWrapper);
        }
    }
}
