using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Runtime.Services.Assets
{
    public interface IAssetProvider
    {
        UniTask<T> LoadAsync<T>(AssetReference reference, CancellationToken ct) where T : class;

        UniTask<GameObject> InstantiateAsync(
            AssetReferenceGameObject reference,
            Transform parent = null,
            CancellationToken ct = default);

        void ReleaseInstance(GameObject go);

        void Cleanup();
    }
}
