using System.Threading.Tasks;

namespace GravityLayer.Utils
{
    public interface IFetchWearablesByMetaverseId
    {
        public Task<string> FetchWearablesByMetaverseId(string metaverseId);
    }

}
