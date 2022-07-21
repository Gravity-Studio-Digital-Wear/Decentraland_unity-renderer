using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace GravityLayer.Utils
{
    public static class Downloader
    {
        public static async Task<Texture> DownloadImage(string mediaUrl)
        {
            try
            {
                using var request = UnityWebRequestTexture.GetTexture(mediaUrl);

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                    Debug.LogError($"Failed: {request.error}");

                return DownloadHandlerTexture.GetContent(request);
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(DownloadImage)} failed: {e.Message}");
                return default;
            }
        }

        public static async Task<byte[]> DownloadAvatar(string url)
        {
            try
            {
                using var request = new UnityWebRequest(url);

                request.downloadHandler = new DownloadHandlerBuffer();

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                    Debug.LogError($"Failed: {request.error}");

                return request.downloadHandler.data;
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(DownloadAvatar)} failed: {e.Message}");
                return default;
            }
        }
    }
}
