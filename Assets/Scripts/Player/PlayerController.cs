using UnityEngine;
using RogueLike.Combat;
using RogueLike.Items;

namespace RogueLike.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float baseMoveSpeed = 4f;
        [SerializeField] private float acceleration = 40f;
        [SerializeField] private float deceleration = 50f;

        [Header("Knockback")]
        [SerializeField] private float knockbackForce = 6f;
        [SerializeField] private float knockbackDuration = 0.15f;

        private Rigidbody2D rb;
        private PlayerInputHandler input;
        private ItemSkillLoadout loadout;
        private Health health;
        private float knockbackTimer;

        public bool IsMoving { get; private set; }
        public Vector2 FacingDirection { get; private set; } = Vector2.down;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerInputHandler>();
            loadout = GetComponent<ItemSkillLoadout>();
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health.OnDamaged += HandleDamaged;
        }

        private void OnDisable()
        {
            health.OnDamaged -= HandleDamaged;
        }

        private void FixedUpdate()
        {
            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Time.fixedDeltaTime;
                IsMoving = rb.linearVelocity.sqrMagnitude > 0.01f;
                if (IsMoving)
                {
                    FacingDirection = rb.linearVelocity.normalized;
                }

                return;
            }

            float speedMultiplier = 1f + (loadout != null ? loadout.GetPassiveBonus(StatType.MoveSpeedMultiplier) : 0f);
            Vector2 targetVelocity = input.MoveInput * baseMoveSpeed * speedMultiplier;
            float rate = targetVelocity.sqrMagnitude > 0.01f ? acceleration : deceleration;

            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, rate * Time.fixedDeltaTime);

            IsMoving = rb.linearVelocity.sqrMagnitude > 0.01f;
            if (IsMoving)
            {
                FacingDirection = rb.linearVelocity.normalized;
            }
        }

        // Bumps the player away from the hit source on damage, as a quick
        // physical feedback cue. Overrides input-driven movement for
        // knockbackDuration so the acceleration-based FixedUpdate blend
        // above doesn't immediately cancel the impulse out.
        private void HandleDamaged(Vector2 sourcePosition)
        {
            Vector2 direction = (Vector2)transform.position - sourcePosition;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = -FacingDirection;
            }

            direction.Normalize();

            rb.linearVelocity = direction * knockbackForce;
            knockbackTimer = knockbackDuration;
        }
    }
}
