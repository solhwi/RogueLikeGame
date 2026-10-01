using System.Collections;
using UnityEngine;

namespace RogueLike.Items
{
    // Addressable character assets must be preloaded once before
    // CharacterAssetComponent.Refresh() can resolve any of them
    // synchronously — mirrors CharacterDemo's own DemoScene.cs, which
    // demonstrated exactly this preload-then-refresh order.
    public class CharacterPreloader : MonoBehaviour
    {
        [SerializeField] private CharacterResourceSystem resourceSystem;
        [SerializeField] private CharacterAssetComponent assetComponent;

        private IEnumerator Start()
        {
            yield return resourceSystem.PreLoadAssets();
            assetComponent.Refresh();
        }
    }
}
