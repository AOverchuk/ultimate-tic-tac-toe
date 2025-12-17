using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Runtime.Services.Assets
{
    public sealed class AddressablesAssetProvider : IAssetProvider, IDisposable
    {
        private readonly List<AsyncOperationHandle> _loadedHandles = new();
        private readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instanceHandles = new();

        public async UniTask<T> LoadAsync<T>(AssetReference reference, CancellationToken ct) where T : class
        {
            var handle = Addressables.LoadAssetAsync<T>(reference.RuntimeKey);

            try
            {
                await handle.ToUniTask(cancellationToken: ct);
                _loadedHandles.Add(handle);
                return handle.Result;
            }
            catch
            {
                if (handle.IsValid())
                    Addressables.Release(handle);

                throw;
            }
        }

        public async UniTask<GameObject> InstantiateAsync(
            AssetReferenceGameObject reference,
            Transform parent = null,
            CancellationToken ct = default)
        {
            var handle = Addressables.InstantiateAsync(reference.RuntimeKey, parent);

            try
            {
                await handle.ToUniTask(cancellationToken: ct);
                var instance = handle.Result;
                
                if (instance == null)
                {
                    if (handle.IsValid())
                        Addressables.Release(handle);

                    throw new InvalidOperationException("[AssetProvider] Addressables.InstantiateAsync returned null instance.");
                }
                
                _instanceHandles[instance] = handle;
                return instance;
            }
            catch
            {
                if (handle.IsValid())
                    Addressables.Release(handle);

                throw;
            }
        }

        public void ReleaseInstance(GameObject go)
        {
            if (ReferenceEquals(go, null))
                return;

            var goLabel = go == null ? "<destroyed>" : go.name;

            if (_instanceHandles.Remove(go, out var handle))
            {
                if (handle.IsValid())
                    Addressables.ReleaseInstance(handle);
            }
            else
                Debug.LogWarning($"[AssetProvider] Trying to release unknown instance: {goLabel}");
        }

        public void Cleanup()
        {
            foreach (var handle in _loadedHandles)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }

            _loadedHandles.Clear();

            foreach (var kvp in _instanceHandles)
            {
                if (kvp.Value.IsValid())
                    Addressables.ReleaseInstance(kvp.Value);
            }

            _instanceHandles.Clear();
        }

        public void Dispose() => Cleanup();
    }
}
