using System.Collections;

namespace RogueLike.CharacterRendering
{
    // Wires ObjectView to the ported CharacterModule
    // (CharacterAssetComponent / CharacterAnimatorComponent, still in the
    // global namespace under Assets/Scripts/CharacterModule) instead of
    // SebamoGameClient's own CharacterDataSetter/CharacterAnimationController,
    // which this project doesn't have. originPrefab (set in the inspector)
    // should point at Assets/Bundles/Prefabs/Character.prefab.
    public class CharacterRenderView : ObjectView
    {
        private CharacterAssetComponent assetComponent;
        private CharacterAnimatorComponent animatorComponent;

        protected override IEnumerator OnPrepareRendering()
        {
            yield return base.OnPrepareRendering();

            if (originObj == null)
            {
                yield break;
            }

            assetComponent = originObj.GetComponentInChildren<CharacterAssetComponent>();
            animatorComponent = originObj.GetComponentInChildren<CharacterAnimatorComponent>();
            assetComponent?.Refresh();
        }

        // Called by CharacterAppearanceAdapter after changing an
        // equipped-item slot, so the new part actually shows up.
        public void Refresh()
        {
            assetComponent?.Refresh();
        }

        public void DoIdle(float crossFadeTime = 0f)
        {
            animatorComponent?.DoIdle(crossFadeTime);
        }

        public void DoRun(float crossFadeTime = 0f)
        {
            animatorComponent?.DoRun(crossFadeTime);
        }
    }
}
