using System.Collections.Generic;
using System.Threading.Tasks;

namespace GravityLayer.Wearables
{
    public interface IWardrobe
    {
        public List<IWearable> Wearables { get; }
    }
}
