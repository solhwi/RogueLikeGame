using UnityEngine;

namespace RogueLike.CharacterRendering
{
    // Ported from solhwi/SebamoGameClient as-is: a flat quad renderer whose
    // material gets its texture swapped at runtime (by ObjectView) instead
    // of being assigned once at import time, the way a SpriteRenderer's
    // sprite normally would be.
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class MeshRenderer2D : MonoBehaviour
    {
        [SerializeField] private string textureParameterName = "_MainTex";
        [SerializeField] private string colorParameterName = "_Color";
        [SerializeField] private string smoothnessParameterName = "_Glossiness";
        [SerializeField] private string cutOffParameterName = "_CutOff";

        private MeshRenderer meshRenderer;
        private Material mainMaterial;
        private float defaultLocalScaleX = 1f;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            mainMaterial = meshRenderer.material;

            defaultLocalScaleX = transform.localScale.x;

            SetSmoothness(0f);
            SetCutOff(0.1f);
        }

        public void SetSortingOrder(int order)
        {
            meshRenderer.sortingOrder = order;
        }

        public void SetTexture(Texture texture)
        {
            mainMaterial.SetTexture(textureParameterName, texture);
        }

        public void SetColor(Color color)
        {
            mainMaterial.SetColor(colorParameterName, color);
        }

        public void SetCutOff(float cutOff)
        {
            mainMaterial.SetFloat(cutOffParameterName, cutOff);
        }

        public void SetSmoothness(float smoothness)
        {
            mainMaterial.SetFloat(smoothnessParameterName, smoothness);
        }

        public void FlipX(bool flipX)
        {
            float localScaleX = flipX ? -defaultLocalScaleX : defaultLocalScaleX;
            transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
        }
    }
}
