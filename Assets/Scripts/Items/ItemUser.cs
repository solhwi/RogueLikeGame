using UnityEngine;
using RogueLike.Combat;
using RogueLike.Player;

namespace RogueLike.Items
{
    // The character instance an equipped item's behavior acts through, plus
    // the small amount of combat plumbing (targeting, passive stat lookups
    // from every OTHER equipped item) every item needs. Built once by
    // ItemSkillLoadout and handed to every ItemSkillBehavior call, so a new
    // item type never has to know how to find the player, its enemy layer,
    // or its stat bonuses on its own.
    public class ItemUser
    {
        public PlayerController Character { get; }
        public Transform Transform => Character.transform;
        public LayerMask EnemyLayer { get; }
        public ItemSkillLoadout Loadout { get; }

        // Null when the character has no CharacterAppearanceAdapter (e.g.
        // no 3D CharacterModule set up on this GameObject) — appearance
        // items no-op via the ?. in AppearanceItemDefinition rather than
        // every other item needing to care that this can be absent.
        public CharacterAppearanceAdapter Appearance { get; }

        public ItemUser(PlayerController character, LayerMask enemyLayer, ItemSkillLoadout loadout, CharacterAppearanceAdapter appearance)
        {
            Character = character;
            EnemyLayer = enemyLayer;
            Loadout = loadout;
            Appearance = appearance;
        }

        public Transform FindNearestEnemy(float range)
        {
            return AutoTargeting.FindNearestEnemy(Transform.position, range, EnemyLayer);
        }

        public float GetPassiveBonus(StatType type)
        {
            return Loadout != null ? Loadout.GetPassiveBonus(type) : 0f;
        }
    }
}
