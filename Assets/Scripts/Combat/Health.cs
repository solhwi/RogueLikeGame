using System;
using UnityEngine;

namespace RogueLike.Combat
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private float invulnerabilityDuration = 1f;

        public event Action<int, int> OnHealthChanged; // current, max
        public event Action<Vector2> OnDamaged; // sourcePosition
        public event Action OnDied;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsInvulnerable { get; private set; }

        private float invulnerabilityTimer;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        private void Update()
        {
            if (!IsInvulnerable)
            {
                return;
            }

            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
            {
                IsInvulnerable = false;
            }
        }

        public void TakeDamage(int amount, Vector2 sourcePosition)
        {
            if (IsInvulnerable || CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            bool died = CurrentHealth <= 0;

            // Set before OnDamaged fires so listeners (e.g. DamageFlicker)
            // that synchronously check IsInvulnerable in response to the
            // event see it already true.
            if (!died)
            {
                IsInvulnerable = true;
                invulnerabilityTimer = invulnerabilityDuration;
            }

            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            OnDamaged?.Invoke(sourcePosition);

            if (died)
            {
                OnDied?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            IsInvulnerable = false;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        // Also used to (re)initialize a pooled instance's health when it's
        // spawned/reused, e.g. EnemyChaseAI.Initialize.
        public void SetMaxHealth(int value)
        {
            maxHealth = Mathf.Max(1, value);
            CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void SetCurrentHealth(int value)
        {
            CurrentHealth = Mathf.Clamp(value, 0, maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}
