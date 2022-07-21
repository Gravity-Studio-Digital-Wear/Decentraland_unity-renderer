using System;
using GravityLayer.Wearables;
using GravityLayer.Utils;
using System.Threading.Tasks;

namespace GravityLayer
{
    /// <summary>
    /// The GravityLayerEntryPoint class contains all public points to deal with Gravity Layer API. It serves as an example.
    /// </summary>
    /// <remarks>
    /// Authentication is done by receiving a JWT token for a User via signing a message from Gravity Layer API. See details in documentation.
    /// </remarks>
    public class GravityLayerEntryPoint
    {
        public Connection GLayerConnection { get; private set; }
        public Wardrobe Wardrobe { get; private set; }

        private string _apiUrl;

        private string _account;

        public GravityLayerEntryPoint(string apiUrl, string account, string metaverseId, Func<string, Task<string>> signMessage)
        {
            _apiUrl = apiUrl;
            _account = account;
 
            GLayerConnection = new Connection(_apiUrl, _account, signMessage);
            Wardrobe = new Wardrobe(GLayerConnection, metaverseId);
        }
    }
}