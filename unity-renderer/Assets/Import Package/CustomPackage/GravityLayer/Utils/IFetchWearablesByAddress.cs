using System.Threading.Tasks;

namespace GravityLayer.Utils
{
    public interface IFetchWearablesByAddress
    {
        public Task<string> GetNFTsByAddress(string address);
    }
}
