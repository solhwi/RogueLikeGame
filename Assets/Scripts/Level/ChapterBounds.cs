using UnityEngine;

namespace RogueLike.Level
{
    // Clamps the player inside the chapter's map: full box for Finite,
    // horizontal-only for Vertical, and left alone entirely for Infinite.
    public class ChapterBounds : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D playerBody;

        private ChapterMapType mapType;
        private Vector2 halfSize;
        private Vector2 center;

        public void Initialize(ChapterMapType type, Vector2 boundsSize, Vector2 boundsCenter)
        {
            mapType = type;
            halfSize = boundsSize * 0.5f;
            center = boundsCenter;
        }

        private void LateUpdate()
        {
            if (playerBody == null || mapType == ChapterMapType.Infinite)
            {
                return;
            }

            Vector2 position = playerBody.position;
            position.x = Mathf.Clamp(position.x, center.x - halfSize.x, center.x + halfSize.x);

            if (mapType == ChapterMapType.Finite)
            {
                position.y = Mathf.Clamp(position.y, center.y - halfSize.y, center.y + halfSize.y);
            }

            playerBody.position = position;
        }
    }
}
