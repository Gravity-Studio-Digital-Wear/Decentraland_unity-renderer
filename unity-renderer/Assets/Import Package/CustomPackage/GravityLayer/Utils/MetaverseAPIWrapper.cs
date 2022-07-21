using System.Threading.Tasks;

namespace GravityLayer.Utils
{

    public class MetaverseAPIWrapper : IFetchWearablesByAddress
    {
        string _apiUrl;
        string _secret;

        public MetaverseAPIWrapper(string apiUrl, string secret)
        {
            _apiUrl = apiUrl;
            _secret = secret;
        }

        public async Task<string> GetNFTs()
        {
            string url = _apiUrl + "/nfts";

            var result = await HTTPClient.Get(url, secret: _secret);

            return result;
        }

        public async Task<string> GetNFTsByAddress(string address)
        {
            string url = _apiUrl + "/nfts/getByAddress?address=" + address;

            var result = await HTTPClient.Get(url, secret: _secret);

            return result;
        }

        /// <param name="filter">for filtering results. Example "param_OutfitGender=0&param_platform=pc"</param>
        public async Task<string> GetModelMetadataByNft(string contractId, string tokenId, string filter = "")
        {
            string url = _apiUrl + "/nfts/getModelByNft?contractId=" + contractId + "&tokenTypeId=" + tokenId;

            if (filter.Length > 0)
            {
                url += "&" + filter;
            }

            var result = await HTTPClient.Get(url, secret: _secret);

            return result;
        }
    }
}
