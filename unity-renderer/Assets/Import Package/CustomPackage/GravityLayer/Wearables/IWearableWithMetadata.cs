using System.Collections.Generic;

namespace GravityLayer.Wearables
{
    public interface IWearableWithMetadata
    {
        public string Title { get; }

        public List<Metadata> Metadata { get; }
    }
}
