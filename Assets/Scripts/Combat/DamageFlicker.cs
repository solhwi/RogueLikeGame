using System.Collections;
using UnityEngine;

namespace RogueLike.Combat
{
    // Blinks the visual while Health's post-hit invulnerability window is
    // active, so getting hit is visible even though damage is blocked.
    // Renderer rather than SpriteRenderer specifically: the player's visual
    // is a MeshRenderer (quad fed by CharacterRenderView) while enemies
    // still use a plain SpriteRenderer — both have Renderer.enabled, which
    // is all this needs.
    [RequireComponent(typeof(Health))]
    public class DamageFlicker : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private float flickerInterval = 0.08f;

        private Health health;
        private Coroutine flickerRoutine;

        private void Awake()
        {
            health = GetComponent<Health>();
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }
        }

        private void OnEnable()
        {
            health.OnDamaged += HandleDamaged;
        }

        private void OnDisable()
        {
            health.OnDamaged -= HandleDamaged;
            StopFlicker();
        }

        private void HandleDamaged(Vector2 sourcePosition)
        {
            StopFlicker();
            flickerRoutine = StartCoroutine(FlickerWhileInvulnerable());
        }

        private IEnumerator FlickerWhileInvulnerable()
        {
            while (health.IsInvulnerable)
            {
                targetRenderer.enabled = !targetRenderer.enabled;
                yield return new WaitForSeconds(flickerInterval);
            }

            targetRenderer.enabled = true;
            flickerRoutine = null;
        }

        private void StopFlicker()
        {
            if (flickerRoutine != null)
            {
                StopCoroutine(flickerRoutine);
                flickerRoutine = null;
            }

            if (targetRenderer != null)
            {
                targetRenderer.enabled = true;
            }
        }
    }
}
