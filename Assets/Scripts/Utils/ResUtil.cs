using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TestGame.Utils
{
    public static class ResUtil
    {
        public static async Task<T> LoadAsset<T>(string key) where T : UnityEngine.Object
        {
            var oh = Addressables.LoadAssetAsync<T>(key);
            await oh.Task;
            if (oh.Status == AsyncOperationStatus.Succeeded)
            {
                return oh.Result;
            }

            throw oh.OperationException;
        }

        public static async Task<GameObject> Instantiate(string key)
        {
            var oh = Addressables.InstantiateAsync(key);
            await oh.Task;
            if (oh.Status == AsyncOperationStatus.Succeeded)
            {
                return oh.Result;
            }

            throw oh.OperationException;
        }
    }
}