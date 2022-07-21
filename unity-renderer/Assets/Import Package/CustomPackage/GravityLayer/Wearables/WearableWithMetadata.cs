using System.Collections.Generic;

namespace GravityLayer.Wearables
{
    public class WearableWithMetadata : IWearableWithMetadata
    {
        public string Title { get; private set; }

        public List<Metadata> Metadata { get; set; }

        public WearableWithMetadata(string title, List<Metadata> metadata = null)
        {
            Title = title;
            Metadata = metadata;
        }
    }
}
