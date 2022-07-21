using System;
using UnityEngine;
using System.Threading.Tasks;
using GravityLayer.Utils.APIResponse.UserResponse;

namespace GravityLayer.Utils
{
    public class Connection : IFetchWearablesByMetaverseId
    {
        public bool ConnectionEstablished { get; private set; }

        private string _address;
        private string _uuid;
        private string _signature;
        private string _jwt;
        private string _apiUrl;
        private Func<string, Task<string>> _signMessage;

        public Connection(string apiUrl, string address, Func<string, Task<string>> signMessage)
        {
            _apiUrl = apiUrl;
            _address = address;
            _signMessage = signMessage;
            _jwt = null;
            ConnectionEstablished = false;
        }

        public async Task<string> EstablishConnection()
        {
            await Challenge();
            await Sign(_uuid);
            await AuthLogin();
            if (_jwt != null)
            {
                ConnectionEstablished = true;
                Debug.Log("Connection to Gravity API established");
            }
            else
            {
                Debug.Log("Connection to Gravity API has not been established");
            }
            return _jwt;
        }

        public async Task Challenge()
        {
            string url = _apiUrl + "/auth/challenge";
            string json = "{\"address\": \"" + _address + "\"}";

            string response = await HTTPClient.Post(url, json);

            LoginResponse data = JsonUtility.FromJson<LoginResponse>(response);

            _uuid = data.challenge;
        }

        public async Task Sign(string message)
        {
            try
            {
                _signature = await _signMessage(message);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async Task AuthLogin()
        {
            string url = _apiUrl + "/auth/login";
            string json = "{\"address\": \"" + _address + "\", \"signature\": \"" + _signature + "\"}";

            string response = await HTTPClient.Post(url, json);

            LoginResponse data = JsonUtility.FromJson<LoginResponse>(response);

            _jwt = data.token;
        }

        public async Task<string> FetchWearablesByMetaverseId(string metaverseId)
        {
            string url = _apiUrl + "/warehouse/products/my?metaverse=" + metaverseId;

            var result = await HTTPClient.Get(url, _jwt);

            return result;
        }

        public string EstablishConnection(string testToken)
        {
            if (testToken != null)
            {
                _jwt = testToken;
                ConnectionEstablished = true;
                return _jwt;
            }
            else
            {
                ConnectionEstablished = false;
                return null;
            }
        }
    }
}