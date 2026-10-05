using System.Collections;
using UnityEngine;
using RogueLike.CharacterRendering;

namespace RogueLike.Items
{
    // Addressable character assets must be preloaded once before
    // CharacterAssetComponent.Refresh() can resolve any of them
    // synchronously — mirrors CharacterDemo's own DemoScene.cs, which
    // demonstrated exactly this preload-then-refresh order. renderView.
    // Initialize() is what actually spawns Character.prefab under the
    // off-screen preview camera and triggers the first Refresh().
    public class CharacterPreloader : MonoBehaviour
    {
        [SerializeField] private CharacterResourceSystem resourceSystem;
        [SerializeField] private CharacterRenderView renderView;

        private IEnumerator Start()
        {
            yield return resourceSystem.PreLoadAssets();
            renderView.Initialize();
        }
    }
}
