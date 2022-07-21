using System.Threading.Tasks;

namespace GravityLayer.Wearables
{
    public interface IStock : IWardrobe
    {
        public Task FetchAllInteroperableWearables();
    }
}
