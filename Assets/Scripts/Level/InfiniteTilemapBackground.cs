using UnityEngine;
using UnityEngine.Tilemaps;

namespace RogueLike.Level
{
    // Keeps the ground tilemap filled around the camera so an Infinite
    // chapter never runs out of floor. Tiles are picked by hashing the cell
    // coordinate, so walking back to an area shows the same pattern even
    // though only the region near the camera is ever kept painted.
    [RequireComponent(typeof(Tilemap))]
    public class InfiniteTilemapBackground : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private TileBase[] tiles;
        [Tooltip("Relative pick chance per tile; empty or mismatched length means uniform.")]
        [SerializeField] private float[] weights;
        [Tooltip("Extra cells painted beyond each view edge.")]
        [SerializeField] private int margin = 4;
        [Tooltip("Painted area snaps to multiples of this, so repaints happen only every few cells of travel.")]
        [SerializeField] private int chunkSize = 8;

        private Tilemap tilemap;
        private BoundsInt painted;
        private bool hasPainted;

        private void Awake()
        {
            tilemap = GetComponent<Tilemap>();
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void LateUpdate()
        {
            if (targetCamera != null)
            {
                Refresh(targetCamera);
            }
        }

        // Public so the scene builder can pre-paint the area in edit mode.
        public void Refresh(Camera cam)
        {
            if (tilemap == null)
            {
                tilemap = GetComponent<Tilemap>();
            }
            if (tiles == null || tiles.Length == 0)
            {
                return;
            }

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            Vector3 center = cam.transform.position;
            Vector3Int viewMin = tilemap.WorldToCell(new Vector3(center.x - halfWidth, center.y - halfHeight, 0f));
            Vector3Int viewMax = tilemap.WorldToCell(new Vector3(center.x + halfWidth, center.y + halfHeight, 0f));

            if (hasPainted && Contains(painted, viewMin) && Contains(painted, viewMax))
            {
                return;
            }

            int chunk = Mathf.Max(1, chunkSize);
            int xMin = FloorToChunk(viewMin.x - margin, chunk);
            int yMin = FloorToChunk(viewMin.y - margin, chunk);
            int xMax = FloorToChunk(viewMax.x + margin, chunk) + chunk;
            int yMax = FloorToChunk(viewMax.y + margin, chunk) + chunk;
            var area = new BoundsInt(xMin, yMin, 0, xMax - xMin, yMax - yMin, 1);

            var block = new TileBase[area.size.x * area.size.y];
            for (int y = 0; y < area.size.y; y++)
            {
                for (int x = 0; x < area.size.x; x++)
                {
                    block[y * area.size.x + x] = PickTile(area.xMin + x, area.yMin + y);
                }
            }

            tilemap.ClearAllTiles();
            tilemap.SetTilesBlock(area, block);
            painted = area;
            hasPainted = true;
        }

        private TileBase PickTile(int x, int y)
        {
            float roll = (Hash(x, y) & 0xFFFFFF) / (float)0x1000000;

            if (weights == null || weights.Length != tiles.Length)
            {
                return tiles[Mathf.Min((int)(roll * tiles.Length), tiles.Length - 1)];
            }

            float total = 0f;
            foreach (float w in weights)
            {
                total += Mathf.Max(0f, w);
            }
            if (total <= 0f)
            {
                return tiles[0];
            }

            float target = roll * total;
            for (int i = 0; i < tiles.Length; i++)
            {
                target -= Mathf.Max(0f, weights[i]);
                if (target < 0f)
                {
                    return tiles[i];
                }
            }
            return tiles[tiles.Length - 1];
        }

        private static uint Hash(int x, int y)
        {
            unchecked
            {
                uint h = (uint)x * 73856093u ^ (uint)y * 19349663u;
                h ^= h >> 13;
                h *= 0x5bd1e995u;
                h ^= h >> 15;
                return h;
            }
        }

        private static int FloorToChunk(int value, int chunk)
        {
            return Mathf.FloorToInt((float)value / chunk) * chunk;
        }

        private static bool Contains(BoundsInt bounds, Vector3Int cell)
        {
            return cell.x >= bounds.xMin && cell.x < bounds.xMax && cell.y >= bounds.yMin && cell.y < bounds.yMax;
        }
    }
}
