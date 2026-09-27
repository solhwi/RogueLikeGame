using System.Collections;
using UnityEngine;

namespace RogueLike.Combat
{
    // Blinks the sprite while Health's post-hit invulnerability window is
    // active, so getting hit is visible even though damage is blocked.
    [RequireComponent(typeof(Health))]
    public class DamageFlicker : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float flickerInterval = 0.08f;

        private Health health;
        private Coroutine flickerRoutine;

        private void Awake()
        {
            health = GetComponent<Health>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
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
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(flickerInterval);
            }

            spriteRenderer.enabled = true;
            flickerRoutine = null;
        }

        private void StopFlicker()
        {
            if (flickerRoutine != null)
            {
                StopCoroutine(flickerRoutine);
                flickerRoutine = null;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
        }
    }
}
