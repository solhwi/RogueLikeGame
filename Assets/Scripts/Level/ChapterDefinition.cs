using UnityEngine;

namespace RogueLike.Level
{
    public enum ChapterMapType
    {
        Finite,
        Infinite,
        Vertical
    }

    [CreateAssetMenu(menuName = "Roguelike/Level/Chapter Definition", fileName = "NewChapter")]
    public class ChapterDefinition : ScriptableObject
    {
        [SerializeField] private string chapterId;
        [SerializeField] private ChapterMapType mapType;
        [SerializeField] private int energyCost = 5;
        [SerializeField] private WaveData waveData;
        [SerializeField] private Vector2 mapBoundsSize = new Vector2(20f, 20f);

        public string ChapterId => chapterId;
        public ChapterMapType MapType => mapType;
        public int EnergyCost => energyCost;
        public WaveData WaveData => waveData;
        public Vector2 MapBoundsSize => mapBoundsSize;
    }
}
