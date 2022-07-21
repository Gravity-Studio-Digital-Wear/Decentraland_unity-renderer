using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GravityLayer.Utils
{
    public static class HTTPClient
    {
        public static async Task<string> Get(string url, string jwt = "", string secret = "")
        {
            try
            {
                using var request = UnityWebRequest.Get(url);

                request.SetRequestHeader("Content-Type", "application/json");

                if (jwt.Length > 0)
                {
                    request.SetRequestHeader("Authorization", "Bearer " + jwt);
                }

                if (secret.Length > 0)
                {
                    request.SetRequestHeader("api-key", secret);
                }

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                    Debug.LogError($"Failed: {request.error}");

                var result = request.downloadHandler.text;

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(Get)} failed: {e.Message}");
                return default;
            }
        }

        public static async Task<string> Post(string url, string jsonData)
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

            try
            {
                using var request = UnityWebRequest.Post(url, "POST");

                request.SetRequestHeader("Content-Type", "application/json");
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                    Debug.LogError($"Failed: {request.error}");

                var result = request.downloadHandler.text;

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(Post)} failed: {e.Message}");
                return default;
            }
        }
    }
}
